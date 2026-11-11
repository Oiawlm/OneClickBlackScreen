using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

// Single point for the app's per-user state under HKCU\Software\BlackScreen
// (V10 D3/D7): the data-root resolution (registered DataDirectory when it
// carries the managed shape, otherwise %LOCALAPPDATA%\BlackScreen), the
// update-awareness marks the overlay reads, and the D4 migration pipeline the
// settings dialog drives. The direct LocalApplicationData concat lives here
// and nowhere else - MaskConfig's ini path and the settings page both resolve
// through this class. Registry access sits behind an injectable seam (the
// same offline-verifier pattern as Updater.Check's baseUrl): production reads
// the real key, VerifyPaths swaps in a -VERIFY-TEST key root before any call.
internal static class AppPaths
{
    // Frozen cross-track contract (RESEARCH-V10 §7): the key root and value
    // names are byte-identical on the B side (.iss [Registry]).
    public const string RegistryRoot = @"Software\BlackScreen";
    public const string ManagedDirectoryName = "BlackScreen";
    public const string DataDirectoryValueName = "DataDirectory";
    public const string DataParentValueName = "DataParentDirectory";
    public const string LastUpdateCheckValueName = "LastUpdateCheck";
    public const string UpdateAvailableValueName = "UpdateAvailable";
    public const string SettingsFileName = "settings.ini";

    // D7 throttle: one silent overlay check per day at most.
    private static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromHours(24);

    // HKCU string-value seam. HkcuStore talks to a real key path; verifiers
    // build one over a test key and inject it with UseRegistryStore.
    internal interface IRegistryStore
    {
        string GetString(string name); // null = absent or not a string
        void SetString(string name, string value);
        void DeleteValue(string name);
    }

    private static IRegistryStore store = new HkcuStore(RegistryRoot);

    internal static void UseRegistryStore(IRegistryStore seam)
    {
        store = seam;
    }

    internal static void UseDefaultRegistryStore()
    {
        store = new HkcuStore(RegistryRoot);
    }

    internal sealed class HkcuStore : IRegistryStore
    {
        private readonly string keyPath;

        internal HkcuStore(string keyPath)
        {
            this.keyPath = keyPath;
        }

        public string GetString(string name)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath))
                {
                    object value = key == null ? null : key.GetValue(name, null);
                    return value is string ? (string)value : null;
                }
            }
            catch
            {
                return null;
            }
        }

        public void SetString(string name, string value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                key.SetValue(name, value, RegistryValueKind.String);
            }
        }

        public void DeleteValue(string name)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                if (key != null) key.DeleteValue(name, false);
            }
        }
    }

    // The managed shape (frozen contract, B side mirrors this logic
    // byte-for-byte in its own IsManagedDataDirectory): a fully qualified
    // path ending in "\BlackScreen" (case-insensitive, no trailing
    // separator) whose parent is a real directory - not a drive root, not a
    // UNC share root, and not the current drive's root (a drive-less
    // "\BlackScreen" fails the parent==root check below).
    public static bool IsManagedDataRoot(string path)
    {
        if (path == null) return false;
        string trimmed = path.Trim();
        if (trimmed.Length == 0) return false;
        if (!trimmed.EndsWith("\\" + ManagedDirectoryName, StringComparison.OrdinalIgnoreCase)) return false;
        string parent = Path.GetDirectoryName(trimmed);
        if (string.IsNullOrEmpty(parent)) return false;
        return !string.Equals(parent, Path.GetPathRoot(parent), StringComparison.OrdinalIgnoreCase);
    }

    public static string DefaultDataRoot
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ManagedDirectoryName);
        }
    }

    // Registered root when its shape is legal, otherwise the default. A
    // registered-but-invalid value reads exactly like no registration at all
    // (fail-closed back to the default), never as an error.
    public static string DataRoot
    {
        get
        {
            string registered = store.GetString(DataDirectoryValueName);
            return IsManagedDataRoot(registered) ? registered : DefaultDataRoot;
        }
    }

    public static string SettingsIniPath
    {
        get { return Path.Combine(DataRoot, SettingsFileName); }
    }

    // Read-only prefill for the settings page (RESEARCH §7: the wizard page
    // on the B side is the writer of this value).
    public static string DataParentDirectory
    {
        get
        {
            string registered = store.GetString(DataParentValueName);
            return registered ?? "";
        }
    }

    // ---- update-awareness marks (D7) --------------------------------------
    // UpdateAvailable carries the newer release tag (or is absent); the mask
    // reads it at start - always a local read, offline-safe. LastUpdateCheck
    // carries the UTC stamp ("o" round-trip format) of the last completed
    // check; the overlay consults it before going online. Marks change hands
    // only through these members so the injected seam covers every write.

    public static string UpdateAvailableTag()
    {
        return store.GetString(UpdateAvailableValueName);
    }

    // Exactly the two-value update a completed check performs: stamp the
    // throttle clock and set (or clear) the hint tag. Callers reach here only
    // after a finished check - a failed or rate-limited check (403) leaves
    // both marks untouched, which is what keeps the overlay silent-failure
    // contract and the zero-footprint verify runs honest.
    public static void CompleteUpdateCheck(string updateTag)
    {
        store.SetString(LastUpdateCheckValueName,
            DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        if (updateTag == null) store.DeleteValue(UpdateAvailableValueName);
        else store.SetString(UpdateAvailableValueName, updateTag);
    }

    // Throttle gate: due only when a stamp exists and is at least a day old.
    // A missing stamp reads as dormant - a fresh machine stays offline until
    // some completed check (settings window finding a newer release, or an
    // applied update) first seeds the clock, so verifier-launched children on
    // an unmarked machine never touch the network or the real registry.
    public static bool IsUpdateCheckDue(DateTime utcNow)
    {
        string raw = store.GetString(LastUpdateCheckValueName);
        if (raw == null) return false;
        DateTime last;
        if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out last))
            return false;
        return utcNow - last.ToUniversalTime() >= UpdateCheckInterval;
    }

    // ---- migration pipeline (D4) -------------------------------------------

    // Commit a data root: the registration exists only while the root is not
    // the default (D3 zero-registration default state), so an explicit move
    // back to the default location deletes the value again.
    public static void CommitDataRoot(string dataRoot)
    {
        string normalized = Path.GetFullPath(dataRoot);
        if (string.CompareOrdinal(normalized, Path.GetFullPath(DefaultDataRoot)) == 0)
            store.DeleteValue(DataDirectoryValueName);
        else
            store.SetString(DataDirectoryValueName, normalized);
    }

    // What the settings dialog needs to know about a finished migration.
    // OldRootPath is set when the guarded delete declined to run (the caller
    // shows it so the user can clean up by hand).
    internal sealed class MigrationOutcome
    {
        public bool OldRootRemoved;
        public string OldRootPath;
    }

    // Copy the current data root to the new location, verify every copied
    // file byte-for-byte, then commit the registration - in that order, so
    // any failure before the commit leaves the old root and the registration
    // untouched (old data is never at risk). The guarded delete of the old
    // root runs last and only when the old directory carries the managed
    // shape and contains nothing beyond settings.ini; anything else stays for
    // a manual cleanup instead of a blind recursive delete.
    public static MigrationOutcome MigrateDataRoot(string newDataRoot)
    {
        string oldRoot = DataRoot; // resolved before any state changes
        string newRoot = Path.GetFullPath(newDataRoot);
        string normalizedOld = Path.GetFullPath(oldRoot);
        MigrationOutcome outcome = new MigrationOutcome();
        if (string.CompareOrdinal(newRoot, normalizedOld) == 0)
        {
            outcome.OldRootRemoved = true;
            return outcome;
        }
        Directory.CreateDirectory(newRoot);
        if (Directory.Exists(normalizedOld))
        {
            foreach (string file in Directory.GetFiles(normalizedOld))
            {
                string target = Path.Combine(newRoot, Path.GetFileName(file));
                File.Copy(file, target, true);
                VerifyBytesEqual(file, target);
            }
            foreach (string folder in Directory.GetDirectories(normalizedOld))
            {
                string target = Path.Combine(newRoot, Path.GetFileName(folder));
                CopyDirectory(folder, target);
            }
        }
        CommitDataRoot(newRoot);
        outcome.OldRootRemoved = TryGuardedDeleteOldRoot(normalizedOld);
        if (!outcome.OldRootRemoved) outcome.OldRootPath = normalizedOld;
        return outcome;
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (string file in Directory.GetFiles(source))
        {
            string targetFile = Path.Combine(target, Path.GetFileName(file));
            File.Copy(file, targetFile, true);
            VerifyBytesEqual(file, targetFile);
        }
        foreach (string folder in Directory.GetDirectories(source))
            CopyDirectory(folder, Path.Combine(target, Path.GetFileName(folder)));
    }

    // The delete guard: managed shape + contents limited to settings.ini
    // (empty included). Subdirectories or any other file mean the user put
    // something of their own there - the directory stays and the caller says
    // so. Best-effort by design: an IO hiccup declines, never throws.
    private static bool TryGuardedDeleteOldRoot(string oldRoot)
    {
        try
        {
            if (!IsManagedDataRoot(oldRoot)) return false;
            if (!Directory.Exists(oldRoot)) return true;
            foreach (string entry in Directory.GetFileSystemEntries(oldRoot))
            {
                if (File.Exists(entry) && string.Compare(Path.GetFileName(entry), SettingsFileName,
                    StringComparison.OrdinalIgnoreCase) == 0) continue;
                return false;
            }
            Directory.Delete(oldRoot, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void VerifyBytesEqual(string source, string copy)
    {
        const int Chunk = 64 * 1024;
        using (FileStream a = new FileStream(source, FileMode.Open, FileAccess.Read))
        using (FileStream b = new FileStream(copy, FileMode.Open, FileAccess.Read))
        {
            if (a.Length != b.Length)
                throw new IOException("迁移校验失败：文件大小不一致（" + Path.GetFileName(source) + "）。");
            byte[] left = new byte[Chunk];
            byte[] right = new byte[Chunk];
            while (true)
            {
                int readA = ReadFully(a, left);
                int readB = ReadFully(b, right);
                if (readA != readB) throw new IOException("迁移校验失败：文件内容不一致（" + Path.GetFileName(source) + "）。");
                if (readA == 0) return;
                for (int i = 0; i < readA; i++)
                    if (left[i] != right[i])
                        throw new IOException("迁移校验失败：文件内容不一致（" + Path.GetFileName(source) + "）。");
            }
        }
    }

    private static int ReadFully(FileStream stream, byte[] buffer)
    {
        int total = 0;
        while (total < buffer.Length)
        {
            int read = stream.Read(buffer, total, buffer.Length - total);
            if (read <= 0) break;
            total += read;
        }
        return total;
    }
}

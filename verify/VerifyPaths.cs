using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

// V10 VerifyPaths: the AppPaths single point end to end - the resolver matrix
// (registered shape vs default fallback), the managed-shape predicate, the
// update-awareness marks, and the D4 migration pipeline rehearsed in TEMP
// (copy -> verify -> commit -> guarded delete, plus failure atomicity). All
// registry state goes through the injected HkcuStore seam over
// Software\BlackScreen-VERIFY-TEST (created and removed in finally); the
// user's real Software\BlackScreen key and real settings.ini are snapshotted
// three times (start / after the test block / exit) and must stay identical.
internal static class VerifyPaths
{
    private const string TestKeyRoot = @"Software\BlackScreen-VERIFY-TEST";
    private const string TestKeyLeaf = "BlackScreen-VERIFY-TEST";

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static int Main(string[] args)
    {
        VerifyGate.Enter();
        try
        {
            Check(args.Length == 0, "Usage: VerifyPaths.exe");
            // Three-state snapshot #1: before anything this process does.
            string realBefore = SnapshotRealKey();
            string iniBefore = Sha256HexOrNull(RealIniPath());
            try
            {
                DeleteTestTree();
                AppPaths.UseRegistryStore(new AppPaths.HkcuStore(TestKeyRoot));
                try
                {
                    CheckManagedShapeMatrix();
                    CheckResolverMatrix();
                    CheckUpdateMarks();
                    CheckMigrationPipeline();
                }
                finally
                {
                    AppPaths.UseDefaultRegistryStore();
                }
                DeleteTestTree();
                Check(!RegistryKeyExists(TestKeyRoot), "test key tree still exists after cleanup");
                // Three-state snapshot #2: after the injected-seam block.
                string realAfter = SnapshotRealKey();
                Check(realAfter == realBefore,
                    "the real Software\\BlackScreen key changed during the test block");
                Check(Sha256HexOrNull(RealIniPath()) == iniBefore,
                    "the real settings.ini changed during the run (isolation leak)");
                Console.WriteLine("PASS Z: real Software\\BlackScreen key and real settings.ini "
                    + "identical across snapshots #1/#2 (three-state tripwire)");
                Console.WriteLine("PASS TOTAL: AppPaths resolver matrix + managed shape + update marks "
                    + "+ migration pipeline (TEMP rehearsal, guarded delete, failure atomicity)");
                return 0;
            }
            finally
            {
                // Three-state snapshot #3 (exit path) and best-effort cleanup;
                // throwing here keeps even this path from reporting success
                // with real user state disturbed.
                try { DeleteTestTree(); } catch { }
                if (SnapshotRealKey() != realBefore || Sha256HexOrNull(RealIniPath()) != iniBefore)
                    throw new Exception("real user state changed on the exit path (isolation leak)");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }

    // ---- 1. the managed-shape predicate (frozen §7 contract) ----------------

    private static void CheckManagedShapeMatrix()
    {
        Check(AppPaths.IsManagedDataRoot(@"D:\App\BlackScreen"), "deep parent + BlackScreen must pass");
        Check(!AppPaths.IsManagedDataRoot(@"D:\BlackScreen"), "a drive root parent must fail - data lives under a real parent directory");
        Check(!AppPaths.IsManagedDataRoot(@"\\server\share\BlackScreen"), "a UNC share root parent must fail - data lives under a real parent directory");
        Check(!AppPaths.IsManagedDataRoot(@"\BlackScreen"), "a drive-less rooted path must fail - the registration must be fully qualified");
        Check(AppPaths.IsManagedDataRoot("d:\\app\\blackscreen"), "suffix match must be case-insensitive");
        Check(!AppPaths.IsManagedDataRoot(@"D:\"), "a drive root must fail");
        Check(!AppPaths.IsManagedDataRoot(@"D:\BlackScreenX"), "a longer leaf name must fail");
        Check(!AppPaths.IsManagedDataRoot(@"D:\NotBlackScreen"), "an unrelated leaf must fail");
        Check(!AppPaths.IsManagedDataRoot(@"D:\App\BlackScreen\"), "a trailing separator must fail");
        Check(!AppPaths.IsManagedDataRoot(""), "empty must fail");
        Check(!AppPaths.IsManagedDataRoot(null), "null must fail");
        Check(AppPaths.IsManagedDataRoot(@"D:\App\BlackScreen "),
            "surrounding whitespace is trimmed, so the shape still passes");
        Console.WriteLine("PASS: managed shape matrix (suffix \\BlackScreen, case-insensitive, parent non-root, fully qualified, no trailing separator)");
    }

    // ---- 2. the resolver matrix (D3) ----------------------------------------

    private static void CheckResolverMatrix()
    {
        AppPaths.IRegistryStore store = new AppPaths.HkcuStore(TestKeyRoot);
        store.DeleteValue(AppPaths.DataDirectoryValueName);
        // No registration: the default root, ini under it, empty prefill.
        Check(AppPaths.DataRoot == AppPaths.DefaultDataRoot,
            "no registration must resolve to the default root");
        Check(AppPaths.SettingsIniPath == Path.Combine(AppPaths.DefaultDataRoot, "settings.ini"),
            "SettingsIniPath must be the default root's settings.ini");
        Check(AppPaths.DataParentDirectory.Length == 0, "no DataParentDirectory value must read empty");
        // Registered and legal: the registered root wins.
        store.SetString(AppPaths.DataDirectoryValueName, @"D:\Data\BlackScreen");
        Check(AppPaths.DataRoot == @"D:\Data\BlackScreen", "a legal registration must win");
        Check(AppPaths.SettingsIniPath == @"D:\Data\BlackScreen\settings.ini",
            "SettingsIniPath must follow the registered root");
        // Registered but illegal shapes: every one falls back to the default,
        // never errors (fail-closed back to default).
        string[] illegal = new string[]
        {
            @"D:\Data\NotBlackScreen",   // wrong leaf
            @"D:\BlackScreenX",          // longer suffix
            @"C:\",                      // drive root
            @"D:\App\BlackScreen\",      // trailing separator
            "  ",                        // whitespace only
        };
        foreach (string candidate in illegal)
        {
            store.SetString(AppPaths.DataDirectoryValueName, candidate);
            Check(AppPaths.DataRoot == AppPaths.DefaultDataRoot,
                "illegal registration must fall back to the default: " + candidate);
        }
        store.DeleteValue(AppPaths.DataDirectoryValueName);
        Check(AppPaths.DataRoot == AppPaths.DefaultDataRoot, "a deleted registration must read as default");
        // DataParentDirectory passes through when present (prefill read-only).
        store.SetString(AppPaths.DataParentValueName, @"E:\Somewhere");
        Check(AppPaths.DataParentDirectory == @"E:\Somewhere", "DataParentDirectory must pass through");
        store.DeleteValue(AppPaths.DataParentValueName);
        Console.WriteLine("PASS: resolver matrix (default / legal registration / illegal fallback / prefill passthrough)");
    }

    // ---- 3. update-awareness marks (D7) -------------------------------------

    private static void CheckUpdateMarks()
    {
        AppPaths.IRegistryStore store = new AppPaths.HkcuStore(TestKeyRoot);
        store.DeleteValue(AppPaths.LastUpdateCheckValueName);
        store.DeleteValue(AppPaths.UpdateAvailableValueName);
        DateTime now = DateTime.UtcNow;
        // Throttle: absent stamp = dormant (verifier children never go online);
        // garbage = dormant; fresh = not due; stale = due.
        Check(!AppPaths.IsUpdateCheckDue(now), "an absent stamp must read dormant");
        store.SetString(AppPaths.LastUpdateCheckValueName, "not-a-date");
        Check(!AppPaths.IsUpdateCheckDue(now), "a garbage stamp must read dormant");
        AppPaths.CompleteUpdateCheck("v9.9.9");
        string tag = AppPaths.UpdateAvailableTag();
        Check(tag == "v9.9.9", "CompleteUpdateCheck must store the tag, saw: " + tag);
        Check(!AppPaths.IsUpdateCheckDue(now), "a fresh stamp must not be due");
        string stale = now.Subtract(TimeSpan.FromHours(25)).ToString("o", CultureInfo.InvariantCulture);
        store.SetString(AppPaths.LastUpdateCheckValueName, stale);
        Check(AppPaths.IsUpdateCheckDue(DateTime.UtcNow), "a 25h-old stamp must be due");
        // Clearing half of the pair: CompleteUpdateCheck(null).
        AppPaths.CompleteUpdateCheck(null);
        Check(AppPaths.UpdateAvailableTag() == null, "CompleteUpdateCheck(null) must clear the tag");
        Check(!AppPaths.IsUpdateCheckDue(DateTime.UtcNow), "the refreshed stamp must not be due");
        store.DeleteValue(AppPaths.LastUpdateCheckValueName);
        store.DeleteValue(AppPaths.UpdateAvailableValueName);
        Console.WriteLine("PASS: update marks (throttle dormant/fresh/due, tag set/clear, round-trip)");
    }

    // ---- 4. migration pipeline rehearsal in TEMP (D4) ------------------------

    private static void CheckMigrationPipeline()
    {
        string work = Path.Combine(Path.GetTempPath(), "VerifyPaths-" + Path.GetRandomFileName());
        Directory.CreateDirectory(work);
        AppPaths.IRegistryStore store = new AppPaths.HkcuStore(TestKeyRoot);
        try
        {
            byte[] ini = Encoding.UTF8.GetBytes("color=#123456\r\ntext=V10\r\n");
            // Case 1 - happy path: settings.ini only, old root guard-deletes.
            string old1 = Path.Combine(work, "Old1", "BlackScreen");
            string new1 = Path.Combine(work, "New1", "BlackScreen");
            WriteRoot(old1, ini, null);
            store.SetString(AppPaths.DataDirectoryValueName, old1);
            AppPaths.MigrationOutcome outcome1 = AppPaths.MigrateDataRoot(new1);
            Check(File.Exists(Path.Combine(new1, "settings.ini"))
                && BytesEqual(File.ReadAllBytes(Path.Combine(new1, "settings.ini")), ini),
                "case 1: the migrated settings.ini must match byte-for-byte");
            Check(store.GetString(AppPaths.DataDirectoryValueName) == Path.GetFullPath(new1),
                "case 1: the registration must commit the new root");
            Check(!Directory.Exists(old1), "case 1: the settings-only old root must be guard-deleted");
            Check(outcome1.OldRootRemoved, "case 1: the outcome must report the old root removed");

            // Case 2 - extra content: copy and commit still happen, but the
            // guarded delete declines (content beyond settings.ini).
            string old2 = Path.Combine(work, "Old2", "BlackScreen");
            string new2 = Path.Combine(work, "New2", "BlackScreen");
            WriteRoot(old2, ini, "user-notes.txt");
            store.SetString(AppPaths.DataDirectoryValueName, old2);
            AppPaths.MigrationOutcome outcome2 = AppPaths.MigrateDataRoot(new2);
            Check(File.Exists(Path.Combine(new2, "settings.ini"))
                && BytesEqual(File.ReadAllBytes(Path.Combine(new2, "settings.ini")), ini),
                "case 2: the settings.ini must still migrate and verify");
            Check(store.GetString(AppPaths.DataDirectoryValueName) == Path.GetFullPath(new2),
                "case 2: the registration must still commit");
            Check(Directory.Exists(old2) && File.Exists(Path.Combine(old2, "user-notes.txt")),
                "case 2: the old root with extra content must survive");
            Check(!outcome2.OldRootRemoved && outcome2.OldRootPath == Path.GetFullPath(old2),
                "case 2: the outcome must report the surviving old root");

            // Case 3 - failure atomicity: a locked destination file aborts the
            // copy, the registration must stay on the old root, data intact.
            string old3 = Path.Combine(work, "Old3", "BlackScreen");
            string new3 = Path.Combine(work, "New3", "BlackScreen");
            WriteRoot(old3, ini, null);
            Directory.CreateDirectory(new3);
            string lockedTarget = Path.Combine(new3, "settings.ini");
            using (FileStream held = new FileStream(lockedTarget, FileMode.CreateNew,
                FileAccess.ReadWrite, FileShare.None))
            {
                store.SetString(AppPaths.DataDirectoryValueName, old3);
                try
                {
                    AppPaths.MigrateDataRoot(new3);
                    throw new Exception("case 3: the locked-destination migration did not fail");
                }
                catch (IOException)
                {
                }
                Check(store.GetString(AppPaths.DataDirectoryValueName) == Path.GetFullPath(old3),
                    "case 3: a failed migration must not commit the registration");
                Check(File.Exists(Path.Combine(old3, "settings.ini"))
                    && BytesEqual(File.ReadAllBytes(Path.Combine(old3, "settings.ini")), ini),
                    "case 3: the old root must stay intact after the failure");
            }

            // Case 4 - missing old root (fresh install): nothing to copy, the
            // commit still lands, no old-root deletion to report.
            string old4 = Path.Combine(work, "Old4", "BlackScreen");
            string new4 = Path.Combine(work, "New4", "BlackScreen");
            store.SetString(AppPaths.DataDirectoryValueName, old4);
            AppPaths.MigrationOutcome outcome4 = AppPaths.MigrateDataRoot(new4);
            Check(store.GetString(AppPaths.DataDirectoryValueName) == Path.GetFullPath(new4),
                "case 4: a missing old root must still commit the new registration");
            Check(outcome4.OldRootRemoved, "case 4: an absent old root must report as removed");

            // Case 5 - CommitDataRoot to the default location deletes the
            // registration entirely (D3 zero-registration default state).
            store.SetString(AppPaths.DataDirectoryValueName, new4);
            AppPaths.CommitDataRoot(AppPaths.DefaultDataRoot);
            Check(store.GetString(AppPaths.DataDirectoryValueName) == null,
                "case 5: committing the default root must delete the registration");
            Console.WriteLine("PASS: migration pipeline in TEMP (copy+verify+commit, guarded delete, "
                + "locked-target atomicity, fresh-install, zero-registration default)");
        }
        finally
        {
            try { Directory.Delete(work, true); } catch { }
            store.DeleteValue(AppPaths.DataDirectoryValueName);
        }
    }

    private static void WriteRoot(string root, byte[] ini, string extraFile)
    {
        Directory.CreateDirectory(root);
        File.WriteAllBytes(Path.Combine(root, "settings.ini"), ini);
        if (extraFile != null)
            File.WriteAllText(Path.Combine(root, extraFile), "keep me");
    }

    // ---- real-user-state tripwires (three-state snapshot) --------------------

    private static string RealIniPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BlackScreen", "settings.ini");
    }

    private static string Sha256HexOrNull(string path)
    {
        if (!File.Exists(path)) return null;
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(path))
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    // A stable, complete dump of the real key tree: subkey names and value
    // names+data, both sorted, so any write anywhere inside shows up as a
    // string difference. "<absent>" covers the never-created case.
    private static string SnapshotRealKey()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AppPaths.RegistryRoot))
        {
            if (key == null) return "<absent>";
            StringBuilder builder = new StringBuilder();
            AppendKey(builder, key, "");
            return builder.ToString();
        }
    }

    private static void AppendKey(StringBuilder builder, RegistryKey key, string prefix)
    {
        string[] names = key.GetValueNames();
        Array.Sort(names, StringComparer.Ordinal);
        foreach (string name in names)
            builder.Append(prefix).Append(name).Append('=')
                .AppendLine(Convert.ToString(key.GetValue(name, ""), CultureInfo.InvariantCulture));
        string[] subkeys = key.GetSubKeyNames();
        Array.Sort(subkeys, StringComparer.Ordinal);
        foreach (string subkey in subkeys)
        {
            using (RegistryKey child = key.OpenSubKey(subkey))
            {
                if (child != null) AppendKey(builder, child, prefix + subkey + "\\");
            }
        }
    }

    private static bool RegistryKeyExists(string path)
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
        {
            return key != null;
        }
    }

    private static void DeleteTestTree()
    {
        try
        {
            // DeleteSubKeyTree takes the child name relative to the opened
            // parent - the leaf, not the full path.
            using (RegistryKey parent = Registry.CurrentUser.OpenSubKey("Software", true))
            {
                if (parent != null)
                    parent.DeleteSubKeyTree(TestKeyLeaf, false);
            }
        }
        catch { }
    }

    private static bool BytesEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }
}

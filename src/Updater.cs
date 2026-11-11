using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Win32;

// Hand-written GitHub Releases updater (B1/B3-B6, R1): zero dependencies beyond
// the BCL, so the single-exe csc build stays intact. Check() reads the
// releases/latest API, DownloadAndVerify() fetches and validates the asset,
// ReplaceSequence() swaps the running exe via rename+copy - a running image can
// be renamed but not replaced in place (R1 rules out the in-place replace API).
// Every method is synchronous - the settings dialog drives them from the thread
// pool and marshals back with Control.Invoke; the mask mode never touches this
// class (U2: only the settings window ever goes online).
// V10 dual-form (D1/D2): Check() also resolves the Setup asset, DownloadStaged()
// predownloads the artifact this installation form consumes into
// %TEMP%\BlackScreenUpdate\, and LaunchSetup() hands the installed form over to
// the silent Inno reinstall. The portable self-replace chain below is untouched.
internal static class Updater
{
    // GitHub rejects default .NET user agents, so every request carries ours.
    private const string UserAgent = "OneClickBlackScreen-Updater/" + AppInfo.Version;

    // What the settings footer needs to know about a newer release. The exe
    // fields (DownloadUrl/Size/Sha256Hex) describe the portable self-replace
    // asset; the Setup fields describe the installer asset the installed form
    // consumes (V10 D2). Sha256Hex/SetupSha256Hex are the primary expectations
    // (assets[].digest, "sha256:<hex>"); SumsUrl is the shared SHA256SUMS
    // fallback asset; either may be null and the chain stays fail-closed (B5).
    internal sealed class UpdateInfo
    {
        public string Tag;
        public string DownloadUrl;
        public long Size;
        public string Sha256Hex;
        public string SumsUrl;
        public string SetupUrl;
        public long SetupSize;
        public string SetupSha256Hex;
    }

    // GET repos/{owner}/{repo}/releases/latest. Returns null when the release
    // tag does not read as newer than AppInfo.Version (malformed tags included,
    // fail-closed); throws on network, HTTP or payload errors so the caller can
    // surface a concrete reason.
    public static UpdateInfo Check(string baseUrl)
    {
        return Check(baseUrl, 0);
    }

    // Timeout overload (V10 D7): the overlay's throttled background check runs
    // with a few seconds of patience so a dead network can never hold a mask
    // hostage; zero keeps the WebClient default for the settings window.
    public static UpdateInfo Check(string baseUrl, int timeoutMs)
    {
        string url = baseUrl + "repos/" + AppInfo.RepoOwner + "/" + AppInfo.RepoName + "/releases/latest";
        string json;
        using (WebClient client = CreateClient(timeoutMs))
        {
            json = client.DownloadString(url);
        }
        Json root = Json.Parse(json);
        string tag = root.StringField("tag_name");
        if (tag == null) throw new InvalidDataException("服务器返回的数据缺少 tag_name。");
        if (!AppInfo.IsNewer(tag, AppInfo.CurrentVersion())) return null;
        Json assets = root.Field("assets");
        if (assets == null || assets.Items == null) throw new InvalidDataException("服务器返回的数据缺少 assets。");
        UpdateInfo info = new UpdateInfo();
        info.Tag = tag;
        foreach (Json asset in assets.Items)
        {
            string name = asset.StringField("name");
            if (name == AppInfo.UpdateAssetName)
            {
                info.DownloadUrl = asset.StringField("browser_download_url");
                info.Size = asset.NumberField("size", -1);
                info.Sha256Hex = ParseDigest(asset.StringField("digest"));
            }
            else if (name == SetupAssetNameFor(tag))
            {
                info.SetupUrl = asset.StringField("browser_download_url");
                info.SetupSize = asset.NumberField("size", -1);
                info.SetupSha256Hex = ParseDigest(asset.StringField("digest"));
            }
            else if (name == "SHA256SUMS")
            {
                info.SumsUrl = asset.StringField("browser_download_url");
            }
        }
        if (info.DownloadUrl == null)
            throw new InvalidDataException("最新发布未包含更新资产 " + AppInfo.UpdateAssetName + "。");
        return info;
    }

    // Phase 1 (worker thread): resolve the expected hash - assets[].digest
    // first, then the SHA256SUMS asset text; both absent means refuse (B5
    // fail-closed) - then download to a %TEMP% random name and check the
    // declared size and the SHA-256. Returns the verified temp path; the caller
    // owns deleting it.
    public static string DownloadAndVerify(UpdateInfo update, Action<int> onProgress)
    {
        string expected = update.Sha256Hex;
        if (expected == null && update.SumsUrl != null)
        {
            string sums;
            using (WebClient client = CreateClient())
            {
                sums = client.DownloadString(update.SumsUrl);
            }
            expected = ParseSha256Sums(sums, AppInfo.UpdateAssetName);
        }
        if (expected == null)
            throw new InvalidOperationException("官方发布未提供 SHA-256 校验和，已按安全策略拒绝更新。");
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            DownloadFile(update.DownloadUrl, temp, update.Size, onProgress);
            VerifySha256(temp, expected);
            return temp;
        }
        catch
        {
            try { if (File.Exists(temp)) File.Delete(temp); } catch { }
            throw;
        }
    }

    // Full chain in one call (download + verify + replace). The settings dialog
    // splits the halves instead so the replace runs on its UI thread.
    public static void DownloadAndApply(UpdateInfo update, string targetPath, Action<int> onProgress)
    {
        string temp = DownloadAndVerify(update, onProgress);
        try
        {
            ReplaceSequence(temp, targetPath);
        }
        finally
        {
            try { if (File.Exists(temp)) File.Delete(temp); } catch { }
        }
    }

    // ---- V10 dual-form update (D1/D2) ---------------------------------------
    // Installed form = the Setup asset silently reinstalled by Inno (which
    // remembers the install directory, closes the app and restarts the mask);
    // portable form = the exe self-replace chain above, byte-for-byte the V7
    // behavior. The preddownload staging below serves both: the artifact this
    // form consumes lands verified in %TEMP%\BlackScreenUpdate\ so the user's
    // "update now" click is instantaneous (D1).

    // Frozen cross-track contract (RESEARCH-V10 §7): the Setup release asset
    // name, mirroring build-setup.cmd's OutputBaseFilename (tag with the
    // leading "v" stripped).
    public static string SetupAssetNameFor(string tag)
    {
        string version = tag != null && tag.StartsWith("v", StringComparison.Ordinal)
            ? tag.Substring(1) : tag;
        return "OneClickBlackScreen-Setup-" + version + ".exe";
    }

    // The per-user Inno uninstall key (see UninstallKeyPath below): its
    // existence is the installed-form verdict (D2). Read-only HKCU probe
    // behind an injectable seam so the offline verifier can pin both states.
    public static bool IsInstalledForm
    {
        get { return installedFormProbe != null ? installedFormProbe() : ReadUninstallKeyPresent(); }
    }

    private static Func<bool> installedFormProbe;

    internal static void UseInstalledFormProbe(Func<bool> probe)
    {
        installedFormProbe = probe;
    }

    private static bool ReadUninstallKeyPresent()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(UninstallKeyPath))
            {
                return key != null;
            }
        }
        catch
        {
            return false;
        }
    }

    // Predownload staging directory (RESEARCH-V10 §7): one well-known place,
    // emptied before anything lands so no stale artifact can mix in, and the
    // verified file stays put until the one-click apply consumes it.
    public static string StagingDirectory
    {
        get { return Path.Combine(Path.GetTempPath(), "BlackScreenUpdate"); }
    }

    // Download the artifact this installation form consumes, verified, into
    // the staging directory (D1). The whole reset/download/verify span rides
    // the machine-wide update mutex so two settings instances cannot
    // interleave their staging writes; a failure cleans the directory again -
    // only a verified file is ever left staged.
    public static string DownloadStaged(UpdateInfo update, Action<int> onProgress)
    {
        bool installed = IsInstalledForm;
        string assetName = installed ? SetupAssetNameFor(update.Tag) : AppInfo.UpdateAssetName;
        using (Mutex mutex = new Mutex(false, UpdateMutexName))
        {
            bool owned = false;
            try
            {
                try { owned = mutex.WaitOne(UpdateMutexWaitMs); }
                catch (AbandonedMutexException) { owned = true; }
                if (!owned)
                    throw new IOException("另一更新正在进行，请稍后重试。");
                string directory = StagingDirectory;
                ResetDirectory(directory);
                try
                {
                    string target = Path.Combine(directory, assetName);
                    DownloadStagedAsset(update, installed, target, onProgress);
                    return target;
                }
                catch
                {
                    try { if (Directory.Exists(directory)) Directory.Delete(directory, true); } catch { }
                    throw;
                }
            }
            finally
            {
                if (owned) mutex.ReleaseMutex();
            }
        }
    }

    // One staged asset: resolve the expected hash for this form's asset name
    // (digest first, SHA256SUMS line fallback, double-missing refusal - the
    // same fail-closed chain as the portable path, B5), then download with
    // the declared size check and the SHA-256 verify, in place.
    private static void DownloadStagedAsset(UpdateInfo update, bool installed, string target, Action<int> onProgress)
    {
        string assetName = installed ? SetupAssetNameFor(update.Tag) : AppInfo.UpdateAssetName;
        string url = installed ? update.SetupUrl : update.DownloadUrl;
        long size = installed ? update.SetupSize : update.Size;
        if (url == null)
            throw new InvalidDataException("最新发布未包含"
                + (installed ? "安装包资产 " : "更新资产 ") + assetName + "。");
        string expected = installed ? update.SetupSha256Hex : update.Sha256Hex;
        if (expected == null && update.SumsUrl != null)
        {
            string sums;
            using (WebClient client = CreateClient())
            {
                sums = client.DownloadString(update.SumsUrl);
            }
            expected = ParseSha256Sums(sums, assetName);
        }
        if (expected == null)
            throw new InvalidOperationException("官方发布未提供 SHA-256 校验和，已按安全策略拒绝更新。");
        DownloadFile(url, target, size, onProgress);
        VerifySha256(target, expected);
    }

    // Empty the staging directory before a landing (best-effort: a file locked
    // by another process stays, and the verify still guards the new artifact),
    // then make sure the directory itself exists.
    private static void ResetDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        Directory.CreateDirectory(directory);
    }

    // Frozen cross-track contract (RESEARCH-V10 §7): the silent reinstall
    // parameters - no UI, no message boxes, no reboot, force-close the
    // running app instances so the overwrite install can proceed.
    public const string SetupSilentArguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS";

    // The launch facts (path + arguments) as one construct, so the offline
    // verifier can pin the frozen command line without ever running the
    // installer.
    internal static ProcessStartInfo SetupStartInfo(string setupPath)
    {
        return new ProcessStartInfo(setupPath, SetupSilentArguments);
    }

    // Installed-form apply (D2): launch the staged, verified Setup under the
    // update mutex - a short critical section around the spawn so two
    // instances cannot start two setups - then hand everything to Inno (B
    // track): it closes the app, reinstalls over the remembered directory and
    // restarts the mask. The caller exits this process right after.
    public static Process LaunchSetup(string setupPath)
    {
        using (Mutex mutex = new Mutex(false, UpdateMutexName))
        {
            bool owned = false;
            try
            {
                try { owned = mutex.WaitOne(UpdateMutexWaitMs); }
                catch (AbandonedMutexException) { owned = true; }
                if (!owned)
                    throw new IOException("另一更新正在进行，请稍后重试。");
                return Process.Start(SetupStartInfo(setupPath));
            }
            finally
            {
                if (owned) mutex.ReleaseMutex();
            }
        }
    }

    // Machine-wide exclusion for the on-disk swap (V7S): the settings window is
    // deliberately multi-instance, so two instances can reach this swap for the
    // same exe at the same time, and the backup the rollback chain depends on
    // is one well-known name - an unserialized second entrant deletes the first
    // one's live backup or double-replaces the target. The exe lives on a
    // machine-shared path (every user, every session), so the mutex is
    // Global\; the mask mutex stays Local\ because a mask per desktop is the
    // intended semantics (U3) - the asymmetry is first-principles correct.
    private const string UpdateMutexName = @"Global\BlackScreen.Update";

    // Bounded wait so a wedged holder can never freeze the settings UI forever.
    // The swap is two metadata ops plus one local copy, so a healthy holder
    // releases within milliseconds; reaching the bound means the other
    // instance is genuinely stuck and the waiter reports the busy failure.
    private const int UpdateMutexWaitMs = 10000;

    // Phase 2: swap the exe in three steps - remove any stale .old, rename the
    // running image to .old (renaming a running image is allowed; an in-place
    // replace would throw, R1), copy the verified file into place. A failure
    // after the rename rolls the backup back so the app keeps booting; the
    // .old left by a success is cleaned at the next Program.Main entry (B6).
    // The whole sequence - pre-clean, rename, copy and the rollback path -
    // runs under the update mutex (V7S). A waiter that acquires after a
    // concurrent instance already finished finds the target identical to the
    // downloaded bytes and returns as a no-op success: same end state and same
    // success the user asked for, without a second swap.
    internal static void ReplaceSequence(string downloadedPath, string targetPath)
    {
        using (Mutex mutex = new Mutex(false, UpdateMutexName))
        {
            bool owned = false;
            try
            {
                try { owned = mutex.WaitOne(UpdateMutexWaitMs); }
                catch (AbandonedMutexException) { owned = true; }
                if (!owned)
                    throw new IOException("另一更新正在进行，请稍后重试。");
                string downloadedSha = Sha256HexOrNull(downloadedPath);
                if (downloadedSha != null && downloadedSha == Sha256HexOrNull(targetPath)) return;
                string backup = targetPath + ".old";
                if (File.Exists(backup)) File.Delete(backup);
                try
                {
                    File.Move(targetPath, backup);
                }
                catch (Exception ex)
                {
                    throw new IOException("无法备份当前程序（" + ex.Message + "），未做任何改动。", ex);
                }
                try
                {
                    File.Copy(downloadedPath, targetPath, false);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (File.Exists(targetPath)) File.Delete(targetPath);
                        File.Move(backup, targetPath);
                    }
                    catch { }
                    throw new IOException("写入新版本失败（" + ex.Message + "），已还原原版本。", ex);
                }
                // With the new bytes on disk, refresh the per-user
                // "Add/Remove Programs" entry so its DisplayVersion cannot
                // lag behind the installed exe (V9 installer integration).
                SyncUninstallVersion(targetPath);
            }
            finally
            {
                if (owned) mutex.ReleaseMutex();
            }
        }
    }

    // The per-user Inno Setup uninstall key (HKCU, because the installer runs
    // PrivilegesRequired=lowest). The GUID must stay byte-identical to
    // RealAppId in setup\OneClickBlackScreen.iss - that script's comment
    // points back here. An AppId cannot be shared across an .iss and C#,
    // so each file carries the value exactly once and they cross-verify.
    private const string UninstallKeyPath =
        @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{5A17668F-45A0-48F0-B5B1-067E37EFA98D}_is1";

    // Sync "Add/Remove Programs" DisplayVersion after a completed swap, but
    // only when this copy is actually installed (key present). A portable
    // user has no HKCU entry, so the call reads as a no-op with zero
    // observable change - exactly what VerifyUpdate proves on a dev machine.
    // Best-effort on purpose: the swap already succeeded, so a registry
    // hiccup must never turn the update into a reported failure.
    private static void SyncUninstallVersion(string installedPath)
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(UninstallKeyPath, true))
            {
                if (key == null) return;
                string version = InstalledVersionOf(installedPath);
                if (version != null)
                    key.SetValue("DisplayVersion", version, RegistryValueKind.String);
            }
        }
        catch { }
    }

    // The only ground truth for the new version string is the file that was
    // just installed - the process running this code still carries the OLD
    // AppInfo.Version. AppInfo.Version is a compile-time const, so reading
    // its field through reflection copies the literal without executing any
    // code from the loaded assembly; anything unexpected reads as null and
    // skips the write rather than writing a value known to be stale.
    private static string InstalledVersionOf(string path)
    {
        try
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(path);
            System.Reflection.FieldInfo field = assembly.GetType("AppInfo").GetField("Version");
            return field != null && field.IsLiteral ? (string)field.GetValue(null) : null;
        }
        catch { return null; }
    }

    // B6 startup cleanup, riding the same mutex as the swap (V7S): a leftover
    // .old is only leftover when no swap is in flight, and an unserialized
    // startup delete could destroy a concurrent swap's live backup. The
    // zero-timeout probe is exactly the needed semantics - mutex free means no
    // swap is running so the delete is safe; mutex held means the .old belongs
    // to a live swap, so the deletion simply waits for the next start. Stays
    // best-effort like the inline delete it replaces.
    internal static void DeleteStaleBackup(string exePath)
    {
        try
        {
            using (Mutex mutex = new Mutex(false, UpdateMutexName))
            {
                bool owned = false;
                try
                {
                    try { owned = mutex.WaitOne(0); }
                    catch (AbandonedMutexException) { owned = true; }
                    if (owned) try { File.Delete(exePath + ".old"); } catch { }
                }
                finally
                {
                    if (owned) mutex.ReleaseMutex();
                }
            }
        }
        catch { }
    }

    // WebClient follows the 302 to the signed URL by itself (R1); the stream
    // copy exists only so progress can stream. A declared size that disagrees
    // with the bytes actually read is a rejection.
    private static void DownloadFile(string url, string target, long expectedSize, Action<int> onProgress)
    {
        using (WebClient client = CreateClient())
        using (Stream source = client.OpenRead(url))
        using (FileStream output = new FileStream(target, FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[64 * 1024];
            long copied = 0;
            while (true)
            {
                int read = source.Read(buffer, 0, buffer.Length);
                if (read <= 0) break;
                output.Write(buffer, 0, read);
                copied += read;
                if (onProgress != null && expectedSize > 0)
                    onProgress((int)Math.Min(99, copied * 100 / expectedSize));
            }
            if (expectedSize > 0 && copied != expectedSize)
                throw new IOException("下载不完整：应为 " + expectedSize + " 字节，实际 " + copied + " 字节。");
        }
        if (onProgress != null) onProgress(100);
    }

    // Streamed SHA-256 as lowercase hex; unreadable file reads as null. The
    // download verification turns null into the checksum failure below, and
    // the under-lock recheck treats null as "not a no-op" so the swap itself
    // surfaces the real error - one hashing path for both callers.
    private static string Sha256HexOrNull(string path)
    {
        try
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            }
        }
        catch { return null; }
    }

    private static void VerifySha256(string path, string expected)
    {
        if (string.CompareOrdinal(Sha256HexOrNull(path), expected) != 0)
            throw new InvalidOperationException("SHA-256 校验失败，下载文件与官方发布不一致，已拒绝更新。");
    }

    // assets[].digest reads "sha256:<hex>"; unknown algorithms or non-hex
    // bodies read as absent so the SHA256SUMS fallback and the double-missing
    // rejection stay in charge (B5).
    private static string ParseDigest(string digest)
    {
        if (digest == null) return null;
        const string Prefix = "sha256:";
        if (!digest.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) return null;
        string hex = digest.Substring(Prefix.Length).ToLowerInvariant();
        return IsSha256Hex(hex) ? hex : null;
    }

    // SHA256SUMS lines read "<hex>␣␣<name>" (certutil/GNU style); a leading "*"
    // on the name (GNU binary marker) is tolerated. Returns null when no line
    // matches the asked-for asset (B5 fail-closed). The asset name is a
    // parameter since V10: the file covers both the portable exe and the
    // Setup installer lines.
    private static string ParseSha256Sums(string sums, string assetName)
    {
        if (sums == null) return null;
        string[] lines = sums.Replace("\r\n", "\n").Split('\n');
        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (line.Length == 0) continue;
            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            string name = parts[1];
            if (name.StartsWith("*", StringComparison.Ordinal)) name = name.Substring(1);
            string hash = parts[0].ToLowerInvariant();
            if (name == assetName && IsSha256Hex(hash)) return hash;
        }
        return null;
    }

    private static bool IsSha256Hex(string text)
    {
        if (text == null || text.Length != 64) return false;
        for (int i = 0; i < 64; i++)
        {
            char c = text[i];
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        }
        return true;
    }

    // GitHub requires TLS 1.2+ and rejects default .NET user agents. This app
    // ships no supportedRuntime marker, so TLS 1.2 is enabled explicitly
    // instead of trusting the .NET 4.8 SystemDefault (B4); |= keeps any
    // stronger protocol the OS already negotiated. timeoutMs > 0 caps the
    // request patience (V10 D7 overlay check); zero keeps the default.
    private static WebClient CreateClient()
    {
        return CreateClient(0);
    }

    private static WebClient CreateClient(int timeoutMs)
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        WebClient client = timeoutMs > 0 ? new TimedWebClient(timeoutMs) : new WebClient();
        client.Headers[HttpRequestHeader.UserAgent] = UserAgent;
        client.Encoding = Encoding.UTF8;
        return client;
    }

    // WebClient hides the underlying request timeout; the overlay's
    // short-patience check (V10 D7) needs one, so this subclass threads a
    // deadline into every request it issues - the established BCL pattern
    // for WebClient timeouts.
    private sealed class TimedWebClient : WebClient
    {
        private readonly int timeoutMs;

        internal TimedWebClient(int timeoutMs)
        {
            this.timeoutMs = timeoutMs;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (timeoutMs > 0) request.Timeout = timeoutMs;
            return request;
        }
    }

    // Minimal JSON reader for the fixed releases/latest shape: objects, arrays,
    // strings, numbers and literals, with structural errors rejected. Not a
    // general-purpose parser - just a correct alternative to regex scraping or
    // a third-party dependency for one API payload.
    private sealed class Json
    {
        public string Str;
        public double Num;
        public List<Json> Items;
        public Dictionary<string, Json> Fields;

        public Json Field(string name)
        {
            if (Fields != null && Fields.ContainsKey(name)) return Fields[name];
            return null;
        }

        public string StringField(string name)
        {
            Json value = Field(name);
            return value != null && value.Str != null ? value.Str : null;
        }

        // Numbers are the only leaf without Str/Items/Fields; literals read 0
        // and never appear in the fields this reader consumes.
        public long NumberField(string name, long fallback)
        {
            Json value = Field(name);
            if (value == null || value.Str != null || value.Items != null || value.Fields != null) return fallback;
            return (long)value.Num;
        }

        public static Json Parse(string text)
        {
            if (text == null) throw new InvalidDataException("服务器返回了空数据。");
            int pos = 0;
            Json value = ReadValue(text, ref pos);
            SkipSpace(text, ref pos);
            if (pos != text.Length) throw new InvalidDataException("JSON 数据含多余内容。");
            return value;
        }

        private static Json ReadValue(string text, ref int pos)
        {
            SkipSpace(text, ref pos);
            if (pos >= text.Length) throw new InvalidDataException("JSON 数据不完整。");
            char c = text[pos];
            if (c == '{') return ReadObject(text, ref pos);
            if (c == '[') return ReadArray(text, ref pos);
            if (c == '"') return ReadString(text, ref pos);
            if (c == 't' || c == 'f' || c == 'n') return ReadLiteral(text, ref pos);
            if (c == '-' || (c >= '0' && c <= '9')) return ReadNumber(text, ref pos);
            throw new InvalidDataException("JSON 数据在位置 " + pos + " 无法解析。");
        }

        private static Json ReadObject(string text, ref int pos)
        {
            Json value = new Json();
            value.Fields = new Dictionary<string, Json>();
            pos++;
            SkipSpace(text, ref pos);
            if (pos < text.Length && text[pos] == '}')
            {
                pos++;
                return value;
            }
            while (true)
            {
                SkipSpace(text, ref pos);
                if (pos >= text.Length || text[pos] != '"')
                    throw new InvalidDataException("JSON 对象键必须是字符串。");
                Json key = ReadString(text, ref pos);
                SkipSpace(text, ref pos);
                if (pos >= text.Length || text[pos] != ':')
                    throw new InvalidDataException("JSON 对象缺少冒号。");
                pos++;
                Json item = ReadValue(text, ref pos);
                if (value.Fields.ContainsKey(key.Str))
                    throw new InvalidDataException("JSON 对象含重复键。");
                value.Fields.Add(key.Str, item);
                SkipSpace(text, ref pos);
                if (pos >= text.Length) throw new InvalidDataException("JSON 对象不完整。");
                if (text[pos] == ',') { pos++; continue; }
                if (text[pos] == '}') { pos++; return value; }
                throw new InvalidDataException("JSON 对象缺少逗号或右括号。");
            }
        }

        private static Json ReadArray(string text, ref int pos)
        {
            Json value = new Json();
            value.Items = new List<Json>();
            pos++;
            SkipSpace(text, ref pos);
            if (pos < text.Length && text[pos] == ']')
            {
                pos++;
                return value;
            }
            while (true)
            {
                value.Items.Add(ReadValue(text, ref pos));
                SkipSpace(text, ref pos);
                if (pos >= text.Length) throw new InvalidDataException("JSON 数组不完整。");
                if (text[pos] == ',') { pos++; continue; }
                if (text[pos] == ']') { pos++; return value; }
                throw new InvalidDataException("JSON 数组缺少逗号或右括号。");
            }
        }

        private static Json ReadString(string text, ref int pos)
        {
            pos++;
            StringBuilder builder = new StringBuilder();
            while (pos < text.Length)
            {
                char c = text[pos];
                if (c == '"')
                {
                    pos++;
                    Json value = new Json();
                    value.Str = builder.ToString();
                    return value;
                }
                if (c == '\\')
                {
                    pos++;
                    if (pos >= text.Length) throw new InvalidDataException("JSON 字符串未闭合。");
                    char escape = text[pos];
                    if (escape == 'u')
                    {
                        if (pos + 4 >= text.Length) throw new InvalidDataException("JSON \\u 转义不完整。");
                        int code = 0;
                        for (int i = 1; i <= 4; i++)
                        {
                            int digit = HexValue(text[pos + i]);
                            if (digit < 0) throw new InvalidDataException("JSON \\u 转义含非十六进制字符。");
                            code = code * 16 + digit;
                        }
                        builder.Append((char)code);
                        pos += 5;
                        continue;
                    }
                    char mapped;
                    switch (escape)
                    {
                        case '"': mapped = '"'; break;
                        case '\\': mapped = '\\'; break;
                        case '/': mapped = '/'; break;
                        case 'b': mapped = '\b'; break;
                        case 'f': mapped = '\f'; break;
                        case 'n': mapped = '\n'; break;
                        case 'r': mapped = '\r'; break;
                        case 't': mapped = '\t'; break;
                        default: throw new InvalidDataException("JSON 含未知转义。");
                    }
                    builder.Append(mapped);
                    pos++;
                    continue;
                }
                if (c < 0x20) throw new InvalidDataException("JSON 字符串含控制字符。");
                builder.Append(c);
                pos++;
            }
            throw new InvalidDataException("JSON 字符串未闭合。");
        }

        private static Json ReadNumber(string text, ref int pos)
        {
            int start = pos;
            if (text[pos] == '-') pos++;
            while (pos < text.Length && text[pos] >= '0' && text[pos] <= '9') pos++;
            if (pos < text.Length && text[pos] == '.')
            {
                pos++;
                while (pos < text.Length && text[pos] >= '0' && text[pos] <= '9') pos++;
            }
            if (pos < text.Length && (text[pos] == 'e' || text[pos] == 'E'))
            {
                pos++;
                if (pos < text.Length && (text[pos] == '+' || text[pos] == '-')) pos++;
                while (pos < text.Length && text[pos] >= '0' && text[pos] <= '9') pos++;
            }
            double number;
            if (!double.TryParse(text.Substring(start, pos - start), NumberStyles.Float,
                CultureInfo.InvariantCulture, out number))
                throw new InvalidDataException("JSON 数字无法解析。");
            Json value = new Json();
            value.Num = number;
            return value;
        }

        private static Json ReadLiteral(string text, ref int pos)
        {
            if (StartsWord(text, pos, "true")) { pos += 4; return new Json(); }
            if (StartsWord(text, pos, "false")) { pos += 5; return new Json(); }
            if (StartsWord(text, pos, "null")) { pos += 4; return new Json(); }
            throw new InvalidDataException("JSON 含未知字面量。");
        }

        private static bool StartsWord(string text, int pos, string word)
        {
            if (pos + word.Length > text.Length) return false;
            return string.CompareOrdinal(text.Substring(pos, word.Length), word) == 0;
        }

        private static int HexValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return -1;
        }

        private static void SkipSpace(string text, ref int pos)
        {
            while (pos < text.Length)
            {
                char c = text[pos];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n') pos++;
                else break;
            }
        }
    }
}

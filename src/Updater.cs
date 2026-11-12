using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

// Hand-written GitHub Releases updater (B1/B3-B6, R1): zero dependencies beyond
// the BCL, so the csc build stays intact. Single-form by contract (V11 D1/D4):
// the only release assets are the Setup installer and its SHA256SUMS - Check()
// resolves the Setup asset (a release without it fails loud and honest),
// DownloadStaged() predownloads it verified into %TEMP%\BlackScreenUpdate\, and
// LaunchSetup() hands over to the silent Inno reinstall; the portable
// self-replace chain is gone with the portable form. Every method is
// synchronous - the settings dialog drives them from the thread pool and
// marshals back with Control.Invoke; the mask mode never touches this class
// (U2: only the settings window ever goes online).
internal static class Updater
{
    // GitHub rejects default .NET user agents, so every request carries ours.
    private const string UserAgent = "OneClickBlackScreen-Updater/" + AppInfo.Version;

    // What the settings footer needs to know about a newer release: the Setup
    // installer asset plus the shared SHA256SUMS fallback asset (V11 D4 - the
    // single form). SetupSha256Hex is the primary expectation (assets[].digest,
    // "sha256:<hex>"); either hash source may be null and the chain stays
    // fail-closed (B5).
    internal sealed class UpdateInfo
    {
        public string Tag;
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
            if (name == SetupAssetNameFor(tag))
            {
                info.SetupUrl = asset.StringField("browser_download_url");
                info.SetupSize = asset.NumberField("size", -1);
                info.SetupSha256Hex = ParseDigest(asset.StringField("digest"));
            }
            else if (name == AppInfo.ChecksumsAssetName)
            {
                info.SumsUrl = asset.StringField("browser_download_url");
            }
        }
        if (info.SetupUrl == null)
            throw new InvalidDataException("最新发布未包含安装包资产 " + SetupAssetNameFor(tag) + "。");
        return info;
    }

    // ---- staged Setup update (V10 D1, V11 single form) ----------------------
    // The single artifact this app ever consumes is the Setup installer: the
    // predownload lands it verified in %TEMP%\BlackScreenUpdate\ so the user's
    // "update now" click is instantaneous, and the apply hands over to the
    // silent Inno reinstall (which remembers the install directory, closes the
    // app and restarts the mask).

    // Frozen cross-track contract (RESEARCH-V11 §7 table 1): the Setup release
    // asset name = the AppInfo prefix + the tag with the leading "v" stripped
    // + ".exe", mirroring the installer's OutputBaseFilename. The prefix is
    // single-sourced in AppInfo so the updater, the build script and the
    // release tooling can never drift.
    public static string SetupAssetNameFor(string tag)
    {
        string version = tag != null && tag.StartsWith("v", StringComparison.Ordinal)
            ? tag.Substring(1) : tag;
        return AppInfo.SetupAssetNamePrefix + version + ".exe";
    }

    // Predownload staging directory (RESEARCH-V11 §7): one well-known place,
    // emptied before anything lands so no stale artifact can mix in, and the
    // verified file stays put until the one-click apply consumes it.
    public static string StagingDirectory
    {
        get { return Path.Combine(Path.GetTempPath(), "BlackScreenUpdate"); }
    }

    // Download the Setup artifact, verified, into the staging directory (D1).
    // The whole reset/download/verify span rides the machine-wide update mutex
    // so two settings instances cannot interleave their staging writes; a
    // failure cleans the directory again - only a verified file is ever left
    // staged.
    public static string DownloadStaged(UpdateInfo update, Action<int> onProgress)
    {
        string assetName = SetupAssetNameFor(update.Tag);
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
                    DownloadStagedAsset(update, target, onProgress);
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

    // One staged asset: resolve the expected hash for the Setup asset name
    // (digest first, SHA256SUMS line fallback, double-missing refusal - the
    // same fail-closed chain as ever, B5), then download with the declared
    // size check and the SHA-256 verify, in place.
    private static void DownloadStagedAsset(UpdateInfo update, string target, Action<int> onProgress)
    {
        string assetName = SetupAssetNameFor(update.Tag);
        if (update.SetupUrl == null)
            throw new InvalidDataException("最新发布未包含安装包资产 " + assetName + "。");
        string expected = update.SetupSha256Hex;
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
        DownloadFile(update.SetupUrl, target, update.SetupSize, onProgress);
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

    // Apply (V10 D2, V11 single form): launch the staged, verified Setup under
    // the update mutex - a short critical section around the spawn so two
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

    // Machine-wide exclusion for the staged update (V7S, V11): the settings
    // window is deliberately multi-instance, so two instances can reach the
    // staging directory and the Setup launch at the same time - an
    // unserialized second entrant could interleave its staging writes into a
    // verified file or start a second installer. The staging path and the
    // install directory live on machine-shared paths (every user, every
    // session), so the mutex is Global\; the mask mutex stays Local\ because
    // a mask per desktop is the intended semantics (U3) - the asymmetry is
    // first-principles correct.
    private const string UpdateMutexName = @"Global\BlackScreen.Update";

    // Bounded wait so a wedged holder can never freeze the settings UI forever.
    // The staged work is a directory reset plus one local download, so a
    // healthy holder releases within moments; reaching the bound means the
    // other instance is genuinely stuck and the waiter reports the busy
    // failure.
    private const int UpdateMutexWaitMs = 10000;

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
    // download verification turns null into the checksum failure below - one
    // hashing path for every caller.
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

    // SHA256SUMS lines read "<hex>␣*<name>" (the frozen release format, GNU
    // binary marker style); plain "<hex>␣␣<name>" is tolerated too. Returns
    // null when no line matches the asked-for asset (B5 fail-closed). The
    // asset name is a parameter: the SUMS file covers every release asset's
    // line.
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

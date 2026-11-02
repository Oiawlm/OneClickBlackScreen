using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

// Hand-written GitHub Releases updater (B1/B3-B6, R1): zero dependencies beyond
// the BCL, so the single-exe csc build stays intact. Check() reads the
// releases/latest API, DownloadAndVerify() fetches and validates the asset,
// ReplaceSequence() swaps the running exe via rename+copy - a running image can
// be renamed but not replaced in place (R1 rules out the in-place replace API).
// Every method is synchronous - the settings dialog drives them from the thread
// pool and marshals back with Control.Invoke; the mask mode never touches this
// class (U2: only the settings window ever goes online).
internal static class Updater
{
    // GitHub rejects default .NET user agents, so every request carries ours.
    private const string UserAgent = "OneClickBlackScreen-Updater/" + AppInfo.Version;

    // What the settings footer needs to know about a newer release. Sha256Hex
    // is the primary expectation (assets[].digest, "sha256:<hex>"); SumsUrl is
    // the SHA256SUMS fallback asset; either may be null and the chain stays
    // fail-closed (B5).
    internal sealed class UpdateInfo
    {
        public string Tag;
        public string DownloadUrl;
        public long Size;
        public string Sha256Hex;
        public string SumsUrl;
    }

    // GET repos/{owner}/{repo}/releases/latest. Returns null when the release
    // tag does not read as newer than AppInfo.Version (malformed tags included,
    // fail-closed); throws on network, HTTP or payload errors so the caller can
    // surface a concrete reason.
    public static UpdateInfo Check(string baseUrl)
    {
        string url = baseUrl + "repos/" + AppInfo.RepoOwner + "/" + AppInfo.RepoName + "/releases/latest";
        string json;
        using (WebClient client = CreateClient())
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
            expected = ParseSha256Sums(sums);
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

    // Phase 2: swap the exe in three steps - remove any stale .old, rename the
    // running image to .old (renaming a running image is allowed; an in-place
    // replace would throw, R1), copy the verified file into place. A failure
    // after the rename rolls the backup back so the app keeps booting; the
    // .old left by a success is cleaned at the next Program.Main entry (B6).
    internal static void ReplaceSequence(string downloadedPath, string targetPath)
    {
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

    private static void VerifySha256(string path, string expected)
    {
        string actual;
        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (SHA256 sha = SHA256.Create())
        {
            actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        if (string.CompareOrdinal(actual, expected) != 0)
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
    // matches the update asset (B5 fail-closed).
    private static string ParseSha256Sums(string sums)
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
            if (name == AppInfo.UpdateAssetName && IsSha256Hex(hash)) return hash;
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
    // stronger protocol the OS already negotiated.
    private static WebClient CreateClient()
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        WebClient client = new WebClient();
        client.Headers[HttpRequestHeader.UserAgent] = UserAgent;
        client.Encoding = Encoding.UTF8;
        return client;
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

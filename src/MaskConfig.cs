using System;
using System.Drawing;
using System.IO;
using System.Text;

// Immutable mask customization settings: the overlay color and the overlay text.
// Persisted as two "key=value" lines; any known key with an invalid value makes the
// whole file fall back to the defaults (pure black, no text = v1 behavior).
internal sealed class MaskConfig
{
    public const int MaxTextLength = 200;
    private const long MaxFileBytes = 65536;

    public Color MaskColor { get; private set; }
    public string MaskText { get; private set; }

    public MaskConfig()
    {
        MaskColor = Color.Black;
        MaskText = "";
    }

    public MaskConfig(Color color, string text)
    {
        MaskColor = Color.FromArgb(255, color.R, color.G, color.B);
        MaskText = text ?? "";
    }

    public static string SettingsPath
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BlackScreen", "settings.ini");
        }
    }

    public static MaskConfig Load()
    {
        return Load(SettingsPath);
    }

    public static MaskConfig Load(string path)
    {
        try
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > MaxFileBytes) return new MaskConfig();
                byte[] raw = new byte[stream.Length];
                int total = 0;
                while (total < raw.Length)
                {
                    int read = stream.Read(raw, total, raw.Length - total);
                    if (read <= 0) break;
                    total += read;
                }
                if (total > MaxFileBytes) return new MaskConfig();
                int start = total >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF ? 3 : 0;
                string content = new UTF8Encoding(false, true).GetString(raw, start, total - start);
                return Parse(content);
            }
        }
        catch
        {
            return new MaskConfig();
        }
    }

    private static MaskConfig Parse(string content)
    {
        Color color = Color.Black;
        string text = "";
        string[] lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');
            if (line.Trim().Length == 0) continue;
            int eq = line.IndexOf('=');
            if (eq < 0) continue;
            string key = line.Substring(0, eq).Trim();
            string value = line.Substring(eq + 1);
            if (key == "color")
            {
                Color parsed;
                if (!TryParseColor(value.Trim(), out parsed)) return new MaskConfig();
                color = parsed;
            }
            else if (key == "text")
            {
                string candidate = Unescape(value);
                if (candidate.Length > MaxTextLength) return new MaskConfig();
                text = candidate;
            }
        }
        return new MaskConfig(color, text);
    }

    private static bool TryParseColor(string value, out Color color)
    {
        color = Color.Black;
        if (value.Length != 7 || value[0] != '#') return false;
        int red, green, blue;
        if (!TryHex(value[1], value[2], out red)) return false;
        if (!TryHex(value[3], value[4], out green)) return false;
        if (!TryHex(value[5], value[6], out blue)) return false;
        color = Color.FromArgb(red, green, blue);
        return true;
    }

    private static bool TryHex(char high, char low, out int value)
    {
        int h = HexValue(high), l = HexValue(low);
        value = 0;
        if (h < 0 || l < 0) return false;
        value = h * 16 + l;
        return true;
    }

    private static int HexValue(char digit)
    {
        if (digit >= '0' && digit <= '9') return digit - '0';
        if (digit >= 'a' && digit <= 'f') return digit - 'a' + 10;
        if (digit >= 'A' && digit <= 'F') return digit - 'A' + 10;
        return -1;
    }

    public static string NormalizeNewlines(string text)
    {
        return (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private static string Escape(string text)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\\') builder.Append("\\\\");
            else if (c == '\n') builder.Append("\\n");
            else builder.Append(c);
        }
        return builder.ToString();
    }

    private static string Unescape(string value)
    {
        StringBuilder builder = new StringBuilder();
        int i = 0;
        while (i < value.Length)
        {
            char c = value[i];
            if (c == '\\' && i + 1 < value.Length)
            {
                char next = value[i + 1];
                if (next == '\\') { builder.Append('\\'); i += 2; continue; }
                if (next == 'n') { builder.Append('\n'); i += 2; continue; }
            }
            builder.Append(c);
            i++;
        }
        return builder.ToString();
    }

    public void Save()
    {
        Save(SettingsPath);
    }

    public void Save(string path)
    {
        string directory = Path.GetDirectoryName(Path.GetFullPath(path));
        Directory.CreateDirectory(directory);
        string body = "color=#" + MaskColor.R.ToString("X2") + MaskColor.G.ToString("X2") + MaskColor.B.ToString("X2")
            + "\r\ntext=" + Escape(NormalizeNewlines(MaskText)) + "\r\n";
        string temp = Path.Combine(directory, Path.GetRandomFileName());
        try
        {
            using (FileStream stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body);
                stream.Write(bytes, 0, bytes.Length);
            }
            if (File.Exists(path)) File.Replace(temp, path, null);
            else
            {
                try { File.Move(temp, path); }
                catch (IOException) { File.Replace(temp, path, null); }
            }
        }
        finally
        {
            try { if (File.Exists(temp)) File.Delete(temp); }
            catch { }
        }
    }
}

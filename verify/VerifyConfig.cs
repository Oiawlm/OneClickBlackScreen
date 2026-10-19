using System;
using System.Drawing;
using System.IO;
using System.Text;

// Config verifier: exercises MaskConfig load/save against temp paths only
// (never touches the real settings.ini). Links src\MaskConfig.cs directly.
internal static class VerifyConfig
{
    private static string root;

    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    private static string NewDir()
    {
        string dir = Path.Combine(root, Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string ConfigOf(string dir)
    { return Path.Combine(dir, "settings.ini"); }

    private static void WriteAscii(string path, string text)
    { File.WriteAllBytes(path, Encoding.ASCII.GetBytes(text)); }

    private static void CheckSame(MaskConfig loaded, Color color, string text, string what)
    {
        Check(loaded.MaskColor.ToArgb() == color.ToArgb(),
            what + ": color mismatch, got " + loaded.MaskColor);
        Check(loaded.MaskText == text,
            what + ": text mismatch, got <" + loaded.MaskText + ">");
    }

    [STAThread]
    private static int Main(string[] args)
    {
        int passed = 0;
        Check(args.Length == 0 || args.Length == 1, "Usage: VerifyConfig.exe");
        try
        {
            root = Path.Combine(Path.GetTempPath(), "VerifyConfig_" + DateTime.Now.Ticks.ToString("x"));
            if (Directory.Exists(root)) Directory.Delete(root, true);
            Directory.CreateDirectory(root);

            // 1. Missing file falls back to defaults.
            CheckSame(MaskConfig.Load(ConfigOf(NewDir())), Color.Black, "", "missing-file");

            // 2. Roundtrip through the real save/load pair, non-ASCII included.
            string dir = NewDir();
            MaskConfig original = new MaskConfig(Color.FromArgb(0x33, 0x66, 0xCC), "a\\b\ncA1\u4e2d\u6587");
            original.Save(ConfigOf(dir));
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(0x33, 0x66, 0xCC), "a\\b\ncA1\u4e2d\u6587", "roundtrip");

            // 3. Save writes the documented file format.
            dir = NewDir();
            new MaskConfig(Color.FromArgb(0x33, 0x66, 0xCC), "T").Save(ConfigOf(dir));
            Check(File.ReadAllText(ConfigOf(dir)) == "color=#3366CC\r\ntext=T\r\n",
                "save-format: unexpected file content: " + File.ReadAllText(ConfigOf(dir)));

            // 4. Golden sample with CRLF endings and escapes.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "color=#010203\r\ntext=Line1\\nLine2\\\\End\r\n");
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(0x01, 0x02, 0x03),
                "Line1\nLine2\\End", "golden-crlf");

            // 5. Golden sample with LF-only endings.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "color=#0A0B0C\ntext=Hi");
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(0x0A, 0x0B, 0x0C), "Hi", "golden-lf");

            // 6. Invalid known value makes the WHOLE file fall back to defaults.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "color=xyz\r\ntext=Keep");
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.Black, "", "invalid-color-whole-default");

            // 7. Oversized text also resets the whole file (valid color must not survive).
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "color=#112233\r\ntext=" + new string('A', 201));
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.Black, "", "oversize-text-whole-default");

            // 8. Text at the limit is accepted.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "color=#112233\r\ntext=" + new string('A', 200));
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(0x11, 0x22, 0x33), new string('A', 200), "limit-text");

            // 9. Unknown keys are ignored, known keys survive.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "foo=bar\r\ncolor=#0A0B0C\r\n");
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(0x0A, 0x0B, 0x0C), "", "unknown-keys");

            // 10. Duplicate keys: last value wins.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "color=#111111\r\ncolor=#222222\r\n");
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(0x22, 0x22, 0x22), "", "duplicate-last-wins");

            // 11. Lowercase hex accepted and normalized on save.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "color=#abcdef\r\n");
            MaskConfig loaded = MaskConfig.Load(ConfigOf(dir));
            CheckSame(loaded, Color.FromArgb(0xAB, 0xCD, 0xEF), "", "lowercase-hex");
            loaded.Save(ConfigOf(dir));
            Check(File.ReadAllText(ConfigOf(dir)) == "color=#ABCDEF\r\ntext=\r\n", "lowercase-hex: save not normalized");

            // 12. File over the 64KB read cap falls back to defaults.
            dir = NewDir();
            byte[] big = new byte[70000];
            for (int i = 0; i < big.Length; i++) big[i] = (byte)'A';
            File.WriteAllBytes(ConfigOf(dir), big);
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.Black, "", "file-cap-64k");

            // 13. Invalid UTF-8 bytes fall back to defaults.
            dir = NewDir();
            File.WriteAllBytes(ConfigOf(dir), new byte[] { 0xFF, 0xFE, 0x41 });
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.Black, "", "invalid-utf8");

            // 14. UTF-8 BOM is accepted.
            dir = NewDir();
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            byte[] body = Encoding.ASCII.GetBytes("color=#040506\r\n");
            File.WriteAllBytes(ConfigOf(dir), Concat(bom, body));
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(0x04, 0x05, 0x06), "", "bom-accepted");

            // 15. Empty text value means no text.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "color=#070809\r\ntext=\r\n");
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(0x07, 0x08, 0x09), "", "empty-text");

            // 16. Escape edge cases: trailing backslash and unknown escape stay literal.
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "text=End\\");
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.Black, "End\\", "trailing-backslash");
            dir = NewDir();
            WriteAscii(ConfigOf(dir), "text=A\\tB");
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.Black, "A\\tB", "unknown-escape");

            // 17. Saving twice replaces atomically and leaves no temp files behind.
            dir = NewDir();
            new MaskConfig(Color.FromArgb(1, 2, 3), "one").Save(ConfigOf(dir));
            new MaskConfig(Color.FromArgb(4, 5, 6), "two").Save(ConfigOf(dir));
            CheckSame(MaskConfig.Load(ConfigOf(dir)), Color.FromArgb(4, 5, 6), "two", "atomic-overwrite");
            Check(Directory.GetFiles(dir).Length == 1, "atomic-overwrite: leftover files in dir");

            // 18. Saving creates the target directory when missing.
            string nested = Path.Combine(NewDir(), "sub", "deeper");
            string nestedConfig = Path.Combine(nested, "settings.ini");
            new MaskConfig(Color.FromArgb(9, 8, 7), "nested").Save(nestedConfig);
            CheckSame(MaskConfig.Load(nestedConfig), Color.FromArgb(9, 8, 7), "nested", "save-creates-dir");

            passed = 18;
            Console.WriteLine("PASS: 18 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL after " + passed + "/18 config tests: " + ex.Message);
            return 1;
        }
        finally
        {
            try { if (Directory.Exists(root)) Directory.Delete(root, true); }
            catch { }
        }
    }

    private static byte[] Concat(byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];
        Array.Copy(first, result, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);
        return result;
    }
}

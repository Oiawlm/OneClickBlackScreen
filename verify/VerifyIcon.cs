using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

// Icon verifier: proves the committed .ico artifacts are structurally valid, that
// the exe embeds the main icon's 256px PNG payload, that the settings.ico managed
// resource is byte-identical and decodable, and that dist carries the deployed
// copy (V11). Read-only: no system state is touched.
internal static class VerifyIcon
{
    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    private sealed class Entry
    {
        public int Declared;
        public long Offset;
        public long Length;
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Check(args.Length == 1, "Usage: VerifyIcon.exe <exe>");
            string exe = Path.GetFullPath(args[0]);
            string dist = Path.GetDirectoryName(exe);
            string repo = Path.GetDirectoryName(dist);
            string mainIco = Path.Combine(repo, "assets", "icon-main.ico");
            string settingsIco = Path.Combine(repo, "assets", "icon-settings.ico");
            string distIco = Path.Combine(dist, "icon-settings.ico");

            // 1. Both .ico files parse: 7 tiers, exact sizes, in-bounds sequential
            //    entries, every entry decodable (DIB full-pixel or PNG).
            List<Entry> mainEntries = ParseIcon(mainIco);
            List<Entry> settingsEntries = ParseIcon(settingsIco);
            Console.WriteLine("PASS: icon-main.ico structure ok (7 tiers, in-bounds, all entries decodable)");
            Console.WriteLine("PASS: icon-settings.ico structure ok (7 tiers, in-bounds, all entries decodable)");

            // 2. The exe embeds icon-main's 256px PNG payload bytes (positive proof
            //    that the win32icon is ours; it does not by itself prove the whole
            //    group - the resource table is covered by check 3's sibling build).
            byte[] exeBytes = File.ReadAllBytes(exe);
            byte[] mainBytes = File.ReadAllBytes(mainIco);
            Entry png256 = mainEntries.Find(delegate(Entry e) { return e.Declared == 256; });
            Check(png256 != null, "icon-main.ico has no 256 tier");
            long found = IndexOf(exeBytes, Slice(mainBytes, png256.Offset, png256.Length));
            Check(found >= 0, "exe does not contain the icon-main 256px PNG payload");
            Console.WriteLine("PASS: exe embeds icon-main 256px PNG payload (offset " + found + ")");

            // 3. The settings.ico managed resource: byte-identical to the artifact
            //    and decodable as an icon.
            byte[] settingsBytes = File.ReadAllBytes(settingsIco);
            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(exe);
            bool named = false;
            foreach (string name in assembly.GetManifestResourceNames())
                if (name == "settings.ico") named = true;
            Check(named, "exe has no managed resource named settings.ico");
            using (Stream stream = assembly.GetManifestResourceStream("settings.ico"))
            {
                Check(stream != null, "settings.ico resource stream is null");
                byte[] embedded = ReadAll(stream);
                Check(embedded.Length == settingsBytes.Length,
                    "settings.ico resource length " + embedded.Length + " != artifact " + settingsBytes.Length);
                for (int i = 0; i < embedded.Length; i++)
                    Check(embedded[i] == settingsBytes[i], "settings.ico resource differs at byte " + i);
                using (MemoryStream decode = new MemoryStream(embedded))
                using (Icon icon = new Icon(decode))
                    Check(icon.Width > 0 && icon.Height > 0, "settings.ico resource did not decode");
            }
            Console.WriteLine("PASS: settings.ico managed resource byte-identical to artifact and decodable");

            // 4. dist carries the deployed settings icon, byte-identical.
            Check(File.Exists(distIco), "dist\\icon-settings.ico missing");
            byte[] deployed = File.ReadAllBytes(distIco);
            Check(deployed.Length > 0 && deployed.Length == settingsBytes.Length,
                "dist\\icon-settings.ico size mismatch");
            for (int i = 0; i < deployed.Length; i++)
                Check(deployed[i] == settingsBytes[i], "dist\\icon-settings.ico differs at byte " + i);
            Console.WriteLine("PASS: dist\\icon-settings.ico deployed byte-identical (" + deployed.Length + " bytes)");

            Console.WriteLine("PASS TOTAL: 4 icon checks");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }

    private static List<Entry> ParseIcon(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Check(bytes.Length >= 6, path + " too short for ICONDIR");
        Check(ReadInt16(bytes, 0) == 0, path + " idReserved != 0");
        Check(ReadInt16(bytes, 2) == 1, path + " idType != 1");
        int count = ReadInt16(bytes, 4);
        Check(count == 7, path + " entry count " + count + " != 7");
        long headerEnd = 6 + 16L * count;
        Check(bytes.Length >= headerEnd, path + " shorter than its directory");
        List<Entry> entries = new List<Entry>();
        List<int> sizes = new List<int>();
        long cursor = headerEnd;
        for (int i = 0; i < count; i++)
        {
            int o = 6 + 16 * i;
            int widthByte = bytes[o];
            int heightByte = bytes[o + 1];
            Check(bytes[o + 2] == 0 && bytes[o + 3] == 0, path + " entry " + i + " colorCount/reserved not zero");
            Check(ReadInt16(bytes, o + 4) == 1, path + " entry " + i + " planes != 1");
            Check(ReadInt16(bytes, o + 6) == 32, path + " entry " + i + " bitCount != 32");
            int declared = widthByte == 0 ? 256 : widthByte;
            Check(heightByte == widthByte, path + " entry " + i + " non-square tier");
            Check(declared == 16 || declared == 24 || declared == 32 || declared == 48
                || declared == 64 || declared == 128 || declared == 256,
                path + " entry " + i + " unexpected tier " + declared);
            Check(!sizes.Contains(declared), path + " duplicate tier " + declared);
            sizes.Add(declared);
            long length = ReadInt32(bytes, o + 8) & 0xFFFFFFFFL;
            long offset = ReadInt32(bytes, o + 12) & 0xFFFFFFFFL;
            Check(offset == cursor, path + " entry " + i + " offset " + offset + " != sequential " + cursor);
            Check(offset >= headerEnd && offset + length <= bytes.Length,
                path + " entry " + i + " out of bounds");
            Entry entry = new Entry();
            entry.Declared = declared;
            entry.Offset = offset;
            entry.Length = length;
            entries.Add(entry);
            cursor += length;
        }
        Check(sizes.Contains(256), path + " has no 256 tier");
        foreach (Entry entry in entries) DecodeEntry(path, bytes, entry);
        return entries;
    }

    private static void DecodeEntry(string path, byte[] bytes, Entry entry)
    {
        bool isPng = entry.Length > 8 && bytes[entry.Offset] == 0x89 && bytes[entry.Offset + 1] == 0x50
            && bytes[entry.Offset + 2] == 0x4E && bytes[entry.Offset + 3] == 0x47;
        if (isPng)
        {
            Check(entry.Declared == 256, path + " PNG entry at non-256 tier " + entry.Declared);
            using (Bitmap bitmap = new Bitmap(new MemoryStream(Slice(bytes, entry.Offset, entry.Length))))
            {
                Check(bitmap.Width == entry.Declared && bitmap.Height == entry.Declared,
                    path + " PNG decodes to " + bitmap.Width + "x" + bitmap.Height + " != " + entry.Declared);
                Check(HasVisiblePixel(bitmap), path + " 256 PNG is fully transparent");
            }
            return;
        }
        // 32bpp BI_RGB DIB: header, doubled height, BGRA bottom-up rows, zero AND mask.
        long w = entry.Declared, h = entry.Declared;
        long andRow = (w + 31) / 32 * 4;
        long expected = 40 + w * h * 4 + andRow * h;
        Check(entry.Length == expected, path + " DIB " + w + " length " + entry.Length + " != " + expected);
        long o = entry.Offset;
        Check(ReadInt32(bytes, (int)o) == 40, path + " biSize != 40");
        Check(ReadInt32(bytes, (int)o + 4) == w, path + " biWidth != " + w);
        Check(ReadInt32(bytes, (int)o + 8) == 2 * h, path + " biHeight != " + 2 * h);
        Check(ReadInt16(bytes, (int)o + 12) == 1, path + " biPlanes != 1");
        Check(ReadInt16(bytes, (int)o + 14) == 32, path + " biBitCount != 32");
        Check(ReadInt32(bytes, (int)o + 16) == 0, path + " biCompression != BI_RGB");
        // Rebuild the image from the DIB and require a visible, colored pixel.
        bool visible = false, colored = false;
        for (long y = 0; y < h && (!visible || !colored); y++)
        {
            long row = o + 40 + y * w * 4; // file rows are bottom-up
            for (long x = 0; x < w; x++)
            {
                byte blue = bytes[row + x * 4], green = bytes[row + x * 4 + 1];
                byte red = bytes[row + x * 4 + 2], alpha = bytes[row + x * 4 + 3];
                if (alpha > 0) visible = true;
                if ((red | green | blue) != 0) colored = true;
                if (visible && colored) break;
            }
        }
        Check(visible, path + " DIB " + w + " has no visible (alpha>0) pixel");
        Check(colored, path + " DIB " + w + " has no colored pixel");
    }

    private static bool HasVisiblePixel(Bitmap bitmap)
    {
        Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                IntPtr row = new IntPtr(data.Scan0.ToInt64() + y * data.Stride);
                for (int x = 0; x < bitmap.Width; x++)
                    if (Marshal.ReadByte(row, x * 4 + 3) > 0) return true;
            }
            return false;
        }
        finally { bitmap.UnlockBits(data); }
    }

    private static byte[] Slice(byte[] source, long offset, long length)
    {
        byte[] result = new byte[length];
        for (long i = 0; i < length; i++) result[i] = source[offset + i];
        return result;
    }

    private static byte[] ReadAll(Stream stream)
    {
        using (MemoryStream buffer = new MemoryStream())
        {
            byte[] chunk = new byte[8192];
            int read;
            while ((read = stream.Read(chunk, 0, chunk.Length)) > 0) buffer.Write(chunk, 0, read);
            return buffer.ToArray();
        }
    }

    // Scan with a 64-byte prefix, then verify the full payload at the hit.
    private static long IndexOf(byte[] haystack, byte[] needle)
    {
        int prefix = Math.Min(64, needle.Length);
        for (int i = 0; i + needle.Length <= haystack.Length; i++)
        {
            int j = 0;
            while (j < prefix && haystack[i + j] == needle[j]) j++;
            if (j < prefix) continue;
            while (j < needle.Length && haystack[i + j] == needle[j]) j++;
            if (j == needle.Length) return i;
        }
        return -1;
    }

    private static int ReadInt16(byte[] b, int o)
    { return b[o] | (b[o + 1] << 8); }

    private static int ReadInt32(byte[] b, int o)
    { return (int)((uint)b[o] | ((uint)b[o + 1] << 8) | ((uint)b[o + 2] << 16) | ((uint)b[o + 3] << 24)); }
}

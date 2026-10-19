using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

// Icon generator for "一键黑屏": draws the rounded-screen-with-moon motif (and the
// settings variant with a gear badge) freshly at each size, then packs the classic
// 7-tier multi-size .ico. Compiled and run standalone with the Framework csc:
//   csc -nologo -target:exe -out:_probe\MakeIcon.exe tools\MakeIcon.cs
// Writing assets\ is deliberate: the generated .ico files are committed artifacts
// and the normal build only consumes them (V8).
internal static class MakeIcon
{
    private static readonly int[] Sizes = new int[] { 16, 24, 32, 48, 64, 128, 256 };

    private static readonly Color BezelFill = Color.FromArgb(0x26, 0x32, 0x38);
    private static readonly Color BezelEdge = Color.FromArgb(0xC3, 0xCF, 0xD8);
    private static readonly Color NightFill = Color.FromArgb(0x0A, 0x0E, 0x14);
    private static readonly Color MoonFill = Color.FromArgb(0xEE, 0xF1, 0xFB);
    private static readonly Color GearFill = Color.FromArgb(0x64, 0xB5, 0xF6);
    private static readonly Color BackingFill = Color.FromArgb(0x32, 0x40, 0x49);

    private static int Main(string[] args)
    {
        try
        {
            string root = AppRoot();
            string assets = Path.Combine(root, "assets");
            string probe = Path.Combine(root, "_probe");
            Directory.CreateDirectory(assets);
            Directory.CreateDirectory(probe);
            WriteIcon(Path.Combine(assets, "icon-main.ico"), false, probe, "icon-main");
            WriteIcon(Path.Combine(assets, "icon-settings.ico"), true, probe, "icon-settings");
            Console.WriteLine("[OK] icons written to " + assets);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[FAIL] " + ex.Message);
            return 1;
        }
    }

    private static string AppRoot()
    {
        // The exe normally lives in _probe\; fall back to the current directory.
        string directory = Path.GetDirectoryName(Path.GetFullPath(new Uri(typeof(MakeIcon).Assembly.CodeBase).LocalPath));
        DirectoryInfo info = new DirectoryInfo(directory);
        while (info != null && !Directory.Exists(Path.Combine(info.FullName, "src")))
            info = info.Parent;
        return info == null ? directory : info.FullName;
    }

    private static void WriteIcon(string path, bool gearBadge, string probe, string name)
    {
        byte[][] blobs = new byte[Sizes.Length][];
        for (int i = 0; i < Sizes.Length; i++)
        {
            using (Bitmap bitmap = Draw(Sizes[i], gearBadge))
            {
                blobs[i] = Sizes[i] <= 128 ? BuildDibEntry(bitmap) : BuildPngEntry(bitmap);
                if (Sizes[i] == 16 || Sizes[i] == 32 || Sizes[i] == 256)
                {
                    bitmap.Save(Path.Combine(probe, name + "-" + Sizes[i] + ".png"), ImageFormat.Png);
                    // Nearest-neighbour blow-ups of the small tiers show the real pixels.
                    if (Sizes[i] <= 32)
                        using (Bitmap scaled = new Bitmap(Sizes[i] * 8, Sizes[i] * 8))
                        {
                            scaled.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
                            using (Graphics g = Graphics.FromImage(scaled))
                            {
                                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                                g.PixelOffsetMode = PixelOffsetMode.Half;
                                g.DrawImage(bitmap, 0, 0, scaled.Width, scaled.Height);
                            }
                            scaled.Save(Path.Combine(probe, name + "-" + Sizes[i] + "-x8.png"), ImageFormat.Png);
                        }
                }
            }
        }
        using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
        {
            writer.Write((ushort)0);      // idReserved
            writer.Write((ushort)1);      // idType = icon
            writer.Write((ushort)Sizes.Length);
            long offset = 6 + 16L * Sizes.Length;
            for (int i = 0; i < Sizes.Length; i++)
            {
                writer.Write((byte)(Sizes[i] >= 256 ? 0 : Sizes[i]));  // width
                writer.Write((byte)(Sizes[i] >= 256 ? 0 : Sizes[i]));  // height
                writer.Write((byte)0);      // color count
                writer.Write((byte)0);      // reserved
                writer.Write((ushort)1);    // planes
                writer.Write((ushort)32);   // bit count
                writer.Write((uint)blobs[i].Length);
                writer.Write((uint)offset);
                offset += blobs[i].Length;
            }
            for (int i = 0; i < Sizes.Length; i++)
                writer.Write(blobs[i]);
        }
        Console.WriteLine("[OK] " + Path.GetFileName(path) + " (" + new FileInfo(path).Length + " bytes)");
    }

    private static Bitmap Draw(int size, bool gearBadge)
    {
        Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size;
            // Screen bezel.
            RectangleF bezel = RectangleF.FromLTRB(s * 0.06f, s * 0.10f, s * 0.94f, s * 0.90f);
            using (GraphicsPath path = Rounded(bezel, s * 0.12f))
            {
                using (SolidBrush brush = new SolidBrush(BezelFill))
                    g.FillPath(brush, path);
                using (Pen pen = new Pen(BezelEdge, Math.Max(1.25f, s / 32f)))
                    g.DrawPath(pen, path);
            }
            // Inner night screen + crescent moon, clipped so the bite stays inside.
            RectangleF night = RectangleF.FromLTRB(s * 0.13f, s * 0.17f, s * 0.87f, s * 0.83f);
            using (GraphicsPath clip = Rounded(night, s * 0.08f))
            {
                g.SetClip(clip);
                using (SolidBrush brush = new SolidBrush(NightFill))
                    g.FillPath(brush, clip);
                // Small tiers get a bigger moon with a thicker crescent so 16px stays readable.
                if (s <= 24f) DrawMoon(g, s * 0.53f, s * 0.46f, s * 0.40f, 1.06f, 0.40f);
                else DrawMoon(g, s * 0.53f, s * 0.46f, s * 0.36f, 1.10f, 0.42f);
                g.ResetClip();
            }
            if (gearBadge)
                DrawGear(g, s * 0.72f, s * 0.74f, s <= 32f ? s * 0.19f : s * 0.17f, s);
        }
        return bitmap;
    }

    private static void DrawMoon(Graphics g, float cx, float cy, float diameter,
        float cutRadiusFactor, float offsetFactor)
    {
        float r = diameter / 2f;
        using (SolidBrush moon = new SolidBrush(MoonFill))
            g.FillEllipse(moon, cx - r, cy - r, diameter, diameter);
        // The bite circle cuts a crescent opening toward the upper right.
        float cutR = r * cutRadiusFactor;
        float offset = diameter * offsetFactor;
        using (SolidBrush night = new SolidBrush(NightFill))
            g.FillEllipse(night, cx + offset * 0.7071f - cutR, cy - offset * 0.7071f - cutR,
                cutR * 2f, cutR * 2f);
    }

    private static void DrawGear(Graphics g, float cx, float cy, float outerR, float s)
    {
        // Backing disc separates the badge from the screen and from any taskbar color.
        float backing = outerR * 1.26f;
        using (SolidBrush brush = new SolidBrush(BackingFill))
            g.FillEllipse(brush, cx - backing, cy - backing, backing * 2f, backing * 2f);
        using (Pen pen = new Pen(BezelEdge, Math.Max(1.5f, s / 28f)))
            g.DrawEllipse(pen, cx - backing, cy - backing, backing * 2f, backing * 2f);
        // Tiny gears cannot resolve teeth: 6 chunky teeth, deep valleys and no hole below 48px.
        int teeth = s <= 32f ? 6 : 8;
        using (SolidBrush gear = new SolidBrush(GearFill))
        {
            float toothWidth = outerR * (s <= 32f ? 0.40f : 0.32f);
            for (int i = 0; i < teeth; i++)
            {
                g.TranslateTransform(cx, cy);
                g.RotateTransform(i * 360f / teeth);
                g.FillRectangle(gear, -toothWidth / 2f, -outerR, toothWidth, outerR * 0.46f);
                g.ResetTransform();
            }
            float body = outerR * (s <= 32f ? 0.58f : 0.60f);
            g.FillEllipse(gear, cx - body, cy - body, body * 2f, body * 2f);
        }
        if (s >= 48f)
        {
            float hole = outerR * 0.30f;
            using (SolidBrush holeBrush = new SolidBrush(NightFill))
                g.FillEllipse(holeBrush, cx - hole, cy - hole, hole * 2f, hole * 2f);
        }
    }

    private static GraphicsPath Rounded(RectangleF bounds, float radius)
    {
        GraphicsPath path = new GraphicsPath();
        float d = radius * 2f;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    // 32bpp BI_RGB DIB: 40-byte header, doubled biHeight, BGRA rows bottom-up,
    // then an all-zero AND mask (per-pixel alpha does the work).
    private static byte[] BuildDibEntry(Bitmap bitmap)
    {
        int w = bitmap.Width, h = bitmap.Height;
        Rectangle rect = new Rectangle(0, 0, w, h);
        BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int andRow = ((w + 31) / 32) * 4;
        byte[] dib = new byte[40 + w * h * 4 + andRow * h];
        try
        {
            WriteInt32(dib, 0, 40);
            WriteInt32(dib, 4, w);
            WriteInt32(dib, 8, h * 2);
            WriteInt16(dib, 12, 1);
            WriteInt16(dib, 14, 32);
            WriteInt32(dib, 16, 0);                                   // BI_RGB
            WriteInt32(dib, 20, 40 + w * h * 4 + andRow * h);         // biSizeImage
            for (int y = 0; y < h; y++)
            {
                // Explicit row addressing: never assume the sign of Stride.
                IntPtr sourceRow = new IntPtr(data.Scan0.ToInt64() + (h - 1 - y) * data.Stride);
                Marshal.Copy(sourceRow, dib, 40 + y * w * 4, w * 4);
            }
            return dib;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private static byte[] BuildPngEntry(Bitmap bitmap)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
    }

    private static void WriteInt16(byte[] target, int offset, int value)
    {
        target[offset] = (byte)value;
        target[offset + 1] = (byte)(value >> 8);
    }

    private static void WriteInt32(byte[] target, int offset, int value)
    {
        target[offset] = (byte)value;
        target[offset + 1] = (byte)(value >> 8);
        target[offset + 2] = (byte)(value >> 16);
        target[offset + 3] = (byte)(value >> 24);
    }
}

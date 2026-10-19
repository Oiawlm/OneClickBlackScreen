using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

// Overlay verifier: extends baseline VerifyBlackScreen for the per-monitor overlay
// with custom color/text. Covers: one window per screen with exact bounds, topmost,
// borderless, sampled pixels equal to the configured color, text pixels present with
// text and absent without, 3-key foreground+background keyboard exit with exit code 0,
// the frozen arming semantics, second-instance self-exit, and InputIdle <= 2000ms.
internal static class VerifyOverlay
{
    private delegate bool EnumProc(IntPtr hwnd, IntPtr data);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] private static extern bool SetProcessDPIAware();

    private static string exePath;
    private static byte[] configBackup;
    private static bool configExisted;
    private static long maxIdleMs;

    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    private static string ConfigPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BlackScreen", "settings.ini");
    }

    private static void BackupConfig()
    {
        string path = ConfigPath();
        configExisted = File.Exists(path);
        configBackup = configExisted ? File.ReadAllBytes(path) : null;
    }

    private static void RestoreConfig()
    {
        try
        {
            string path = ConfigPath();
            if (configExisted) File.WriteAllBytes(path, configBackup);
            else if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("WARN: could not restore config: " + ex.Message);
        }
    }

    private static void RemoveConfig()
    {
        string path = ConfigPath();
        if (File.Exists(path)) File.Delete(path);
    }

    private static void WriteConfig(string colorHex, string text)
    {
        string path = ConfigPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string body = "color=" + colorHex + "\r\n" + (text == null ? "" : "text=" + text + "\r\n");
        File.WriteAllText(path, body);
    }

    private static Process Launch()
    {
        ProcessStartInfo start = new ProcessStartInfo(exePath);
        start.UseShellExecute = false;
        start.WindowStyle = ProcessWindowStyle.Hidden;
        Stopwatch watch = Stopwatch.StartNew();
        Process process = Process.Start(start);
        Check(process.WaitForInputIdle(5000), "UI did not become ready");
        watch.Stop();
        if (watch.ElapsedMilliseconds > maxIdleMs) maxIdleMs = watch.ElapsedMilliseconds;
        Check(watch.ElapsedMilliseconds <= 2000,
            "InputIdle too slow: " + watch.ElapsedMilliseconds + " ms");
        Thread.Sleep(400); // Let the launch-input arming timer finish.
        return process;
    }

    private static List<IntPtr> EnumVisible(int pid)
    {
        List<IntPtr> windows = new List<IntPtr>();
        EnumWindows(delegate(IntPtr hwnd, IntPtr data)
        {
            uint owner;
            GetWindowThreadProcessId(hwnd, out owner);
            if (owner == (uint)pid && IsWindowVisible(hwnd)) windows.Add(hwnd);
            return true;
        }, IntPtr.Zero);
        return windows;
    }

    // Window creation timing is not guaranteed by WaitForInputIdle; poll until the
    // expected per-screen window count is visible (or the process exits / times out).
    private static List<IntPtr> OverlayWindows(Process process, Screen[] screens)
    {
        List<IntPtr> windows = new List<IntPtr>();
        for (int elapsed = 0; elapsed <= 5000; elapsed += 50)
        {
            Check(!process.HasExited, "Overlay exited before showing windows");
            windows = EnumVisible(process.Id);
            if (windows.Count == screens.Length) return windows;
            Thread.Sleep(50);
        }
        throw new Exception("Expected " + screens.Length + " overlay windows, saw " + windows.Count);
    }

    // Maps each screen to its window; every window must equal its screen bounds.
    private static Dictionary<Screen, IntPtr> MapWindows(List<IntPtr> windows, Screen[] screens)
    {
        Dictionary<Screen, IntPtr> map = new Dictionary<Screen, IntPtr>();
        foreach (Screen screen in screens)
        {
            bool found = false;
            foreach (IntPtr hwnd in windows)
            {
                Rect rect;
                GetWindowRect(hwnd, out rect);
                if (Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom) == screen.Bounds)
                {
                    map[screen] = hwnd;
                    found = true;
                    break;
                }
            }
            Check(found, "No window covers " + screen.DeviceName + " exactly");
        }
        foreach (IntPtr hwnd in windows)
        {
            Check((GetWindowLong(hwnd, -20) & 0x8) != 0, "Window is not topmost");
            Check((GetWindowLong(hwnd, -16) & 0x00C40000) == 0, "Window has a caption or resize border");
        }
        return map;
    }

    private static Bitmap RenderWindow(IntPtr hwnd, Rectangle bounds)
    {
        Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Magenta);
            IntPtr dc = graphics.GetHdc();
            try { Check(PrintWindow(hwnd, dc, 0), "Unable to render window"); }
            finally { graphics.ReleaseHdc(dc); }
        }
        return bitmap;
    }

    private static void CheckSolidPoints(Bitmap bitmap, Rectangle window, Color expected)
    {
        int[] offsets = new int[] { 0, 1, window.Width / 2, window.Width - 2, window.Width - 1 };
        int[] offsetY = new int[] { 0, 1, window.Height / 2, window.Height - 2, window.Height - 1 };
        foreach (int x in offsets)
            foreach (int y in offsetY)
                Check(bitmap.GetPixel(Normalize(x, window.Width), Normalize(y, window.Height)).ToArgb() == expected.ToArgb(),
                    "Pixel mismatch at " + x + "," + y + ": expected " + expected);
    }

    private static int Normalize(int value, int max)
    {
        if (max <= 1) return 0;
        if (value < 0) return 0;
        if (value >= max) return max - 1;
        return value;
    }

    // Scans the central third of the window for pixels of the expected text color.
    private static void CheckTextPixels(Bitmap bitmap, Rectangle window, Color textColor)
    {
        int left = window.Width / 3, top = window.Height / 3;
        int width = window.Width / 3, height = window.Height / 3;
        int hits = 0;
        for (int x = left; x < left + width; x += 2)
            for (int y = top; y < top + height; y += 2)
                if (bitmap.GetPixel(x, y).ToArgb() == textColor.ToArgb()) hits++;
        Check(hits > 0, "No text-colored pixels found in center region");
    }

    private static void CheckCenterSolid(Bitmap bitmap, Rectangle window, Color expected)
    {
        for (int i = 0; i <= 6; i++)
        {
            int x = window.Width / 3 + (window.Width / 3) * i / 6;
            int y = window.Height / 3 + (window.Height / 3) * i / 6;
            Check(bitmap.GetPixel(Normalize(x, window.Width), Normalize(y, window.Height)).ToArgb() == expected.ToArgb(),
                "Center not solid at " + x + "," + y);
        }
    }

    // Independent WCAG implementation of the automatic text color (D4).
    private static Color ExpectedTextColor(Color background)
    {
        double light = Contrast(background, Color.FromArgb(0xF5, 0xF5, 0xF5));
        double dark = Contrast(background, Color.FromArgb(0x11, 0x11, 0x11));
        return light >= dark ? Color.FromArgb(0xF5, 0xF5, 0xF5) : Color.FromArgb(0x11, 0x11, 0x11);
    }

    private static double Contrast(Color a, Color b)
    {
        double la = Luminance(a), lb = Luminance(b);
        double lighter = Math.Max(la, lb), darker = Math.Min(la, lb);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double Luminance(Color c)
    {
        return 0.2126 * Linear(c.R / 255.0) + 0.7152 * Linear(c.G / 255.0) + 0.0722 * Linear(c.B / 255.0);
    }

    private static double Linear(double channel)
    {
        return channel <= 0.03928 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    private static void ExitByKey(Process process, byte key)
    {
        keybd_event(key, 0, 0, UIntPtr.Zero);
        keybd_event(key, 0, 2, UIntPtr.Zero);
        Check(process.WaitForExit(3000), "Keyboard key 0x" + key.ToString("X") + " failed to close overlay");
        Check(process.ExitCode == 0, "Program exited with an error");
    }

    private static void FullPixelCheck(Process process, Screen[] screens, Color maskColor, string text)
    {
        List<IntPtr> windows = OverlayWindows(process, screens);
        Dictionary<Screen, IntPtr> map = MapWindows(windows, screens);
        foreach (Screen screen in screens)
        {
            using (Bitmap bitmap = RenderWindow(map[screen], screen.Bounds))
            {
                CheckSolidPoints(bitmap, screen.Bounds, maskColor);
                if (text != null) CheckTextPixels(bitmap, screen.Bounds, ExpectedTextColor(maskColor));
                else CheckCenterSolid(bitmap, screen.Bounds, maskColor);
            }
        }
    }

    private static void KillIfNeeded(Process process)
    {
        if (!process.HasExited)
        {
            try { process.Kill(); }
            catch (InvalidOperationException) { }
            process.WaitForExit(3000);
        }
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Check(args.Length == 1, "Usage: VerifyOverlay.exe <exe>");
            exePath = args[0];
            try { SetProcessDpiAwarenessContext(new IntPtr(-4)); }
            catch (EntryPointNotFoundException) { SetProcessDPIAware(); }
            Screen[] screens = Screen.AllScreens;
            Check(screens.Length > 0, "No usable displays");
            BackupConfig();

            try
            {
                // A. No config file: v1-equivalent pure black, no text.
                RemoveConfig();
                using (Process process = Launch())
                {
                    try
                    {
                        FullPixelCheck(process, screens, Color.Black, null);
                        ExitByKey(process, 0x87);
                        Console.WriteLine("PASS A: no config = pure black v1 behavior, " + screens.Length + " windows, exact bounds, topmost, borderless");
                    }
                    finally { KillIfNeeded(process); }
                }

                // B. Configured color + text.
                WriteConfig("#1A2B3C", "VERIFY 123");
                using (Process process = Launch())
                {
                    try
                    {
                        FullPixelCheck(process, screens, Color.FromArgb(0x1A, 0x2B, 0x3C), "VERIFY 123");
                        ExitByKey(process, 0x1B);
                        Console.WriteLine("PASS B: configured color + text rendered, auto-contrast text pixels found");
                    }
                    finally { KillIfNeeded(process); }
                }

                // C. Configured color only: whole mask solid.
                WriteConfig("#2E8B57", null);
                using (Process process = Launch())
                {
                    try
                    {
                        FullPixelCheck(process, screens, Color.FromArgb(0x2E, 0x8B, 0x57), null);
                        ExitByKey(process, 0x10);
                        Console.WriteLine("PASS C: color-only config = solid mask on every screen");
                    }
                    finally { KillIfNeeded(process); }
                }

                // D. Keyboard exit: 3 keys x {foreground, background} + Alt syskey pass.
                RemoveConfig();
                foreach (byte key in new byte[] { 0x87, 0x1B, 0x10, 0x12 })
                {
                    foreach (bool background in new bool[] { false, true })
                    {
                        if (key == 0x12 && background) continue;
                        IntPtr previousWindow = GetForegroundWindow();
                        using (Process process = Launch())
                        {
                            try
                            {
                                List<IntPtr> windows = OverlayWindows(process, screens);
                                MapWindows(windows, screens);
                                if (background && previousWindow != IntPtr.Zero)
                                {
                                    SetForegroundWindow(previousWindow);
                                    Thread.Sleep(100);
                                    IntPtr foreground = GetForegroundWindow();
                                    foreach (IntPtr hwnd in windows)
                                        Check(foreground != hwnd, "Could not exercise background keyboard hook");
                                }
                                ExitByKey(process, key);
                                Console.WriteLine("PASS D: key 0x" + key.ToString("X")
                                    + (background ? " background" : " foreground") + " exit, code 0");
                            }
                            finally
                            {
                                KillIfNeeded(process);
                                if (previousWindow != IntPtr.Zero) SetForegroundWindow(previousWindow);
                            }
                        }
                    }
                }

                // E. Frozen arming semantics: held key blocks arm; unarmed key is ignored;
                //    after release the same key exits.
                keybd_event(0x10, 0, 0, UIntPtr.Zero); // Shift down before launch
                using (Process process = Launch())
                {
                    try
                    {
                        keybd_event(0x1B, 0, 0, UIntPtr.Zero);
                        keybd_event(0x1B, 0, 2, UIntPtr.Zero);
                        Thread.Sleep(800);
                        Check(!process.HasExited, "Overlay exited before arming");
                        keybd_event(0x10, 0, 2, UIntPtr.Zero); // release Shift -> next tick arms
                        Thread.Sleep(400);
                        ExitByKey(process, 0x1B);
                        Console.WriteLine("PASS E: held key blocked arming, unarmed key ignored, exit after release");
                    }
                    finally
                    {
                        keybd_event(0x10, 0, 2, UIntPtr.Zero);
                        KillIfNeeded(process);
                    }
                }

                // F. Second instance self-exits while the first stays alive.
                using (Process first = Launch())
                {
                    try
                    {
                        ProcessStartInfo start = new ProcessStartInfo(exePath);
                        start.UseShellExecute = false;
                        start.WindowStyle = ProcessWindowStyle.Hidden;
                        using (Process second = Process.Start(start))
                        {
                            Check(second.WaitForExit(3000), "Second instance did not self-exit");
                            Check(second.ExitCode == 0, "Second instance exit code " + second.ExitCode);
                        }
                        Thread.Sleep(200);
                        Check(!first.HasExited, "First instance died after second launch");
                        ExitByKey(first, 0x87);
                        Console.WriteLine("PASS F: second instance self-exited silently, first stayed alive");
                    }
                    finally { KillIfNeeded(first); }
                }

                Check(maxIdleMs <= 2000, "InputIdle exceeded budget");
                Console.WriteLine("PASS TOTAL: A-F; " + screens.Length + " displays; max InputIdle " + maxIdleMs + " ms (limit 2000)");
                return 0;
            }
            finally
            {
                RestoreConfig();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }
}

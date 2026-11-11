using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

// Overlay verifier: extends baseline VerifyBlackScreen for the per-monitor overlay
// with custom color/text. Covers: one window per screen with exact bounds, topmost,
// borderless, sampled pixels equal to the configured color, text pixels present with
// text and absent without, 3-key foreground+background keyboard exit with exit code 0,
// the frozen arming semantics, second-instance self-exit, InputIdle <= 2000ms, the
// launch-key self-latch (G) and the phantom-key arming regression (H) from V6A.
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
    // V7V zero footprint: children resolve config via the USERPROFILE seam
    // (NewStart) into this temp tree; the old backup-overwrite-restore is gone.
    private static string userRoot;
    private static string configPath;
    private static long maxIdleMs;

    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    private static string Sha256HexOrNull(string path)
    {
        if (!File.Exists(path)) return null;
        using (SHA256 sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
    }

    private static void WriteConfig(string colorHex, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(configPath));
        string body = "color=" + colorHex + "\r\n" + (text == null ? "" : "text=" + text + "\r\n");
        File.WriteAllText(configPath, body);
    }

    // USERPROFILE seam: GetFolderPath(LocalApplicationData) expands the
    // "%USERPROFILE%\AppData\Local" template against the process env
    // (LOCALAPPDATA is ignored; docs/VERIFY-V7V.md); the B/C pixel checks prove
    // the child really reads the redirected config.
    private static ProcessStartInfo NewStart()
    {
        ProcessStartInfo start = new ProcessStartInfo(exePath);
        start.UseShellExecute = false;
        start.WindowStyle = ProcessWindowStyle.Hidden;
        start.EnvironmentVariables["USERPROFILE"] = userRoot;
        return start;
    }

    private static Process Launch()
    {
        Stopwatch watch = Stopwatch.StartNew();
        Process process = Process.Start(NewStart());
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
            if (process.HasExited)
                throw new Exception("Overlay exited before showing windows (exit code "
                    + process.ExitCode + "; 0 = closed by real user input mid-test, 1 = internal failure)");
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

    // Renders the hint painter into a bitmap and asserts the two injected
    // states: tagged = pixels land in the bottom-right quadrant and only
    // there; untagged = the whole area stays the pure background color.
    private static void CheckUpdateHintPaint()
    {
        const int Width = 600, Height = 400;
        Color background = Color.FromArgb(0x1A, 0x2B, 0x3C);
        int hintPixels = 0, strayPixels = 0, centerHits = 0;
        using (Bitmap bitmap = new Bitmap(Width, Height))
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(background);
                OverlayForm.PaintUpdateHint(graphics, new Rectangle(0, 0, Width, Height),
                    "检测到新版本 v9.9.9", background);
            }
            for (int x = 0; x < Width; x += 2)
                for (int y = 0; y < Height; y += 2)
                {
                    if (bitmap.GetPixel(x, y).ToArgb() == background.ToArgb()) continue;
                    bool inCorner = x >= Width / 2 && y >= Height / 2;
                    bool inCenter = x >= Width / 3 && x < Width * 2 / 3 && y >= Height / 3 && y < Height * 2 / 3;
                    if (inCorner) hintPixels++;
                    else if (inCenter) centerHits++;
                    else strayPixels++;
                }
        }
        Check(hintPixels > 0, "the tagged hint painted nothing in the bottom-right quadrant");
        Check(centerHits == 0, "the hint bled into the central text region");
        Check(strayPixels == 0, "the hint painted outside the bottom-right quadrant");
        using (Bitmap clean = new Bitmap(Width, Height))
        {
            using (Graphics graphics = Graphics.FromImage(clean))
            {
                graphics.Clear(background);
                OverlayForm.PaintUpdateHint(graphics, new Rectangle(0, 0, Width, Height), null, background);
            }
            for (int x = 0; x < Width; x += 2)
                for (int y = 0; y < Height; y += 2)
                    Check(clean.GetPixel(x, y).ToArgb() == background.ToArgb(),
                        "the untagged hint must paint absolutely nothing");
        }
        Console.WriteLine("PASS I: update hint line - tagged paints the bottom-right quadrant only, untagged paints nothing");
    }

    [STAThread]
    private static int Main(string[] args)
    {
        VerifyGate.Enter();
        try
        {
            Check(args.Length == 1, "Usage: VerifyOverlay.exe <exe>");
            exePath = args[0];
            try { SetProcessDpiAwarenessContext(new IntPtr(-4)); }
            catch (EntryPointNotFoundException) { SetProcessDPIAware(); }
            Screen[] screens = Screen.AllScreens;
            Check(screens.Length > 0, "No usable displays");
            // Real settings.ini hashed before/after as the zero-footprint tripwire.
            string realIni = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BlackScreen", "settings.ini");
            string realShaBefore = Sha256HexOrNull(realIni);
            userRoot = Path.Combine(Path.GetTempPath(), "VerifyOverlay_" + DateTime.Now.Ticks.ToString("x"));

            try
            {
                // Known-folder skeleton GetFolderPath existence-validates.
                Directory.CreateDirectory(Path.Combine(userRoot, "AppData", "Local"));
                Directory.CreateDirectory(Path.Combine(userRoot, "AppData", "Roaming"));
                configPath = Path.Combine(userRoot, "AppData", "Local", "BlackScreen", "settings.ini");

                // A. No config file: v1-equivalent pure black, no text.
                if (File.Exists(configPath)) File.Delete(configPath);
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
                if (File.Exists(configPath)) File.Delete(configPath);
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
                //    after release the same key exits. The outer finally pairs the Shift
                //    down even when Launch() throws (V6B hygiene audit handoff).
                keybd_event(0x10, 0, 0, UIntPtr.Zero); // Shift down before launch
                try
                {
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
                }
                finally { keybd_event(0x10, 0, 2, UIntPtr.Zero); }

                // F. Second instance self-exits while the first stays alive.
                using (Process first = Launch())
                {
                    try
                    {
                        using (Process second = Process.Start(NewStart()))
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

                // G. Launch-key self-latch (V6A): a key held since launch never exits by
                //    itself - its auto-repeat KEYDOWNs pass through - until its release is
                //    observed; the same key then exits (README: release first, press to exit).
                keybd_event(0x10, 0, 0, UIntPtr.Zero); // Shift held since before launch
                try
                {
                    using (Process process = Launch())
                    {
                        try
                        {
                            Thread.Sleep(1200); // pass the 1s arming grace: Shift latched, arming done
                            for (int repeat = 0; repeat < 3; repeat++)
                            {
                                keybd_event(0x10, 0, 0, UIntPtr.Zero); // auto-repeat of the held key
                                Thread.Sleep(50);
                            }
                            Thread.Sleep(300);
                            Check(!process.HasExited, "Held launch key auto-repeat exited overlay");
                            keybd_event(0x10, 0, 2, UIntPtr.Zero); // release lifts the latch
                            Thread.Sleep(400);
                            ExitByKey(process, 0x10);
                            Console.WriteLine("PASS G: held launch key auto-repeat passed through; exit after release");
                        }
                        finally
                        {
                            keybd_event(0x10, 0, 2, UIntPtr.Zero);
                            KillIfNeeded(process);
                        }
                    }
                }
                finally { keybd_event(0x10, 0, 2, UIntPtr.Zero); }

                // H. Phantom-key regression (V6A): a key state stuck down without real
                //    events (external down injection with no up) must not disable the exit
                //    paths: after the grace window only that key's own exit input stays
                //    suppressed - SPACE still closes the mask.
                keybd_event(0x1B, 0, 0, UIntPtr.Zero); // phantom ESC down, no up
                try
                {
                    using (Process process = Launch())
                    {
                        try
                        {
                            Thread.Sleep(1200); // pass the 1s arming grace under the phantom
                            Check(!process.HasExited, "Phantom key alone closed the overlay");
                            ExitByKey(process, 0x20);
                            Console.WriteLine("PASS H: phantom held key did not block arming; SPACE exited");
                        }
                        finally { KillIfNeeded(process); }
                    }
                }
                finally { keybd_event(0x1B, 0, 2, UIntPtr.Zero); } // clear the phantom on every path

                // I. Update hint line (V10 D8): state injected straight into the
                //    painter seam - a tag paints a low-key line in the
                //    bottom-right corner only, and no tag paints nothing at all.
                //    The line is pure display by construction (never part of the
                //    exit logic), and the one-em corner padding keeps the exact
                //    corner pixels clean so the unmarked end-to-end mask above
                //    (A/B/C corner samples) stays the no-hint proof.
                CheckUpdateHintPaint();

                Check(maxIdleMs <= 2000, "InputIdle exceeded budget");
                Check(Sha256HexOrNull(realIni) == realShaBefore,
                    "the user's real settings.ini changed during the run (isolation leak)");
                Console.WriteLine("PASS Z: user's real settings.ini untouched (SHA-256 before == after)");
                Console.WriteLine("PASS TOTAL: A-H; " + screens.Length + " displays; max InputIdle " + maxIdleMs + " ms (limit 2000)");
                return 0;
            }
            finally
            {
                try { if (Directory.Exists(userRoot)) Directory.Delete(userRoot, true); } catch { }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// Settings-mode verifier: --settings must open exactly one normal (non-mask)
// window that can be closed; settings instances are not single-instance gated;
// unknown arguments must never show any mask window.
internal static class VerifySettings
{
    private const int WM_CLOSE = 0x0010;

    private delegate bool EnumProc(IntPtr hwnd, IntPtr data);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern int GetClassName(IntPtr hwnd, StringBuilder name, int maxCount);
    [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] private static extern bool SetProcessDPIAware();

    // Independent copy of the theme probe (the verifier never links src\): read
    // the registry expectation, sample the rendered dialog, never write anything.
    private static bool AppsUseLightTheme()
    {
        try
        {
            object value = Microsoft.Win32.Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", null);
            if (value is int) return (int)value != 0;
        }
        catch { }
        return true;
    }

    // The dialog background must equal the palette color expected from the system
    // app theme (light #F5F5F5 / dark #202020). Samples sit in always-empty strips:
    // the bottom padding below the button row and the left margin (V11, T7 #19/#20).
    private static void CheckThemeBackground(string exe)
    {
        bool light = AppsUseLightTheme();
        int expected = light ? 0xF5F5F5 : 0x202020;
        using (Process process = Start(exe, "--settings"))
        {
            try
            {
                Check(process.WaitForInputIdle(5000), "settings UI did not become ready");
                List<IntPtr> windows = WaitForVisibleWindows(process, 5000);
                Check(windows.Count == 1, "expected exactly one settings window, saw " + windows.Count);
                IntPtr hwnd = windows[0];
                Rect before, after;
                GetWindowRect(hwnd, out before);
                Thread.Sleep(150);
                GetWindowRect(hwnd, out after);
                Check(before.Left == after.Left && before.Top == after.Top
                    && before.Right == after.Right && before.Bottom == after.Bottom,
                    "settings window bounds did not settle");
                Thread.Sleep(150);
                Rect client;
                Check(GetClientRect(hwnd, out client), "GetClientRect failed");
                int width = client.Right - client.Left, height = client.Bottom - client.Top;
                Check(width > 40 && height > 40, "client area too small: " + width + "x" + height);
                using (Bitmap bitmap = new Bitmap(width, height))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.Clear(Color.Magenta);
                        IntPtr dc = graphics.GetHdc();
                        bool printed;
                        try { printed = PrintWindow(hwnd, dc, 1 /* PW_CLIENTONLY */); }
                        finally { graphics.ReleaseHdc(dc); }
                        Check(printed, "PrintWindow failed");
                    }
                    Color bottom = bitmap.GetPixel(width / 2, height - 4);
                    Color left = bitmap.GetPixel(2, height / 2);
                    Check((bottom.ToArgb() & 0xFFFFFF) == expected,
                        "bottom sample " + bottom + " is not the themed background #" + expected.ToString("X6"));
                    Check((left.ToArgb() & 0xFFFFFF) == expected,
                        "left sample " + left + " is not the themed background #" + expected.ToString("X6"));
                }
                CloseWindow(process, hwnd, "settings window");
            }
            finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(); } }
        }
        Console.WriteLine("PASS: settings background follows AppsUseLightTheme ("
            + (light ? "light" : "dark") + ", expected #" + expected.ToString("X6") + ")");
    }

    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    private static List<IntPtr> VisibleWindows(int pid)
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
    // process has visible windows (or exits / times out).
    private static List<IntPtr> WaitForVisibleWindows(Process process, int timeoutMs)
    {
        List<IntPtr> windows = new List<IntPtr>();
        for (int elapsed = 0; elapsed <= timeoutMs; elapsed += 50)
        {
            Check(!process.HasExited, "process exited before showing a window");
            windows = VisibleWindows(process.Id);
            if (windows.Count > 0) return windows;
            Thread.Sleep(50);
        }
        throw new Exception("no visible window within " + timeoutMs + " ms");
    }

    // A mask window is borderless, topmost and covers a whole monitor.
    private static bool IsMaskWindow(IntPtr hwnd)
    {
        Rect rect;
        GetWindowRect(hwnd, out rect);
        bool borderless = (GetWindowLong(hwnd, -16) & 0x00C40000) == 0;
        bool topmost = (GetWindowLong(hwnd, -20) & 0x8) != 0;
        if (!borderless || !topmost) return false;
        foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            if (screen.Bounds == System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom))
                return true;
        return false;
    }

    private static Process Start(string exe, string argument)
    {
        ProcessStartInfo start = new ProcessStartInfo(exe);
        start.Arguments = argument;
        start.UseShellExecute = false;
        start.WindowStyle = ProcessWindowStyle.Hidden;
        return Process.Start(start);
    }

    private static void CloseWindow(Process process, IntPtr hwnd, string what)
    {
        Check(PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero), "Could not post close to " + what);
        Check(process.WaitForExit(5000), what + " did not close");
        Check(process.ExitCode == 0, what + " exited with " + process.ExitCode);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Check(args.Length == 1, "Usage: VerifySettings.exe <exe>");
            // A DPI-aware observer sees the PMv2 settings window in physical pixels
            // (an unaware one gets virtualized coordinates, which would misplace the
            // sampling points of check 4).
            try { SetProcessDpiAwarenessContext(new IntPtr(-4)); }
            catch (EntryPointNotFoundException) { SetProcessDPIAware(); }

            // 1. --settings opens exactly one normal window, alive until closed.
            using (Process process = Start(args[0], "--settings"))
            {
                try
                {
                    Check(process.WaitForInputIdle(5000), "settings UI did not become ready");
                    Thread.Sleep(300);
                    Check(!process.HasExited, "settings process exited on its own");
                    List<IntPtr> windows = WaitForVisibleWindows(process, 5000);
                    Check(windows.Count == 1, "expected exactly one settings window, saw " + windows.Count);
                    Check(!IsMaskWindow(windows[0]), "settings window looks like a mask window");
                    Check((GetWindowLong(windows[0], -16) & 0x00C00000) != 0, "settings window has no caption");
                    CloseWindow(process, windows[0], "settings window");
                }
                finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(); } }
            }
            Console.WriteLine("PASS: --settings opened exactly one closable non-mask window, exit code 0");

            // 2. Settings mode is not single-instance gated: two instances coexist.
            Process first = Start(args[0], "--settings");
            Process second = null;
            try
            {
                Check(first.WaitForInputIdle(5000), "first settings UI did not become ready");
                second = Start(args[0], "--settings");
                Check(second.WaitForInputIdle(5000), "second settings UI did not become ready");
                Thread.Sleep(300);
                Check(!first.HasExited && !second.HasExited, "settings instances did not coexist");
                CloseWindow(second, WaitForVisibleWindows(second, 5000)[0], "second settings window");
                CloseWindow(first, WaitForVisibleWindows(first, 5000)[0], "first settings window");
            }
            finally
            {
                if (second != null && !second.HasExited) { second.Kill(); second.WaitForExit(); }
                if (!first.HasExited) { first.Kill(); first.WaitForExit(); }
            }
            Console.WriteLine("PASS: two --settings instances coexisted and both closed with exit code 0");

            // 3. Unknown argument shows the usage dialog and never any mask window.
            using (Process process = Start(args[0], "--bogus"))
            {
                try
                {
                    Check(process.WaitForInputIdle(5000), "usage dialog did not appear");
                    List<IntPtr> windows = WaitForVisibleWindows(process, 5000);
                    IntPtr dialog = IntPtr.Zero;
                    StringBuilder className = new StringBuilder(64);
                    foreach (IntPtr hwnd in windows)
                    {
                        Check(!IsMaskWindow(hwnd), "mask window appeared for unknown argument");
                        GetClassName(hwnd, className, className.Capacity);
                        if (className.ToString() == "#32770") dialog = hwnd;
                    }
                    Check(dialog != IntPtr.Zero, "usage MessageBox dialog not found");
                    CloseWindow(process, dialog, "usage dialog");
                }
                finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(); } }
            }
            Console.WriteLine("PASS: unknown argument showed usage dialog, zero mask windows, exit code 0");

            // 4. The settings background follows the system app theme (read-only).
            CheckThemeBackground(args[0]);

            Console.WriteLine("PASS TOTAL: 4 settings checks (3 original + theme background)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }
}

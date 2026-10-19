using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            Console.WriteLine("PASS TOTAL: 3 settings checks");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }
}

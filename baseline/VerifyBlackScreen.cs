using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

internal static class VerifyBlackScreen
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

    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            // Query physical desktop coordinates, including mixed-DPI monitors.
            try { SetProcessDpiAwarenessContext(new IntPtr(-4)); }
            catch (EntryPointNotFoundException) { SetProcessDPIAware(); }
            Screen[] screens = Screen.AllScreens;
            Rectangle expected = SystemInformation.VirtualScreen;
            Check(screens.Length > 0 && expected.Width > 0 && expected.Height > 0, "No usable displays");
            foreach (byte key in new byte[] { 0x87, 0x1B, 0x10 }) // F24, Escape, Shift
            {
                IntPtr previousWindow = GetForegroundWindow();
                ProcessStartInfo start = new ProcessStartInfo(args[0]);
                start.UseShellExecute = false;
                start.WindowStyle = ProcessWindowStyle.Hidden;
                using (Process process = Process.Start(start))
                {
                    try
                    {
                        Check(process.WaitForInputIdle(5000), "UI did not become ready");
                        Thread.Sleep(400);
                        IntPtr window = IntPtr.Zero;
                        int visibleWindows = 0;
                        EnumWindows(delegate(IntPtr hwnd, IntPtr data)
                        {
                            uint pid;
                            GetWindowThreadProcessId(hwnd, out pid);
                            if (pid == process.Id && IsWindowVisible(hwnd))
                            { window = hwnd; visibleWindows++; }
                            return true;
                        }, IntPtr.Zero);
                        Check(visibleWindows == 1, "Expected exactly one visible overlay");
                        Rect bounds;
                        Check(GetWindowRect(window, out bounds), "Cannot inspect bounds");
                        Rectangle actual = Rectangle.FromLTRB(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
                        Check(actual == expected, "Overlay bounds differ from virtual desktop: actual=" + actual + ", expected=" + expected);
                        Check((GetWindowLong(window, -20) & 0x8) != 0, "Window is not topmost");
                        Check((GetWindowLong(window, -16) & 0x00C40000) == 0, "Window has a caption or resize border");
                        foreach (Screen screen in screens)
                            Check(actual.Contains(screen.Bounds), "Overlay does not fully cover " + screen.DeviceName + ": " + screen.Bounds);

                        using (Bitmap bitmap = new Bitmap(actual.Width, actual.Height))
                        {
                            using (Graphics graphics = Graphics.FromImage(bitmap))
                            {
                                graphics.Clear(Color.Magenta);
                                IntPtr dc = graphics.GetHdc();
                                try { Check(PrintWindow(window, dc, 0), "Unable to render own window"); }
                                finally { graphics.ReleaseHdc(dc); }
                            }
                            foreach (Screen screen in screens)
                            {
                                Rectangle monitor = screen.Bounds;
                                int offsetX = monitor.Left - actual.Left;
                                int offsetY = monitor.Top - actual.Top;
                                foreach (int x in new int[] { 0, Math.Min(1, monitor.Width - 1), monitor.Width / 2, Math.Max(0, monitor.Width - 2), monitor.Width - 1 })
                                    foreach (int y in new int[] { 0, Math.Min(1, monitor.Height - 1), monitor.Height / 2, Math.Max(0, monitor.Height - 2), monitor.Height - 1 })
                                        Check(bitmap.GetPixel(offsetX + x, offsetY + y).ToArgb() == Color.Black.ToArgb(),
                                            "Non-black pixel on " + screen.DeviceName + " at " + x + "," + y);
                            }
                        }

                        if (key == 0x87 && previousWindow != IntPtr.Zero)
                        {
                            SetForegroundWindow(previousWindow);
                            Thread.Sleep(100);
                            Check(GetForegroundWindow() != window, "Could not exercise background keyboard hook");
                        }
                        keybd_event(key, 0, 0, UIntPtr.Zero);
                        keybd_event(key, 0, 2, UIntPtr.Zero);
                        Check(process.WaitForExit(3000), "Keyboard key failed to close overlay: " + key);
                        Check(process.ExitCode == 0, "Program exited with an error");
                        Console.WriteLine("PASS: key=" + key + ", bounds=" + actual + ", " + screens.Length + " displays fully covered, black monitor edges/centers, borderless, topmost");
                    }
                    finally
                    {
                        if (!process.HasExited) { process.Kill(); process.WaitForExit(); }
                        if (previousWindow != IntPtr.Zero) SetForegroundWindow(previousWindow);
                    }
                }
            }
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("FAIL: " + ex.Message); return 1; }
    }
}

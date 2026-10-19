using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

// Mouse-exit verifier for the per-monitor overlay: one window per screen, each
// window receiving the mouse messages for its own screen. Move/wheel/down keep
// the mask open; button up closes it (frozen v1 semantics).
internal static class VerifyMouseExit
{
    private delegate bool EnumProc(IntPtr hwnd, IntPtr data);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] private static extern bool SetProcessDPIAware();

    private sealed class ButtonCase
    {
        public string Name;
        public uint Down, Up;
        public int HeldFlags, ExtraWord;
        public ButtonCase(string name, uint down, uint up, int heldFlags, int extraWord)
        { Name = name; Down = down; Up = up; HeldFlags = heldFlags; ExtraWord = extraWord; }
    }

    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    private static IntPtr PackPoint(int x, int y)
    {
        Check(x >= short.MinValue && x <= short.MaxValue && y >= short.MinValue && y <= short.MaxValue,
            "Mouse coordinates exceed the signed 16-bit message range");
        return new IntPtr(unchecked((int)(((uint)(ushort)y << 16) | (ushort)x)));
    }

    private static void Post(IntPtr window, uint message, int flags, int x, int y)
    {
        Check(PostMessage(window, message, new IntPtr(flags), PackPoint(x, y)),
            "Could not post mouse message 0x" + message.ToString("X"));
    }

    private static void StayOpen(Process process, string scenario)
    { Check(!process.WaitForExit(200), "Overlay closed after " + scenario); }

    // Visible windows of the process: polls until the per-screen count is visible
    // (window creation timing is not guaranteed by WaitForInputIdle), then every
    // window must line up exactly with a physical monitor rectangle.
    private static List<IntPtr> OverlayWindows(Process process, Screen[] screens)
    {
        List<IntPtr> windows = new List<IntPtr>();
        for (int elapsed = 0; elapsed <= 5000; elapsed += 50)
        {
            if (process.HasExited)
                throw new Exception("Overlay exited before showing windows (exit code "
                    + process.ExitCode + "; 0 = closed by real user input mid-test)");
            windows.Clear();
            EnumWindows(delegate(IntPtr hwnd, IntPtr data)
            {
                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);
                if (pid == (uint)process.Id && IsWindowVisible(hwnd)) windows.Add(hwnd);
                return true;
            }, IntPtr.Zero);
            if (windows.Count == screens.Length) break;
            Thread.Sleep(50);
        }
        Check(windows.Count == screens.Length,
            "Expected " + screens.Length + " overlay windows, saw " + windows.Count);
        bool[] matched = new bool[windows.Count];
        foreach (Screen screen in screens)
        {
            int hits = 0;
            for (int i = 0; i < windows.Count; i++)
            {
                if (matched[i]) continue;
                Rect rect;
                GetWindowRect(windows[i], out rect);
                if (Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom) == screen.Bounds)
                { matched[i] = true; hits++; }
            }
            Check(hits == 1, "No exact window for " + screen.DeviceName + " " + screen.Bounds);
        }
        foreach (bool wasMatched in matched)
            Check(wasMatched, "An overlay window did not match any screen");
        return windows;
    }

    private static IntPtr WindowForScreen(List<IntPtr> windows, Screen screen)
    {
        foreach (IntPtr hwnd in windows)
        {
            Rect rect;
            GetWindowRect(hwnd, out rect);
            if (Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom) == screen.Bounds)
                return hwnd;
        }
        throw new Exception("No window for " + screen.DeviceName);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        int completed = 0, moveCount = 0, wheelCount = 0;
        try
        {
            Check(args.Length == 1, "Usage: VerifyMouseExit.exe <exe>");
            try { SetProcessDpiAwarenessContext(new IntPtr(-4)); }
            catch (EntryPointNotFoundException) { SetProcessDPIAware(); }
            Screen[] screens = Screen.AllScreens;
            Check(screens.Length > 0, "No displays available");
            ButtonCase[] buttons = new ButtonCase[] {
                new ButtonCase("Left",   0x0201, 0x0202, 0x0001, 0),
                new ButtonCase("Right",  0x0204, 0x0205, 0x0002, 0),
                new ButtonCase("Middle", 0x0207, 0x0208, 0x0010, 0),
                new ButtonCase("X1",     0x020B, 0x020C, 0x0020, 1 << 16),
                new ButtonCase("X2",     0x020B, 0x020C, 0x0040, 2 << 16)
            };

            foreach (ButtonCase button in buttons)
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
                        Thread.Sleep(400); // Let the launch-input arming timer finish.
                        List<IntPtr> windows = OverlayWindows(process, screens);
                        foreach (IntPtr hwnd in windows)
                        {
                            Rect windowRect, clientRect;
                            GetWindowRect(hwnd, out windowRect);
                            GetClientRect(hwnd, out clientRect);
                            Rectangle bounds = Rectangle.FromLTRB(windowRect.Left, windowRect.Top, windowRect.Right, windowRect.Bottom);
                            Check(clientRect.Left == 0 && clientRect.Top == 0
                                  && clientRect.Right == bounds.Width && clientRect.Bottom == bounds.Height,
                                "Expected borderless client bounds matching the window");
                        }

                        int caseMoves = 0;
                        foreach (Screen screen in screens)
                        {
                            Rectangle monitor = screen.Bounds;
                            IntPtr window = WindowForScreen(windows, screen);
                            Point[] locations = new Point[] {
                                new Point(monitor.Left, monitor.Top),
                                new Point(monitor.Right - 1, monitor.Top),
                                new Point(monitor.Left + monitor.Width / 2, monitor.Top + monitor.Height / 2),
                                new Point(monitor.Left, monitor.Bottom - 1),
                                new Point(monitor.Right - 1, monitor.Bottom - 1)
                            };
                            foreach (Point location in locations)
                            {
                                Post(window, 0x0200, 0, location.X - monitor.Left, location.Y - monitor.Top); // WM_MOUSEMOVE
                                caseMoves++; moveCount++;
                            }
                        }
                        StayOpen(process, "mouse movement across all displays (" + button.Name + ")");

                        Screen first = screens[0];
                        Rectangle firstRect = first.Bounds;
                        IntPtr firstWindow = WindowForScreen(windows, first);
                        int screenX = firstRect.Left + firstRect.Width / 2;
                        int screenY = firstRect.Top + firstRect.Height / 2;
                        // WM_MOUSEWHEEL uniquely expects screen coordinates, unlike move/button messages.
                        Post(firstWindow, 0x020A, 120 << 16, screenX, screenY);
                        Post(firstWindow, 0x020A, -120 << 16, screenX, screenY);
                        wheelCount += 2;
                        StayOpen(process, "scrolling both directions (" + button.Name + ")");

                        Post(firstWindow, button.Down, button.ExtraWord | button.HeldFlags, firstRect.Width / 2, firstRect.Height / 2);
                        StayOpen(process, button.Name + " button down without release");
                        foreach (Screen screen in screens)
                        {
                            Rectangle monitor = screen.Bounds;
                            IntPtr window = WindowForScreen(windows, screen);
                            Post(window, 0x0200, button.HeldFlags, monitor.Width / 2, monitor.Height / 2);
                            caseMoves++; moveCount++;
                        }
                        StayOpen(process, "movement while " + button.Name + " is held");

                        Post(firstWindow, button.Up, button.ExtraWord, firstRect.Width / 2, firstRect.Height / 2);
                        Check(process.WaitForExit(3000), button.Name + " button up did not close overlay");
                        Check(process.ExitCode == 0, "Overlay exited with an error after " + button.Name);
                        completed++;
                        Console.WriteLine("PASS: " + button.Name + ": " + caseMoves + " moves and 2 wheel messages kept open; down kept open; up exited");
                    }
                    finally
                    {
                        // Only terminate the exact process this iteration launched.
                        if (!process.HasExited)
                        {
                            try { process.Kill(); }
                            catch (InvalidOperationException) { }
                            Check(process.WaitForExit(3000), "Spawned overlay did not exit during cleanup");
                        }
                        if (previousWindow != IntPtr.Zero) SetForegroundWindow(previousWindow);
                    }
                }
            }
            Console.WriteLine("PASS TOTAL: " + completed + "/5 buttons; " + moveCount + " movement messages; " + wheelCount + " wheel messages; " + screens.Length + " windows per launch");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL after " + completed + "/5 buttons, " + moveCount + " moves, " + wheelCount + " wheel messages: " + ex.Message);
            return 1;
        }
    }
}

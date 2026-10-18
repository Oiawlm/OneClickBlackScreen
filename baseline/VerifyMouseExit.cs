using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

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

    [STAThread]
    private static int Main(string[] args)
    {
        int completed = 0, moveCount = 0, wheelCount = 0;
        try
        {
            Check(args.Length == 1, "Usage: VerifyMouseExit.exe <BlackScreen.exe>");
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
                        Check(visibleWindows == 1, "Expected exactly one visible overlay for " + button.Name);
                        Rect windowRect, clientRect;
                        Check(GetWindowRect(window, out windowRect), "Cannot inspect window bounds");
                        Check(GetClientRect(window, out clientRect), "Cannot inspect client bounds");
                        Rectangle bounds = Rectangle.FromLTRB(windowRect.Left, windowRect.Top, windowRect.Right, windowRect.Bottom);
                        Check(clientRect.Left == 0 && clientRect.Top == 0 && clientRect.Right == bounds.Width && clientRect.Bottom == bounds.Height,
                            "Expected borderless client bounds matching the window");

                        int caseMoves = 0;
                        foreach (Screen screen in screens)
                        {
                            Rectangle monitor = screen.Bounds;
                            Check(bounds.Contains(monitor), "Overlay does not contain " + screen.DeviceName);
                            Point[] locations = new Point[] {
                                new Point(monitor.Left, monitor.Top),
                                new Point(monitor.Right - 1, monitor.Top),
                                new Point(monitor.Left + monitor.Width / 2, monitor.Top + monitor.Height / 2),
                                new Point(monitor.Left, monitor.Bottom - 1),
                                new Point(monitor.Right - 1, monitor.Bottom - 1)
                            };
                            foreach (Point location in locations)
                            {
                                Post(window, 0x0200, 0, location.X - bounds.Left, location.Y - bounds.Top); // WM_MOUSEMOVE
                                caseMoves++; moveCount++;
                            }
                        }
                        StayOpen(process, "mouse movement across all displays (" + button.Name + ")");

                        Rectangle first = screens[0].Bounds;
                        int screenX = first.Left + first.Width / 2;
                        int screenY = first.Top + first.Height / 2;
                        int clientX = screenX - bounds.Left;
                        int clientY = screenY - bounds.Top;
                        // WM_MOUSEWHEEL uniquely expects screen coordinates, unlike move/button messages.
                        Post(window, 0x020A, 120 << 16, screenX, screenY);
                        Post(window, 0x020A, -120 << 16, screenX, screenY);
                        wheelCount += 2;
                        StayOpen(process, "scrolling both directions (" + button.Name + ")");

                        Post(window, button.Down, button.ExtraWord | button.HeldFlags, clientX, clientY);
                        StayOpen(process, button.Name + " button down without release");
                        foreach (Screen screen in screens)
                        {
                            Rectangle monitor = screen.Bounds;
                            clientX = monitor.Left + monitor.Width / 2 - bounds.Left;
                            clientY = monitor.Top + monitor.Height / 2 - bounds.Top;
                            Post(window, 0x0200, button.HeldFlags, clientX, clientY);
                            caseMoves++; moveCount++;
                        }
                        StayOpen(process, "movement while " + button.Name + " is held");

                        Post(window, button.Up, button.ExtraWord, clientX, clientY);
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
            Console.WriteLine("PASS TOTAL: " + completed + "/5 buttons; " + moveCount + " movement messages; " + wheelCount + " wheel messages; all input posted only to each spawned overlay HWND");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL after " + completed + "/5 buttons, " + moveCount + " moves, " + wheelCount + " wheel messages: " + ex.Message);
            return 1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// Settings-mode verifier: --settings must open exactly one normal (non-mask)
// window that can be closed; settings instances are not single-instance gated;
// unknown arguments must never show any mask window.
internal static class VerifySettings
{
    private const int WM_CLOSE = 0x0010;
    private const int WM_SETTEXT = 0x000C;
    private const int WM_GETTEXT = 0x000D;
    private const int WM_GETTEXTLENGTH = 0x000E;
    private const int EM_SETSEL = 0x00B1;
    private const uint WM_KEYDOWN = 0x0100;
    private const uint WM_KEYUP = 0x0101;
    private const int VK_RETURN = 0x0D;
    private const int GW_OWNER = 4;

    private delegate bool EnumProc(IntPtr hwnd, IntPtr data);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr hwnd, EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool IsWindowEnabled(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, StringBuilder text);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, string text);
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hwnd, int command);
    [DllImport("user32.dll")] private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool attach);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr SetFocus(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetFocus();
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
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

    // 5. The Enter contract (B1): inside the multiline box Enter must insert a
    // newline and never save; on the dialog itself Enter must still fire the
    // save button. Both branches deliver WM_KEYDOWN(VK_RETURN) at the message
    // level, like the mouse verifier - branch b attaches the input queues first
    // because dialog-key routing only runs for the focused window (EnterProbe2).
    // The user's config is snapshotted once and restored byte-for-byte at the
    // end, whatever happened inside.
    private static void CheckEnterContract(string exe)
    {
        string configPath = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "BlackScreen", "settings.ini");
        bool existed = File.Exists(configPath);
        byte[] original = existed ? File.ReadAllBytes(configPath) : null;
        try
        {
            CheckEnterNewlines(exe, configPath, existed, original);
            CheckEnterSaves(exe, configPath);
        }
        finally
        {
            if (existed) File.WriteAllBytes(configPath, original);
            else if (File.Exists(configPath)) File.Delete(configPath);
            bool restored = existed
                ? File.Exists(configPath) && BytesEqual(File.ReadAllBytes(configPath), original)
                : !File.Exists(configPath);
            Check(restored, "user config was not restored byte-for-byte");
        }
    }

    // Branch a: Enter delivered straight to the multiline EDIT.
    private static void CheckEnterNewlines(string exe, string configPath, bool existed, byte[] original)
    {
        using (Process process = Start(exe, "--settings"))
        {
            try
            {
                Check(process.WaitForInputIdle(5000), "settings UI did not become ready");
                IntPtr hwnd = WaitForVisibleWindows(process, 5000)[0];
                Thread.Sleep(300);
                IntPtr edit = FindEditChild(hwnd);
                // A controlled short newline-free baseline: whatever newline is
                // seen below can only come from the posted Enter, and buffer
                // capacity can never mask the outcome.
                Check(SendMessage(edit, WM_SETTEXT, IntPtr.Zero, "V3") != IntPtr.Zero,
                    "WM_SETTEXT baseline failed");
                StringBuilder baseline = new StringBuilder(64);
                SendMessage(edit, WM_GETTEXT, (IntPtr)64, baseline);
                Check(baseline.ToString() == "V3", "baseline readback failed: '" + baseline + "'");
                // WM_SETTEXT leaves the caret at the start; park it after the text
                // like a real typer so the newline lands at the end.
                int length = SendMessage(edit, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
                SendMessage(edit, EM_SETSEL, (IntPtr)length, (IntPtr)length);
                Check(PostMessage(edit, WM_KEYDOWN, new IntPtr(VK_RETURN), IntPtr.Zero),
                    "could not post Enter down");
                Check(PostMessage(edit, WM_KEYUP, new IntPtr(VK_RETURN), IntPtr.Zero),
                    "could not post Enter up");
                // Bounded observation window: keep watching for dialogs and exit
                // while waiting for the newline, then verify the config bytes.
                string after = "";
                for (int elapsed = 0; elapsed <= 2000; elapsed += 100)
                {
                    Thread.Sleep(100);
                    Check(!process.HasExited, "settings process exited after Enter in the text box");
                    Check(MessageBoxOf(process) == IntPtr.Zero,
                        "a dialog appeared after Enter in the text box");
                    StringBuilder current = new StringBuilder(128);
                    SendMessage(edit, WM_GETTEXT, (IntPtr)128, current);
                    after = current.ToString();
                    if (after.Length > 2) break;
                }
                Check(after.StartsWith("V3\r\n"),
                    "Enter did not become a newline in the text box: '" + Escape(after) + "'");
                Check(File.Exists(configPath) == existed, "config existence changed");
                if (existed) Check(BytesEqual(File.ReadAllBytes(configPath), original),
                    "config bytes changed while editing");
                CloseWindow(process, hwnd, "settings window");
            }
            finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(); } }
        }
        Console.WriteLine("PASS: Enter in the multiline box inserted a newline, raised no dialog, left the config untouched");
    }

    // Branch b: Enter delivered while a non-input child (the preview) holds
    // focus - the dialog-key routing must fall through to the save button and
    // run the save end to end. A focused BUTTON would consume Enter as its own
    // click and a focused EDIT consumes it as a newline, so the preview is the
    // faithful stand-in for "focus outside the text box".
    private static void CheckEnterSaves(string exe, string configPath)
    {
        // Controlled pre-state that differs from the expected result, in the
        // frozen ini format (UTF-8 without BOM, CRLF, uppercase color).
        Directory.CreateDirectory(Path.GetDirectoryName(configPath));
        File.WriteAllBytes(configPath, Encoding.UTF8.GetBytes("color=#123456\r\ntext=V3pre\r\n"));
        byte[] expected = Encoding.UTF8.GetBytes("color=#123456\r\ntext=V3ok\r\n");
        using (Process process = Start(exe, "--settings"))
        {
            try
            {
                Check(process.WaitForInputIdle(5000), "settings UI did not become ready");
                IntPtr hwnd = WaitForVisibleWindows(process, 5000)[0];
                Thread.Sleep(300);
                IntPtr edit = FindEditChild(hwnd);
                Check(SendMessage(edit, WM_SETTEXT, IntPtr.Zero, "V3ok") != IntPtr.Zero,
                    "WM_SETTEXT preset failed");
                IntPtr preview = FindPreviewChild(hwnd);
                uint pid;
                uint targetThread = GetWindowThreadProcessId(hwnd, out pid);
                Check(AttachThreadInput(GetCurrentThreadId(), targetThread, true),
                    "AttachThreadInput failed");
                try
                {
                    SetForegroundWindow(hwnd);
                    SetFocus(preview);
                    Check(GetFocus() == preview, "focus did not land on the preview");
                    Check(PostMessage(preview, WM_KEYDOWN, new IntPtr(VK_RETURN), IntPtr.Zero),
                        "could not post Enter down");
                    Check(PostMessage(preview, WM_KEYUP, new IntPtr(VK_RETURN), IntPtr.Zero),
                        "could not post Enter up");
                }
                finally { AttachThreadInput(GetCurrentThreadId(), targetThread, false); }
                IntPtr box = WaitForSavedMessageBox(process, hwnd, 5000);
                Check(File.Exists(configPath) && BytesEqual(File.ReadAllBytes(configPath), expected),
                    "config bytes after save differ from the expected transition");
                CloseWindow(process, box, "save confirmation box");
            }
            finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(); } }
        }
        Console.WriteLine("PASS: Enter on the dialog (focus outside the text box) saved the preset text via the confirmation path");
    }

    // The single non-EDIT non-BUTTON child: the preview panel (WinForms
    // superclasses Panel as "WindowsForms10.Window.8.app...").
    private static IntPtr FindPreviewChild(IntPtr hwnd)
    {
        List<IntPtr> found = new List<IntPtr>();
        EnumChildWindows(hwnd, delegate(IntPtr child, IntPtr data)
        {
            StringBuilder className = new StringBuilder(64);
            GetClassName(child, className, className.Capacity);
            string upper = className.ToString().ToUpperInvariant();
            if (upper.IndexOf(".WINDOW.8.") >= 0) found.Add(child);
            return true;
        }, IntPtr.Zero);
        Check(found.Count == 1, "expected exactly one preview panel child, saw " + found.Count);
        return found[0];
    }

    // The single visible enabled EDIT child of the settings window. WinForms
    // superclasses the native control as "WindowsForms10.EDIT.app...", so match
    // both spellings case-insensitively.
    private static bool IsEditClass(string className)
    {
        string upper = className.ToUpperInvariant();
        return upper == "EDIT" || upper.IndexOf(".EDIT.") >= 0;
    }

    private static IntPtr FindEditChild(IntPtr hwnd)
    {
        List<IntPtr> edits = new List<IntPtr>();
        EnumChildWindows(hwnd, delegate(IntPtr child, IntPtr data)
        {
            StringBuilder className = new StringBuilder(64);
            GetClassName(child, className, className.Capacity);
            if (IsEditClass(className.ToString()) && IsWindowVisible(child) && IsWindowEnabled(child))
                edits.Add(child);
            return true;
        }, IntPtr.Zero);
        Check(edits.Count == 1, "expected exactly one editable text child, saw " + edits.Count);
        return edits[0];
    }

    // First visible #32770 of the process (used to assert dialogs never appear).
    private static IntPtr MessageBoxOf(Process process)
    {
        foreach (IntPtr hwnd in VisibleWindows(process.Id))
        {
            StringBuilder className = new StringBuilder(64);
            GetClassName(hwnd, className, className.Capacity);
            if (className.ToString() == "#32770") return hwnd;
        }
        return IntPtr.Zero;
    }

    // Waits for the save confirmation box: #32770, owned by the settings window,
    // whose body text says the save happened (rules out the over-length and
    // save-failure boxes which carry different bodies).
    private static IntPtr WaitForSavedMessageBox(Process process, IntPtr owner, int timeoutMs)
    {
        for (int elapsed = 0; elapsed <= timeoutMs; elapsed += 50)
        {
            Check(!process.HasExited, "settings process exited before the save dialog appeared");
            foreach (IntPtr hwnd in VisibleWindows(process.Id))
            {
                StringBuilder className = new StringBuilder(64);
                GetClassName(hwnd, className, className.Capacity);
                if (className.ToString() != "#32770" || GetWindow(hwnd, GW_OWNER) != owner) continue;
                string body = StaticTextOf(hwnd);
                Check(body.IndexOf("已保存") >= 0, "save dialog body does not confirm the save: " + Escape(body));
                return hwnd;
            }
            Thread.Sleep(50);
        }
        throw new Exception("save confirmation dialog did not appear within " + timeoutMs + " ms");
    }

    // Concatenated text of the Static children of a dialog (the message body).
    private static string StaticTextOf(IntPtr dialog)
    {
        StringBuilder found = new StringBuilder();
        EnumChildWindows(dialog, delegate(IntPtr child, IntPtr data)
        {
            StringBuilder className = new StringBuilder(64);
            GetClassName(child, className, className.Capacity);
            string upper = className.ToString().ToUpperInvariant();
            if (upper == "STATIC" || upper.IndexOf(".STATIC.") >= 0)
            {
                StringBuilder text = new StringBuilder(256);
                SendMessage(child, WM_GETTEXT, (IntPtr)256, text);
                if (text.Length > 0) found.AppendLine(text.ToString());
            }
            return true;
        }, IntPtr.Zero);
        return found.ToString();
    }

    private static bool BytesEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    private static string Escape(string text)
    {
        return (text ?? "").Replace("\r", "\\r").Replace("\n", "\\n");
    }

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

            // 5. Enter inserts a newline in the text box and still saves on the dialog.
            CheckEnterContract(args[0]);

            Console.WriteLine("PASS TOTAL: 5 settings checks (3 original + theme background + Enter contract)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }
}

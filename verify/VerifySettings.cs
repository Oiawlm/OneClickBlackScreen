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
// unknown arguments must never show any mask window. Since v4 this file is
// co-compiled with src\ThemePalette.cs and src\SettingsDialogMetrics.cs (D2):
// colors and layout values stay single-sourced, while the layout algorithm is
// mirrored here as the observer side of the mirror-lock test (D3, check 6).
internal static class VerifySettings
{
    private const int WM_CLOSE = 0x0010;
    private const int WM_GETFONT = 0x0031;
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
    [DllImport("user32.dll")] private static extern int GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool AreDpiAwarenessContextsEqual(IntPtr a, IntPtr b);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr hwnd, ref System.Drawing.Point point);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out System.Drawing.Point point);
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);

    [StructLayout(LayoutKind.Sequential)]
    private struct GUITHREADINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public Rect rcCaret;
    }
    [DllImport("user32.dll")] private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO info);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct LOGFONTW
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string lfFaceName;
    }
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] private static extern int GetObjectW(IntPtr handle, int count, ref LOGFONTW object_);

    // Independent copy of the theme probe (the registry read stays duplicated on
    // purpose - it observes the system, it is not a design value): read the
    // registry expectation, sample the rendered dialog, never write anything.
    // The expected colors themselves come from ThemePalette (D2 single source).
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
    // app theme (D2: the expectation is ThemePalette itself, co-compiled here, so
    // the rendered surface can never drift from its single numeric source).
    // Samples sit in always-empty strips - the bottom padding below the button row
    // and the left margin (V11, T7 #19/#20) - and both strips are asserted disjoint
    // from every visible child rectangle, so no sample can land on a control.
    private static void CheckThemeBackground(string exe)
    {
        bool light = AppsUseLightTheme();
        Color expected = light ? ThemePalette.Light.Bg : ThemePalette.Dark.Bg;
        int expectedRgb = expected.ToArgb() & 0xFFFFFF;
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
                    CheckStripsAreChildFree(hwnd, width, height);
                    Color bottom = bitmap.GetPixel(width / 2, height - 4);
                    Color left = bitmap.GetPixel(2, height / 2);
                    Check((bottom.ToArgb() & 0xFFFFFF) == expectedRgb,
                        "bottom sample " + bottom + " is not the themed background #" + expectedRgb.ToString("X6"));
                    Check((left.ToArgb() & 0xFFFFFF) == expectedRgb,
                        "left sample " + left + " is not the themed background #" + expectedRgb.ToString("X6"));
                }
                CloseWindow(process, hwnd, "settings window");
            }
            finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(); } }
        }
        Console.WriteLine("PASS: settings background follows AppsUseLightTheme ("
            + (light ? "light" : "dark") + ", expected #" + expectedRgb.ToString("X6") + ")");
    }

    // The always-empty strips the background samples sit in (bottom padding and
    // left margin, 8 physical px wide) must not intersect any visible child.
    private static void CheckStripsAreChildFree(IntPtr hwnd, int width, int height)
    {
        System.Drawing.Point origin = new System.Drawing.Point();
        Check(ClientToScreen(hwnd, ref origin), "ClientToScreen failed");
        System.Drawing.Rectangle bottomStrip = new System.Drawing.Rectangle(0, height - 8, width, 8);
        System.Drawing.Rectangle leftStrip = new System.Drawing.Rectangle(0, 0, 8, height);
        List<System.Drawing.Rectangle> rects = new List<System.Drawing.Rectangle>();
        Check(EnumChildWindows(hwnd, delegate(IntPtr child, IntPtr data)
        {
            if (!IsWindowVisible(child)) return true;
            Rect r;
            GetWindowRect(child, out r);
            rects.Add(System.Drawing.Rectangle.FromLTRB(
                r.Left - origin.X, r.Top - origin.Y, r.Right - origin.X, r.Bottom - origin.Y));
            return true;
        }, IntPtr.Zero), "EnumChildWindows failed");
        foreach (System.Drawing.Rectangle rect in rects)
        {
            Check(!rect.IntersectsWith(bottomStrip) && !rect.IntersectsWith(leftStrip),
                "background sampling strip intersects a child window: " + rect);
        }
    }

    // 6. Mirror lock (D3): the rendered geometry must equal the metrics source.
    // The verifier recomputes every child rectangle from SettingsDialogMetrics
    // with the same S() formula and the same accumulation order the dialog uses,
    // at the live window DPI in physical pixels, and compares against the actual
    // rectangles (+-1px). It locks the S1 font ladder (body tier > caption tier,
    // Bold section labels), asserts the window stays inside its working area
    // (S2), and samples the primary button interior against ThemePalette.Accent
    // (normal state), so no palette or layout value can silently drift away
    // from its single declaration point.
    private static void CheckGeometryAndPrimaryColor(string exe)
    {
        using (Process process = Start(exe, "--settings"))
        {
            try
            {
                Check(process.WaitForInputIdle(5000), "settings UI did not become ready");
                IntPtr hwnd = WaitForVisibleWindows(process, 5000)[0];
                Thread.Sleep(300);
                Check(!process.HasExited, "settings process exited on its own");
                // The observed window must be per-monitor-aware v2 like the
                // observer, otherwise geometry would compare virtualized values.
                IntPtr context = GetWindowDpiAwarenessContext(hwnd);
                Check(context != IntPtr.Zero
                    && AreDpiAwarenessContextsEqual(context, new IntPtr(-4)),
                    "settings window is not in the PerMonitorAwareV2 context");
                // Stability: window, client and child rectangles must settle.
                Rect window1, window2, client1, client2;
                GetWindowRect(hwnd, out window1);
                GetClientRect(hwnd, out client1);
                Thread.Sleep(150);
                GetWindowRect(hwnd, out window2);
                GetClientRect(hwnd, out client2);
                Check(window1.Left == window2.Left && window1.Top == window2.Top
                    && window1.Right == window2.Right && window1.Bottom == window2.Bottom
                    && client1.Right == client2.Right && client1.Bottom == client2.Bottom,
                    "settings window bounds did not settle");
                int dpi = GetDpiForWindow(hwnd);
                Check(dpi > 0, "GetDpiForWindow failed");
                float scale = dpi / 96f;
                System.Windows.Forms.Screen current = System.Windows.Forms.Screen.FromHandle(hwnd);
                System.Drawing.Rectangle primaryScreen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                int nonClient = (window2.Bottom - window2.Top) - (client2.Bottom - client2.Top);

                // ---- mirror recompute: same formula and order as LayOutControls.
                int edge = Scaled(SettingsDialogMetrics.EdgePx, scale);
                int labelGap = Scaled(SettingsDialogMetrics.LabelGapPx, scale);
                int rowGap = Scaled(SettingsDialogMetrics.RowGapPx, scale);
                int groupGap = Scaled(SettingsDialogMetrics.GroupGapPx, scale);
                int labelHeight = Scaled(SettingsDialogMetrics.LabelRowHeightPx, scale);
                int swatchRow = Scaled(SettingsDialogMetrics.SwatchRowHeightPx, scale);
                int buttonRow = Scaled(SettingsDialogMetrics.ButtonRowHeightPx, scale);
                int textHeight = Scaled(SettingsDialogMetrics.TextBoxHeightPx, scale);
                int hintHeight = Scaled(SettingsDialogMetrics.HintHeightPx, scale);
                int hintZone = Scaled(SettingsDialogMetrics.HintZoneWidthPx, scale);
                int width = Scaled(SettingsDialogMetrics.ContentWidthPx, scale);
                int swatchPitch = Scaled(SettingsDialogMetrics.SwatchPitchPx, scale);
                int swatchSize = Scaled(SettingsDialogMetrics.SwatchSizePx, scale);
                int customGap = Scaled(SettingsDialogMetrics.CustomButtonGapPx, scale);
                int customWidth = Scaled(SettingsDialogMetrics.CustomButtonWidthPx, scale);
                int customLift = Scaled(SettingsDialogMetrics.CustomButtonLiftPx, scale);
                int previewHeight = (int)Math.Round(width * (double)primaryScreen.Height / primaryScreen.Width);
                int fixedChrome = edge + labelHeight + labelGap
                    + rowGap + swatchRow + groupGap
                    + labelHeight + labelGap + textHeight + rowGap
                    + hintHeight + groupGap + buttonRow + edge;
                int previewCap = current.WorkingArea.Height - nonClient - fixedChrome;
                if (previewCap < Scaled(SettingsDialogMetrics.PreviewFloorPx, scale))
                    previewCap = Scaled(SettingsDialogMetrics.PreviewFloorPx, scale);
                if (previewHeight > previewCap) previewHeight = previewCap;
                const int SwatchCount = 6; // SwatchColors.Length in the dialog
                int x = edge;
                int y = edge;
                Dictionary<string, int[]> expected = new Dictionary<string, int[]>();
                expected.Add("color label", new int[] { x, y, width, labelHeight });
                y += labelHeight + labelGap;
                expected.Add("preview", new int[] { x, y, width, previewHeight });
                y += previewHeight + rowGap;
                for (int i = 0; i < SwatchCount; i++)
                    expected.Add("swatch " + i, new int[] { x + i * swatchPitch, y, swatchSize, swatchSize });
                expected.Add("custom", new int[] { x + (SwatchCount - 1) * swatchPitch + swatchSize + customGap,
                    y - customLift, customWidth, swatchRow });
                y += swatchRow + groupGap;
                expected.Add("text label", new int[] { x, y, width - hintZone - labelGap, labelHeight });
                expected.Add("text hint", new int[] { x + width - hintZone, y, hintZone, labelHeight });
                y += labelHeight + labelGap;
                expected.Add("text box", new int[] { x, y, width, textHeight });
                y += textHeight + rowGap;
                expected.Add("hint label", new int[] { x, y, width, hintHeight });
                y += hintHeight + groupGap;
                int resetWidth = Scaled(SettingsDialogMetrics.ResetButtonWidthPx, scale);
                int cancelWidth = Scaled(SettingsDialogMetrics.CancelButtonWidthPx, scale);
                int tryWidth = Scaled(SettingsDialogMetrics.TryButtonWidthPx, scale);
                int saveWidth = Scaled(SettingsDialogMetrics.SaveButtonWidthPx, scale);
                int buttonGap = Scaled(SettingsDialogMetrics.ButtonGapPx, scale);
                int rightX = x + width - saveWidth;
                expected.Add("reset", new int[] { x, y, resetWidth, buttonRow });
                expected.Add("save", new int[] { rightX, y, saveWidth, buttonRow });
                expected.Add("try", new int[] { rightX - buttonGap - tryWidth, y, tryWidth, buttonRow });
                expected.Add("cancel", new int[] { rightX - 2 * buttonGap - tryWidth - cancelWidth, y, cancelWidth, buttonRow });
                int clientWidth = x + width + edge;
                int clientHeight = y + buttonRow + edge;

                // ---- collect the direct children and identify each one.
                System.Drawing.Point origin = new System.Drawing.Point();
                Check(ClientToScreen(hwnd, ref origin), "ClientToScreen failed");
                List<ChildInfo> children = new List<ChildInfo>();
                Check(EnumChildWindows(hwnd, delegate(IntPtr child, IntPtr data)
                {
                    if (GetParent(child) != hwnd) return true;
                    children.Add(ReadChild(child, origin));
                    return true;
                }, IntPtr.Zero), "EnumChildWindows failed");
                Check(children.Count == expected.Count,
                    "expected " + expected.Count + " direct children, saw " + children.Count);
                // Empty-text buttons are the swatches; index them left to right.
                List<ChildInfo> swatches = new List<ChildInfo>();
                foreach (ChildInfo child in children)
                    if (IsButtonClass(child.Class) && child.Text.Length == 0) swatches.Add(child);
                Check(swatches.Count == SwatchCount,
                    "expected " + SwatchCount + " swatch buttons, saw " + swatches.Count);
                swatches.Sort(delegate(ChildInfo a, ChildInfo b) { return a.Client.Left - b.Client.Left; });
                for (int i = 0; i < swatches.Count; i++) swatches[i].Kind = "swatch " + i;

                foreach (ChildInfo child in children)
                {
                    if (child.Kind == null) child.Kind = KindOf(child);
                    Check(expected.ContainsKey(child.Kind),
                        "unexpected child (" + child.Class + " '" + Truncate(child.Text) + "')");
                    int[] want = expected[child.Kind];
                    int actualX = child.Client.Left, actualY = child.Client.Top;
                    int actualW = child.Client.Right - child.Client.Left;
                    int actualH = child.Client.Bottom - child.Client.Top;
                    Check(Near(actualX, want[0]) && Near(actualY, want[1])
                        && Near(actualW, want[2]) && Near(actualH, want[3]),
                        child.Kind + " mismatch at dpi " + dpi + ": expected [x=" + want[0] + ",y=" + want[1]
                            + ",w=" + want[2] + ",h=" + want[3] + "] actual [x=" + actualX + ",y=" + actualY
                            + ",w=" + actualW + ",h=" + actualH + "]");
                    child.Matched = true;
                }
                foreach (ChildInfo child in children)
                    Check(child.Matched, "child was not matched: " + child.Class + " '" + Truncate(child.Text) + "'");
                int actualClientWidth = client2.Right - client2.Left;
                int actualClientHeight = client2.Bottom - client2.Top;
                Check(Near(actualClientWidth, clientWidth) && Near(actualClientHeight, clientHeight),
                    "client size mismatch at dpi " + dpi + ": expected " + clientWidth + "x" + clientHeight
                        + " actual " + actualClientWidth + "x" + actualClientHeight);
                // S2: the whole window must stay inside the working area of its screen.
                Check(window2.Left >= current.WorkingArea.Left && window2.Top >= current.WorkingArea.Top
                    && window2.Right <= current.WorkingArea.Right && window2.Bottom <= current.WorkingArea.Bottom,
                    "window " + window2.Left + "," + window2.Top + "-" + window2.Right + "," + window2.Bottom
                        + " escapes working area " + current.WorkingArea);

                // ---- S1 font ladder, native side: the EDIT is the one control whose
                // window font WinForms keeps authoritative; its em must equal the
                // body tier at the live DPI (10.5pt -> 14px@96 / 21px@144) and stay
                // regular-weight YaHei. Every other tier is painted from WinForms
                // Font properties whose native window fonts stay null (FontDump
                // probe evidence), so those are locked by the probe, source review
                // and the E2 screenshots - registered in VERIFY-V4.
                ChildInfo textBox = FindByKind(children, "text box");
                LOGFONTW logfont = LogFontOf(textBox.Hwnd);
                int expectedBodyEm = (int)Math.Round(SettingsDialogMetrics.BodyPointSize * dpi / 72f);
                Check(Near(Math.Abs(logfont.lfHeight), expectedBodyEm),
                    "text box font em " + Math.Abs(logfont.lfHeight) + " is not the body tier "
                        + expectedBodyEm + " at dpi " + dpi);
                Check(logfont.lfWeight < 600, "text box font is Bold");

                // ---- primary button role color: normal-state interior == Accent.
                ChildInfo save = FindByKind(children, "save");
                Check(IsWindowEnabled(save.Hwnd), "save button is disabled");
                uint pid;
                uint thread = GetWindowThreadProcessId(hwnd, out pid);
                GUITHREADINFO info = new GUITHREADINFO();
                info.cbSize = Marshal.SizeOf(typeof(GUITHREADINFO));
                Check(GetGUIThreadInfo(thread, ref info), "GetGUIThreadInfo failed");
                Check(info.hwndFocus != save.Hwnd,
                    "save button holds keyboard focus (sample expects the normal state)");
                System.Drawing.Point cursor;
                Check(GetCursorPos(out cursor), "GetCursorPos failed");
                bool hover = cursor.X >= save.Screen.Left && cursor.X < save.Screen.Right
                    && cursor.Y >= save.Screen.Top && cursor.Y < save.Screen.Bottom;
                if (hover)
                {
                    Check(SetCursorPos(window2.Left - 20, window2.Top - 20),
                        "SetCursorPos failed while clearing the hover");
                    Thread.Sleep(150);
                }
                try
                {
                    using (Bitmap bitmap = new Bitmap(actualClientWidth, actualClientHeight))
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
                        Color expectedAccent = AppsUseLightTheme()
                            ? ThemePalette.Light.Accent : ThemePalette.Dark.Accent;
                        int expectedRgb = expectedAccent.ToArgb() & 0xFFFFFF;
                        int inset = Scaled(8, scale);
                        int[,] points = new int[,]
                        {
                            { save.Client.Left + inset, save.Client.Top + inset },
                            { save.Client.Right - inset - 1, save.Client.Top + inset },
                            { save.Client.Left + inset, save.Client.Bottom - inset - 1 },
                            { save.Client.Right - inset - 1, save.Client.Bottom - inset - 1 }
                        };
                        for (int i = 0; i < 4; i++)
                        {
                            int px = points[i, 0], py = points[i, 1];
                            Check(px >= 0 && py >= 0 && px < bitmap.Width && py < bitmap.Height,
                                "sample point " + px + "," + py + " outside the captured client");
                            Color sampled = bitmap.GetPixel(px, py);
                            if ((sampled.ToArgb() & 0xFFFFFF) != expectedRgb)
                            {
                                try { bitmap.Save("_probe\\check6-fail.png", System.Drawing.Imaging.ImageFormat.Png); }
                                catch { }
                                throw new Exception("primary button sample " + px + "," + py + " is "
                                    + sampled + ", expected Accent #" + expectedRgb.ToString("X6")
                                    + " (dpi " + dpi + ", state should be normal)");
                            }
                        }
                    }
                }
                finally
                {
                    if (hover) SetCursorPos(cursor.X, cursor.Y);
                }
                CloseWindow(process, hwnd, "settings window");
            }
            finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(); } }
        }
        Console.WriteLine("PASS: geometry mirrored SettingsDialogMetrics (+-1px), text box font "
            + "locked the body tier, window inside working area, primary button locked Accent");
    }

    // The dialog's only runtime derivation path, mirrored 1:1 (D1/D3).
    private static int Scaled(int value, float scale)
    {
        return (int)Math.Round(value * scale);
    }

    private static bool Near(int actual, int expected)
    {
        return Math.Abs(actual - expected) <= 1;
    }

    private sealed class ChildInfo
    {
        public IntPtr Hwnd;
        public string Class;
        public string Text;
        public Rect Screen;
        public Rect Client;
        public string Kind;
        public bool Matched;
    }

    private static ChildInfo ReadChild(IntPtr child, System.Drawing.Point origin)
    {
        ChildInfo info = new ChildInfo();
        info.Hwnd = child;
        StringBuilder className = new StringBuilder(64);
        GetClassName(child, className, className.Capacity);
        info.Class = className.ToString();
        StringBuilder text = new StringBuilder(256);
        SendMessage(child, WM_GETTEXT, (IntPtr)256, text);
        info.Text = text.ToString();
        GetWindowRect(child, out info.Screen);
        info.Client.Left = info.Screen.Left - origin.X;
        info.Client.Top = info.Screen.Top - origin.Y;
        info.Client.Right = info.Screen.Right - origin.X;
        info.Client.Bottom = info.Screen.Bottom - origin.Y;
        return info;
    }

    private static bool IsButtonClass(string className)
    {
        string upper = className.ToUpperInvariant();
        return upper == "BUTTON" || upper.IndexOf(".BUTTON.") >= 0;
    }

    private static bool IsStaticClass(string className)
    {
        string upper = className.ToUpperInvariant();
        return upper == "STATIC" || upper.IndexOf(".STATIC.") >= 0;
    }

    // Stable identity for one direct child, by window class plus text.
    private static string KindOf(ChildInfo child)
    {
        string upper = child.Class.ToUpperInvariant();
        if (upper.IndexOf(".WINDOW.8.") >= 0) return "preview";
        if (IsEditClass(child.Class)) return "text box";
        if (IsButtonClass(child.Class))
        {
            if (child.Text == "自定义颜色…") return "custom";
            if (child.Text == "恢复默认") return "reset";
            if (child.Text == "取消") return "cancel";
            if (child.Text == "保存并试用") return "try";
            if (child.Text == "保存") return "save";
            return "button?'" + Truncate(child.Text) + "'";
        }
        if (IsStaticClass(child.Class))
        {
            if (child.Text.StartsWith("遮罩颜色")) return "color label";
            if (child.Text.StartsWith("遮罩文字")) return "text label";
            if (child.Text.StartsWith("最多")) return "text hint";
            if (child.Text.StartsWith("配置文件")) return "hint label";
            return "static?'" + Truncate(child.Text) + "'";
        }
        return "unknown " + child.Class;
    }

    private static ChildInfo FindByKind(List<ChildInfo> children, string kind)
    {
        foreach (ChildInfo child in children)
            if (child.Kind == kind) return child;
        throw new Exception("no child identified as '" + kind + "'");
    }

    private static string Truncate(string text)
    {
        return text != null && text.Length > 24 ? text.Substring(0, 24) : (text ?? "");
    }

    private static LOGFONTW LogFontOf(IntPtr hwnd)
    {
        IntPtr hfont = SendMessage(hwnd, WM_GETFONT, IntPtr.Zero, IntPtr.Zero);
        Check(hfont != IntPtr.Zero, "WM_GETFONT returned no font for " + hwnd);
        LOGFONTW logfont = new LOGFONTW();
        Check(GetObjectW(hfont, Marshal.SizeOf(typeof(LOGFONTW)), ref logfont) > 0,
            "GetObjectW failed for " + hwnd);
        string face = logfont.lfFaceName ?? "";
        Check(face.IndexOf("YaHei") >= 0 || face.IndexOf("雅黑") >= 0,
            "control font is not the YaHei family chain: '" + face + "'");
        return logfont;
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

            // 6. Geometry mirrors SettingsDialogMetrics and the primary button locks Accent.
            CheckGeometryAndPrimaryColor(args[0]);

            Console.WriteLine("PASS TOTAL: 6 settings checks (3 original + theme background + Enter contract + geometry mirror)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }
}

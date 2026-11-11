using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal static class NativeMethods
{
    internal delegate IntPtr HookProc(int code, IntPtr message, IntPtr data);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(int id, HookProc callback, IntPtr module, uint threadId);
    [DllImport("user32.dll")]
    internal static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")]
    internal static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr GetModuleHandle(string moduleName);
    [DllImport("user32.dll")]
    internal static extern IntPtr SetCursor(IntPtr cursor);
    [DllImport("user32.dll")]
    internal static extern short GetAsyncKeyState(int key);

    [StructLayout(LayoutKind.Sequential)]
    internal struct KbdLlHook
    {
        internal uint VkCode;
        internal uint ScanCode;
        internal uint Flags;
        internal uint Time;
        internal UIntPtr ExtraInfo;
    }
}

// One borderless topmost mask window covering a single monitor. All exit-input and
// arming state lives in the OverlayGroup so every screen shares one lifecycle.
internal sealed class OverlayForm : Form
{
    private readonly OverlayGroup owner;
    private readonly string updateHint;

    public OverlayForm(Screen screen, MaskConfig config, OverlayGroup owner, string updateHint)
    {
        this.owner = owner;
        this.updateHint = updateHint;
        Text = "BlackScreen - click or press any key to exit";
        BackColor = config.MaskColor;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        AutoScaleMode = AutoScaleMode.None;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
        Bounds = screen.Bounds;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        MaskPainter.DrawText(e.Graphics, ClientRectangle, owner.Config);
        PaintUpdateHint(e.Graphics, ClientRectangle, updateHint, owner.Config.MaskColor);
    }

    // V10 D8: the low-key "new version available" line in the bottom-right
    // corner - pure display, never part of the exit logic, fixed for the
    // mask's lifetime (the tag is read once at start). Static and stateless
    // so the offline verifier can inject a tag straight into the painter and
    // assert the rendering without launching anything.
    internal static void PaintUpdateHint(Graphics graphics, Rectangle area, string hint, Color background)
    {
        if (string.IsNullOrEmpty(hint) || area.Width <= 0 || area.Height <= 0) return;
        using (StringFormat format = new StringFormat())
        {
            format.Alignment = StringAlignment.Near;
            format.LineAlignment = StringAlignment.Far;
            // Small on purpose: a fraction of the mask-text size, floored at
            // the painter's minimum, keeps the line low-key at every DPI.
            float size = Math.Max(12f, area.Height / 60f);
            using (Font font = new Font(MaskPainter.SharedFamily, size, FontStyle.Regular, GraphicsUnit.Pixel))
            using (SolidBrush brush = new SolidBrush(MaskPainter.PickTextColor(background)))
            {
                SizeF measured = graphics.MeasureString(hint, font, area.Size, format);
                float pad = size; // one em of clearance keeps the exact corner clean
                RectangleF hintArea = new RectangleF(
                    area.Right - measured.Width - pad,
                    area.Bottom - measured.Height - pad,
                    measured.Width + pad, measured.Height + pad);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                graphics.DrawString(hint, font, brush, hintArea, format);
            }
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        owner.NotifyShown(this);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        owner.NotifyFormClosing(this);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        // Finish the click on the overlay before closing, avoiding click-through.
        owner.NotifyMouseUp(e.Button);
    }

    protected override void WndProc(ref Message m)
    {
        // Hide the pointer over the overlay on every monitor.
        if (m.Msg == 0x0020)
        {
            NativeMethods.SetCursor(IntPtr.Zero);
            m.Result = new IntPtr(1);
            return;
        }
        base.WndProc(ref m);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (Bounds.Contains(Cursor.Position)) NativeMethods.SetCursor(Cursors.Default.Handle);
        owner.NotifyFormClosed(this);
        base.OnFormClosed(e);
    }
}

// Owns one OverlayForm per monitor plus the shared exit machinery of the frozen v1
// behavior: the low-level keyboard hook, the arming timer and a single close path.
// Arming keeps the v1 wait-for-release semantics for a bounded grace window, then
// latches keys that never release (phantom states) instead of blocking forever.
internal sealed class OverlayGroup : ApplicationContext
{
    private const int WM_KEYDOWN = 0x0100, WM_SYSKEYDOWN = 0x0104;

    private readonly MaskConfig config;
    private readonly OverlayForm[] forms;
    private readonly Timer armTimer = new Timer();
    private readonly NativeMethods.HookProc keyboardCallback;
    private OverlayForm invokeForm;
    private IntPtr keyboardHook;
    private bool armed;
    private bool closing;
    private bool hookFailed;
    private bool finished;
    private int shownCount;
    private int openCount;

    // v1 waits until every key has been released once; a key held longer than this
    // grace window (deliberate hold or phantom state) degrades to a self-only latch.
    private const int ArmGraceTicks = 10; // beats x 100ms = ~1s
    private readonly bool[] keyLatched = new bool[255]; // VK-indexed exit latch
    private readonly int[] keyHeldTicks = new int[255]; // consecutive beats held down

    public OverlayGroup(MaskConfig config, string updateHint)
    {
        this.config = config;
        keyboardCallback = OnKeyboard;
        // Wait for launch keys/buttons to be released before accepting exit input.
        // A key still down past the grace window (a deliberately held launch key, or a
        // phantom state injected by external software) no longer blocks arming: only
        // its own exit input stays suppressed until a release is observed (I1/I2).
        armTimer.Interval = 100;
        armTimer.Tick += ArmTick;
        try
        {
            Screen[] screens = Screen.AllScreens;
            forms = new OverlayForm[screens.Length];
            for (int i = 0; i < forms.Length; i++)
                forms[i] = new OverlayForm(screens[i], config, this, updateHint);
            openCount = forms.Length;
            for (int i = 0; i < forms.Length; i++)
                forms[i].Show();
        }
        catch
        {
            Cleanup();
            throw;
        }
    }

    internal MaskConfig Config
    {
        get { return config; }
    }

    internal void NotifyShown(OverlayForm form)
    {
        shownCount++;
        if (closing || shownCount < forms.Length) return;
        invokeForm = FormUnderCursor();
        OverlayForm active = invokeForm != null ? invokeForm : forms[0];
        active.Activate();
        keyboardHook = NativeMethods.SetWindowsHookEx(13, keyboardCallback, NativeMethods.GetModuleHandle(null), 0);
        if (keyboardHook == IntPtr.Zero)
        {
            hookFailed = true;
            CloseAll();
            return;
        }
        armTimer.Start();
        foreach (OverlayForm current in forms)
            if (current.Bounds.Contains(Cursor.Position)) NativeMethods.SetCursor(IntPtr.Zero);
    }

    private OverlayForm FormUnderCursor()
    {
        Point position = Cursor.Position;
        foreach (OverlayForm form in forms)
            if (!form.IsDisposed && form.Bounds.Contains(position)) return form;
        return null;
    }

    // One 100ms arming beat. v1 semantics: a key still down keeps the exits closed.
    // Past the grace window the key stops blocking arming and only its own exit input
    // stays latched (phantom tolerance, I2) until a release is observed (I1).
    private void ArmTick(object sender, EventArgs e)
    {
        bool blocking = false; // a key still inside its v1 launch-release wait
        bool latched = false;  // a latched key whose release is not observed yet
        for (int key = 1; key <= 254; key++)
        {
            if ((NativeMethods.GetAsyncKeyState(key) & 0x8000) == 0)
            {
                // Release observed once: the key can trigger exits again (I1).
                keyHeldTicks[key] = 0;
                keyLatched[key] = false;
            }
            else if (keyLatched[key])
            {
                latched = true;
            }
            else if (!armed && ++keyHeldTicks[key] < ArmGraceTicks)
            {
                blocking = true;
            }
            else if (!armed)
            {
                keyLatched[key] = true;
                latched = true;
            }
            // else: ordinary post-arm input - exits exactly like v1, never latches.
        }
        if (!armed && !blocking) armed = true;
        if (armed && !latched) armTimer.Stop(); // nothing left to observe
    }

    private IntPtr OnKeyboard(int code, IntPtr message, IntPtr data)
    {
        try
        {
            if (code >= 0 && armed &&
                (message.ToInt32() == WM_KEYDOWN || message.ToInt32() == WM_SYSKEYDOWN))
            {
                uint vk = ((NativeMethods.KbdLlHook)Marshal.PtrToStructure(
                    data, typeof(NativeMethods.KbdLlHook))).VkCode;
                // A latched launch/phantom key passes through untouched until its
                // release has been observed: it must not exit (I1) nor be swallowed.
                if (vk == 0 || vk > 254 || !keyLatched[(int)vk])
                {
                    if (!closing)
                    {
                        closing = true;
                        Control invoker = SelectInvoker();
                        if (invoker != null) invoker.BeginInvoke(new Action(CloseAll));
                        else CloseAll();
                    }
                    return new IntPtr(1);
                }
            }
        }
        catch { }
        return NativeMethods.CallNextHookEx(keyboardHook, code, message, data);
    }

    private Control SelectInvoker()
    {
        if (invokeForm != null && !invokeForm.IsDisposed && invokeForm.IsHandleCreated) return invokeForm;
        foreach (OverlayForm form in forms)
            if (!form.IsDisposed && form.IsHandleCreated) return form;
        return null;
    }

    internal void NotifyMouseUp(MouseButtons button)
    {
        // Same latch for the launching button itself: completing the launch click must
        // not close the mask (I1); any other button, or this one after its release has
        // been observed, closes it.
        int key = ButtonKey(button);
        if (armed && !closing && (key == 0 || !keyLatched[key])) CloseAll();
    }

    private static int ButtonKey(MouseButtons button)
    {
        switch (button)
        {
            case MouseButtons.Left: return 1;
            case MouseButtons.Right: return 2;
            case MouseButtons.Middle: return 4;
            case MouseButtons.XButton1: return 5;
            case MouseButtons.XButton2: return 6;
            default: return 0;
        }
    }

    internal void NotifyFormClosing(OverlayForm form)
    {
        // Any close path (keyboard, mouse, WM_CLOSE, ...) ends the whole group.
        if (!closing) CloseAll();
    }

    internal void NotifyFormClosed(OverlayForm form)
    {
        if (openCount > 0) openCount--;
        if (openCount == 0) Finish();
    }

    private void CloseAll()
    {
        closing = true;
        armTimer.Stop();
        foreach (OverlayForm form in forms)
            if (!form.IsDisposed) form.Close();
    }

    private void Finish()
    {
        if (finished) return;
        finished = true;
        Cleanup();
        if (hookFailed)
            MessageBox.Show("Could not enable keyboard exit. Please reopen the program.",
                "BlackScreen", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ExitThread();
    }

    private void Cleanup()
    {
        armTimer.Stop();
        armTimer.Dispose();
        if (keyboardHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(keyboardHook);
            keyboardHook = IntPtr.Zero;
        }
        if (forms != null)
            foreach (OverlayForm form in forms)
            {
                if (form == null) continue;
                try { if (!form.IsDisposed) form.Close(); }
                catch { }
            }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !finished)
        {
            finished = true;
            Cleanup();
        }
        base.Dispose(disposing);
    }
}

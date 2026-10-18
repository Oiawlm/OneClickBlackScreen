using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal sealed class BlackScreen : Form
{
    private const int WM_KEYDOWN = 0x0100, WM_SYSKEYDOWN = 0x0104;
    private readonly HookProc keyboardCallback;
    private IntPtr keyboardHook;
    private bool closing;
    private readonly Timer armTimer = new Timer();
    private bool armed;

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using (BlackScreen window = new BlackScreen())
            Application.Run(window);
    }

    private BlackScreen()
    {
        Text = "BlackScreen - click or press any key to exit";
        BackColor = Color.Black;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        AutoScaleMode = AutoScaleMode.None;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);

        Bounds = SystemInformation.VirtualScreen;

        keyboardCallback = OnKeyboard;
        // Wait for launch keys/buttons to be released before accepting exit input.
        armTimer.Interval = 100;
        armTimer.Tick += delegate
        {
            for (int key = 1; key <= 254; key++)
                if ((GetAsyncKeyState(key) & 0x8000) != 0) return;
            armed = true;
            armTimer.Stop();
        };
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
        keyboardHook = SetWindowsHookEx(13, keyboardCallback, GetModuleHandle(null), 0);
        if (keyboardHook == IntPtr.Zero)
        {
            Close();
            MessageBox.Show("Could not enable keyboard exit. Please reopen the program.",
                            "BlackScreen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        armTimer.Start();
        if (Bounds.Contains(Cursor.Position)) SetCursor(IntPtr.Zero);
    }

    private IntPtr OnKeyboard(int code, IntPtr message, IntPtr data)
    {
        if (code >= 0 && armed &&
            (message.ToInt32() == WM_KEYDOWN || message.ToInt32() == WM_SYSKEYDOWN))
        {
            if (!closing)
            {
                closing = true;
                BeginInvoke(new Action(Close));
            }
            return new IntPtr(1);
        }
        return CallNextHookEx(keyboardHook, code, message, data);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        // Finish the click on the overlay before closing, avoiding click-through.
        if (armed && !closing)
        {
            closing = true;
            Close();
        }
    }

    protected override void WndProc(ref Message m)
    {
        // Hide the pointer over the overlay on every monitor.
        if (m.Msg == 0x0020)
        {
            SetCursor(IntPtr.Zero);
            m.Result = new IntPtr(1);
            return;
        }
        base.WndProc(ref m);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        armTimer.Stop();
        armTimer.Dispose();
        if (keyboardHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(keyboardHook);
            keyboardHook = IntPtr.Zero;
        }
        if (Bounds.Contains(Cursor.Position)) SetCursor(Cursors.Default.Handle);
        base.OnFormClosed(e);
    }

    private delegate IntPtr HookProc(int code, IntPtr message, IntPtr data);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int id, HookProc callback, IntPtr module, uint threadId);
    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string moduleName);
    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr cursor);
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);
}

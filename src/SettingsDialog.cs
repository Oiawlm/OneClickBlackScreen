using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Settings window opened via "--settings". Edits a working copy of MaskConfig and
// only persists it on save; changes apply the next time the mask starts (D9).
// The chrome follows the system light/dark app theme (V1-V4): the palette recolors
// the existing controls in place on WM_SETTINGCHANGE (debounced), while preview,
// swatch and mask colors always come from the configuration only.
internal sealed class SettingsDialog : Form
{
    private static readonly Color[] SwatchColors = new Color[]
    {
        Color.Black,
        Color.FromArgb(0x40, 0x40, 0x40),
        Color.FromArgb(0x1F, 0x3A, 0x5F),
        Color.FromArgb(0x1E, 0x3B, 0x2E),
        Color.FromArgb(0x4A, 0x1F, 0x1F),
        Color.White
    };

    private const int WM_SETTINGCHANGE = 0x001A;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    private readonly PreviewPanel preview;
    private readonly Button[] swatches = new Button[SwatchColors.Length];
    private readonly Button customColorButton = new Button();
    private readonly Label colorLabel = new Label();
    private readonly Label textLabel = new Label();
    private readonly TextBox textBox = new TextBox();
    private readonly Label hintLabel = new Label();
    private readonly Button resetButton = new Button();
    private readonly Button cancelButton = new Button();
    private readonly Button saveButton = new Button();
    private readonly Button tryButton = new Button();
    private readonly Timer themeTimer = new Timer();
    private MaskConfig draft;
    private float dpiScale = 1f;
    private bool lightTheme = ThemePalette.AppsUseLightTheme();
    private ThemePalette palette;
    private Icon embeddedIcon;
    private bool themeTimerDisposed;

    [DllImport("user32.dll")]
    private static extern int GetDpiForWindow(IntPtr hwnd);
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hwnd, string subApplication, string subIdList);

    public SettingsDialog()
    {
        draft = MaskConfig.Load();
        palette = lightTheme ? ThemePalette.Light : ThemePalette.Dark;
        Text = "一键黑屏 · 设置";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.None;
        ShowIcon = true;
        LoadEmbeddedIcon();

        preview = new PreviewPanel(draft);
        colorLabel.Text = "遮罩颜色（点击色块选择，或自定义）：";
        colorLabel.AutoSize = false;
        for (int i = 0; i < SwatchColors.Length; i++)
        {
            swatches[i] = new Button();
            swatches[i].BackColor = SwatchColors[i];
            swatches[i].Tag = SwatchColors[i];
            swatches[i].FlatStyle = FlatStyle.Flat;
            swatches[i].Cursor = Cursors.Hand;
            swatches[i].TabStop = false;
            swatches[i].Click += delegate(object sender, EventArgs e)
            {
                ApplyColor((Color)((Control)sender).Tag);
            };
        }
        customColorButton.Text = "自定义颜色…";
        customColorButton.Click += delegate { PickCustomColor(); };
        textLabel.Text = "遮罩文字（最多 " + MaskConfig.MaxTextLength + " 字，回车换行，留空则不显示）：";
        textLabel.AutoSize = false;
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Vertical;
        textBox.MaxLength = MaskConfig.MaxTextLength;
        textBox.Text = draft.MaskText;
        textBox.TextChanged += delegate
        {
            draft = new MaskConfig(draft.MaskColor, MaskConfig.NormalizeNewlines(textBox.Text));
            preview.UpdateConfig(draft);
        };
        hintLabel.Text = "配置文件：" + MaskConfig.SettingsPath + "\n保存后，下一次启动遮罩时生效；退出交互与旧版完全一致。";
        hintLabel.AutoSize = false;
        resetButton.Text = "恢复默认";
        resetButton.Click += delegate { ResetToDefault(); };
        cancelButton.Text = "取消";
        cancelButton.DialogResult = DialogResult.Cancel;
        saveButton.Text = "保存";
        saveButton.Click += delegate { SaveAndClose(false); };
        tryButton.Text = "保存并试用";
        tryButton.Click += delegate { SaveAndClose(true); };

        // Theme switching: any WM_SETTINGCHANGE restarts the debounce; after it
        // settles, reread the registry and only reapply on a real change (V3).
        themeTimer.Interval = 150;
        themeTimer.Tick += delegate
        {
            themeTimer.Stop();
            if (IsDisposed || Disposing) return;
            bool light = ThemePalette.AppsUseLightTheme();
            if (light != lightTheme) ApplyTheme(light);
        };
        // The textbox needs its own theme on first handle creation and on any
        // handle recreation; the parent's OnHandleCreated runs before children
        // have handles (T7 #6).
        textBox.HandleCreated += delegate { ApplyTextBoxTheme(); };
        ApplyPalette();

        Controls.Add(preview);
        Controls.Add(colorLabel);
        for (int i = 0; i < swatches.Length; i++) Controls.Add(swatches[i]);
        Controls.Add(customColorButton);
        Controls.Add(textLabel);
        Controls.Add(textBox);
        Controls.Add(hintLabel);
        Controls.Add(resetButton);
        Controls.Add(cancelButton);
        Controls.Add(saveButton);
        Controls.Add(tryButton);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        Load += delegate { LayOutControls(); };
    }

    private int S(int value)
    {
        return (int)Math.Round(value * dpiScale);
    }

    private void LayOutControls()
    {
        // WinForms without an app.config DPI section reports 96 via DeviceDpi even in
        // a PerMonitorV2 process; ask the window itself for its real monitor DPI.
        int dpi = DeviceDpi;
        try
        {
            int monitorDpi = GetDpiForWindow(Handle);
            if (monitorDpi > 0) dpi = monitorDpi;
        }
        catch (EntryPointNotFoundException) { }
        dpiScale = dpi / 96f;
        // Point sizes already scale with the window DPI (measured 9pt = 18px em at
        // 144 DPI), so only the layout values are scaled by dpiScale - one scaling
        // path, no double scaling (V7 erratum).
        Font = new Font(MaskPainter.SharedFamily, 9f, FontStyle.Regular, GraphicsUnit.Point);
        Rectangle primary = Screen.PrimaryScreen.Bounds;
        int previewHeight = (int)Math.Round(S(408) * (double)primary.Height / primary.Width);
        int x = S(16);
        int y = S(12);
        int width = S(408);
        colorLabel.SetBounds(x, y, width, S(20));
        y += S(26);
        preview.SetBounds(x, y, width, previewHeight);
        y += previewHeight + S(14);
        for (int i = 0; i < swatches.Length; i++)
            swatches[i].SetBounds(x + i * S(36), y, S(28), S(28));
        customColorButton.SetBounds(x + swatches.Length * S(36) + S(8), y - S(3), S(116), S(34));
        y += S(42);
        textLabel.SetBounds(x, y, width, S(20));
        y += S(26);
        textBox.SetBounds(x, y, width, S(64));
        y += S(64) + S(12);
        hintLabel.SetBounds(x, y, width, S(40));
        y += S(46);
        int resetWidth = S(88), cancelWidth = S(75), tryWidth = S(104), saveWidth = S(75), gap = S(8);
        int startX = x + width - resetWidth - cancelWidth - tryWidth - saveWidth - 3 * gap;
        resetButton.SetBounds(startX, y, resetWidth, S(32));
        cancelButton.SetBounds(startX + resetWidth + gap, y, cancelWidth, S(32));
        tryButton.SetBounds(startX + resetWidth + cancelWidth + 2 * gap, y, tryWidth, S(32));
        saveButton.SetBounds(startX + resetWidth + cancelWidth + tryWidth + 3 * gap, y, saveWidth, S(32));
        ClientSize = new Size(x + width + S(16), y + S(32) + S(12));
        // CenterScreen placed the form by its initial default size; re-center after
        // the real layout so the intent holds.
        Screen current = Screen.FromControl(this);
        Location = new Point(current.Bounds.X + (current.Bounds.Width - Width) / 2,
            current.Bounds.Y + (current.Bounds.Height - Height) / 2);
    }

    private void LoadEmbeddedIcon()
    {
        try
        {
            using (Stream stream = typeof(SettingsDialog).Assembly.GetManifestResourceStream("settings.ico"))
            {
                if (stream == null) return;
                embeddedIcon = new Icon(stream, SystemInformation.SmallIconSize);
                Icon = embeddedIcon;
            }
        }
        catch { }
    }

    private void ApplyTheme(bool light)
    {
        lightTheme = light;
        palette = light ? ThemePalette.Light : ThemePalette.Dark;
        ApplyPalette();
    }

    // Recolors the existing controls in place: form first, then children, then
    // invalidate (V4). Never touches the preview content or the swatch colors.
    private void ApplyPalette()
    {
        BackColor = palette.Bg;
        ForeColor = palette.Text;
        colorLabel.BackColor = palette.Bg;
        colorLabel.ForeColor = palette.Text;
        textLabel.BackColor = palette.Bg;
        textLabel.ForeColor = palette.Text;
        hintLabel.BackColor = palette.Bg;
        hintLabel.ForeColor = palette.TextSecondary;
        preview.SetFrameColor(palette.Border);
        ThemeButton(customColorButton);
        ThemeButton(resetButton);
        ThemeButton(cancelButton);
        ThemeButton(saveButton);
        ThemeButton(tryButton);
        for (int i = 0; i < swatches.Length; i++)
        {
            Color swatch = (Color)swatches[i].Tag;
            swatches[i].FlatStyle = FlatStyle.Flat;
            swatches[i].UseVisualStyleBackColor = false;
            // Hover/press keep the swatch color so the candidate never shifts.
            swatches[i].FlatAppearance.MouseOverBackColor = swatch;
            swatches[i].FlatAppearance.MouseDownBackColor = swatch;
        }
        UpdateSwatchSelection();
        ApplyTextBoxTheme();
        if (IsHandleCreated)
        {
            ApplyFrameTheme();
            Invalidate(true);
        }
    }

    private void ThemeButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.BackColor = palette.Surface;
        button.ForeColor = palette.Text;
        button.FlatAppearance.MouseOverBackColor = palette.ButtonHover;
        button.FlatAppearance.MouseDownBackColor = palette.ButtonPressed;
        button.FlatAppearance.BorderColor = palette.Border;
    }

    private void UpdateSwatchSelection()
    {
        int current = draft.MaskColor.ToArgb();
        for (int i = 0; i < swatches.Length; i++)
        {
            bool selected = ((Color)swatches[i].Tag).ToArgb() == current;
            swatches[i].FlatAppearance.BorderColor = selected ? palette.Accent : palette.Border;
        }
    }

    private void ApplyTextBoxTheme()
    {
        if (IsDisposed || !textBox.IsHandleCreated) return;
        if (lightTheme)
        {
            // null,null restores the default theme association (T7 #8).
            SetWindowTheme(textBox.Handle, null, null);
            textBox.BackColor = SystemColors.Window;
            textBox.ForeColor = SystemColors.WindowText;
        }
        else
        {
            SetWindowTheme(textBox.Handle, "DarkMode_Explorer", null);
            textBox.BackColor = palette.Surface;
            textBox.ForeColor = palette.Text;
        }
    }

    private void ApplyFrameTheme()
    {
        if (!IsHandleCreated) return;
        int dark = lightTheme ? 0 : 1;
        try { DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, 4); }
        catch (EntryPointNotFoundException) { }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyFrameTheme();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_SETTINGCHANGE && !themeTimerDisposed)
        {
            themeTimer.Stop();
            themeTimer.Start();
        }
        base.WndProc(ref m);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            themeTimerDisposed = true;
            themeTimer.Stop();
            themeTimer.Dispose();
            if (embeddedIcon != null)
            {
                embeddedIcon.Dispose();
                embeddedIcon = null;
            }
        }
        base.Dispose(disposing);
    }

    private void ApplyColor(Color color)
    {
        draft = new MaskConfig(color, draft.MaskText);
        preview.UpdateConfig(draft);
        UpdateSwatchSelection();
    }

    private void PickCustomColor()
    {
        using (ColorDialog dialog = new ColorDialog())
        {
            dialog.FullOpen = true;
            dialog.Color = draft.MaskColor;
            if (dialog.ShowDialog(this) == DialogResult.OK) ApplyColor(dialog.Color);
        }
    }

    private void ResetToDefault()
    {
        draft = new MaskConfig();
        textBox.Text = "";
        preview.UpdateConfig(draft);
        UpdateSwatchSelection();
    }

    private void SaveAndClose(bool tryMask)
    {
        string text = MaskConfig.NormalizeNewlines(textBox.Text);
        if (text.Length > MaskConfig.MaxTextLength)
        {
            MessageBox.Show("文字超过 " + MaskConfig.MaxTextLength + " 字（换行按 1 字计），请删减后再保存。",
                "一键黑屏 · 设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        MaskConfig saving = new MaskConfig(draft.MaskColor, text);
        try
        {
            saving.Save();
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败：" + ex.Message + "\n设置未写入，请重试。",
                "一键黑屏 · 设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (tryMask)
        {
            try
            {
                Process.Start(Application.ExecutablePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法启动遮罩预览：" + ex.Message + "\n设置已保存，可双击主程序查看效果。",
                    "一键黑屏 · 设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Close();
            return;
        }
        MessageBox.Show("已保存。设置将在下一次启动遮罩时生效。",
            "一键黑屏 · 设置", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }

    // Renders the working copy through the shared painter at the primary screen
    // aspect ratio, so wrapping and relative sizes match the real mask.
    private sealed class PreviewPanel : Panel
    {
        private MaskConfig preview;
        private Color frameColor;

        public PreviewPanel(MaskConfig initial)
        {
            preview = initial;
            frameColor = ThemePalette.Light.Border;
            BackColor = initial.MaskColor;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void UpdateConfig(MaskConfig value)
        {
            preview = value;
            BackColor = value.MaskColor;
            Invalidate();
        }

        public void SetFrameColor(Color value)
        {
            frameColor = value;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            MaskPainter.DrawText(e.Graphics, ClientRectangle, preview);
            using (Pen pen = new Pen(frameColor))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}

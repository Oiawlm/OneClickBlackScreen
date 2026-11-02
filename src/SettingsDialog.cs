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
// Since v5 the footer strip also hosts the updater (V5B): a silent check runs on
// load and stays quiet unless a newer release exists (U2 - this window is the
// app's only network touchpoint; the mask never goes online).
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
    private readonly Label textHintLabel = new Label();
    private readonly TextBox textBox = new TextBox();
    private readonly Label hintLabel = new Label();
    private readonly Button resetButton = new Button();
    private readonly Button cancelButton = new Button();
    private readonly Button saveButton = new Button();
    private readonly Button tryButton = new Button();
    private readonly Label versionLabel = new Label();
    private readonly Button checkUpdateButton = new Button();
    private readonly Label updateStatusLabel = new Label();
    private readonly Button applyUpdateButton = new Button();
    private readonly Timer themeTimer = new Timer();
    private MaskConfig draft;
    private Updater.UpdateInfo pendingUpdate;
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
        colorLabel.Text = "遮罩颜色";
        colorLabel.AutoSize = false;
        textLabel.Text = "遮罩文字";
        textLabel.AutoSize = false;
        textHintLabel.Text = "最多 " + MaskConfig.MaxTextLength + " 字 · 回车换行 · 留空不显示";
        textHintLabel.AutoSize = false;
        textHintLabel.TextAlign = ContentAlignment.MiddleRight;
        textBox.Multiline = true;
        // The multiline box consumes Enter as a newline, exactly as the label
        // promises; leaving the default false makes the EDIT forward Enter to the
        // AcceptButton instead (B1).
        textBox.AcceptsReturn = true;
        textBox.ScrollBars = ScrollBars.Vertical;
        // The native buffer counts each hard line break as CR+LF (2 TCHAR), so 200
        // logical characters worst-case need 400; the business cap stays enforced
        // at save time by NormalizeNewlines plus the 200-character check (B2).
        textBox.MaxLength = MaskConfig.MaxTextLength * 2;
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
        versionLabel.Text = "版本 " + AppInfo.Version;
        versionLabel.AutoSize = false;
        versionLabel.TextAlign = ContentAlignment.MiddleLeft;
        checkUpdateButton.Text = "检查更新";
        checkUpdateButton.Click += delegate { BeginCheck(true); };
        updateStatusLabel.Text = "";
        updateStatusLabel.AutoSize = false;
        updateStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
        updateStatusLabel.AutoEllipsis = true;
        applyUpdateButton.Text = "立即更新";
        applyUpdateButton.Visible = false;
        applyUpdateButton.Click += delegate { BeginApplyUpdate(); };
        // Explicit tab order for the focusable controls, matching the visual
        // reading order; labels, the preview and the swatches stay out of the
        // chain. The footer strip sits between the text section and the button
        // row, so the update pair reads after the text box.
        customColorButton.TabIndex = 0;
        textBox.TabIndex = 1;
        checkUpdateButton.TabIndex = 2;
        applyUpdateButton.TabIndex = 3;
        resetButton.TabIndex = 4;
        cancelButton.TabIndex = 5;
        tryButton.TabIndex = 6;
        saveButton.TabIndex = 7;

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

        Controls.Add(colorLabel);
        Controls.Add(preview);
        for (int i = 0; i < swatches.Length; i++) Controls.Add(swatches[i]);
        Controls.Add(customColorButton);
        Controls.Add(textLabel);
        Controls.Add(textHintLabel);
        Controls.Add(textBox);
        Controls.Add(hintLabel);
        Controls.Add(versionLabel);
        Controls.Add(checkUpdateButton);
        Controls.Add(updateStatusLabel);
        Controls.Add(applyUpdateButton);
        Controls.Add(resetButton);
        Controls.Add(cancelButton);
        Controls.Add(tryButton);
        Controls.Add(saveButton);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        Load += delegate
        {
            LayOutControls();
            BeginCheck(false);
        };
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
        // path, no double scaling (V7 erratum). Font ladder from the metrics source
        // (S1): body 10.5pt everywhere except the two caption hints; the section
        // labels add Bold (Body Strong). Children without an explicit font follow
        // the form font, so one shared instance per tier is enough.
        Font bodyFont = new Font(MaskPainter.SharedFamily, SettingsDialogMetrics.BodyPointSize,
            FontStyle.Regular, GraphicsUnit.Point);
        Font labelFont = new Font(MaskPainter.SharedFamily, SettingsDialogMetrics.BodyPointSize,
            FontStyle.Bold, GraphicsUnit.Point);
        Font hintFont = new Font(MaskPainter.SharedFamily, SettingsDialogMetrics.CaptionPointSize,
            FontStyle.Regular, GraphicsUnit.Point);
        Font = bodyFont;
        colorLabel.Font = labelFont;
        textLabel.Font = labelFont;
        hintLabel.Font = hintFont;
        textHintLabel.Font = hintFont;
        // The update footer runs on the caption tier end to end; its two buttons
        // take the hint font so they stay compact beside the status line.
        versionLabel.Font = hintFont;
        updateStatusLabel.Font = hintFont;
        checkUpdateButton.Font = hintFont;
        applyUpdateButton.Font = hintFont;
        // Every layout number comes from SettingsDialogMetrics (D1): spacing
        // rungs, font ladder and structural sizes are declared there once; S()
        // per value is the only runtime derivation path.
        int edge = S(SettingsDialogMetrics.EdgePx);
        int labelGap = S(SettingsDialogMetrics.LabelGapPx);
        int rowGap = S(SettingsDialogMetrics.RowGapPx);
        int groupGap = S(SettingsDialogMetrics.GroupGapPx);
        int labelHeight = S(SettingsDialogMetrics.LabelRowHeightPx);
        int swatchRow = S(SettingsDialogMetrics.SwatchRowHeightPx);
        int buttonRow = S(SettingsDialogMetrics.ButtonRowHeightPx);
        int textHeight = S(SettingsDialogMetrics.TextBoxHeightPx);
        int hintHeight = S(SettingsDialogMetrics.HintHeightPx);
        int footerRow = S(SettingsDialogMetrics.FooterRowHeightPx);
        int hintZone = S(SettingsDialogMetrics.HintZoneWidthPx);
        int width = S(SettingsDialogMetrics.ContentWidthPx);
        // One screen source feeds both the preview cap and the re-center at the
        // end, so the cap is judged against the screen the dialog actually stays on.
        Screen current = Screen.FromControl(this);
        Rectangle primary = Screen.PrimaryScreen.Bounds;
        int previewHeight = (int)Math.Round(width * (double)primary.Height / primary.Width);
        // Cap the preview so the text section and the button row stay inside the
        // working area; the floor only gives way on working areas this app does
        // not support (S2: compress to the floor and let containment fail honestly).
        int fixedChrome = edge + labelHeight + labelGap
            + rowGap + swatchRow + groupGap
            + labelHeight + labelGap + textHeight + rowGap
            + hintHeight + groupGap + footerRow + groupGap + buttonRow + edge;
        int previewCap = current.WorkingArea.Height - (Height - ClientSize.Height) - fixedChrome;
        if (previewCap < S(SettingsDialogMetrics.PreviewFloorPx))
            previewCap = S(SettingsDialogMetrics.PreviewFloorPx);
        if (previewHeight > previewCap) previewHeight = previewCap;
        int x = edge;
        int y = edge;
        colorLabel.SetBounds(x, y, width, labelHeight);
        y += labelHeight + labelGap;
        preview.SetBounds(x, y, width, previewHeight);
        y += previewHeight + rowGap;
        for (int i = 0; i < swatches.Length; i++)
            swatches[i].SetBounds(x + i * S(SettingsDialogMetrics.SwatchPitchPx), y,
                S(SettingsDialogMetrics.SwatchSizePx), S(SettingsDialogMetrics.SwatchSizePx));
        // The custom button follows the last swatch at one inner-gap distance so
        // the row keeps a uniform rhythm, lifted so both share a center line.
        customColorButton.SetBounds(
            x + (swatches.Length - 1) * S(SettingsDialogMetrics.SwatchPitchPx)
                + S(SettingsDialogMetrics.SwatchSizePx) + S(SettingsDialogMetrics.CustomButtonGapPx),
            y - S(SettingsDialogMetrics.CustomButtonLiftPx),
            S(SettingsDialogMetrics.CustomButtonWidthPx), swatchRow);
        y += swatchRow + groupGap;
        // The section label and the constraint hint share a row; the hint keeps
        // its one-line zone on the right and the label gets the rest, so their
        // rects stay disjoint (no opaque-label overlap).
        textLabel.SetBounds(x, y, width - hintZone - labelGap, labelHeight);
        textHintLabel.SetBounds(x + width - hintZone, y, hintZone, labelHeight);
        y += labelHeight + labelGap;
        textBox.SetBounds(x, y, width, textHeight);
        y += textHeight + rowGap;
        hintLabel.SetBounds(x, y, width, hintHeight);
        y += hintHeight + groupGap;
        // Update footer (B7): version line, check button, one-line status, and a
        // fixed slot for the primary apply button - hidden until a newer release
        // exists, and revealing it moves nothing else (constant geometry).
        int footerGap = S(SettingsDialogMetrics.ButtonGapPx);
        int versionZone = S(SettingsDialogMetrics.FooterVersionZonePx);
        int footerButton = S(SettingsDialogMetrics.FooterButtonWidthPx);
        versionLabel.SetBounds(x, y, versionZone, footerRow);
        checkUpdateButton.SetBounds(x + versionZone + footerGap, y, footerButton, footerRow);
        updateStatusLabel.SetBounds(x + versionZone + footerButton + 2 * footerGap, y,
            width - versionZone - 2 * footerButton - 3 * footerGap, footerRow);
        applyUpdateButton.SetBounds(x + width - footerButton, y, footerButton, footerRow);
        y += footerRow + groupGap;
        // Reset stays apart on the left; the save actions cluster on the right
        // with the primary action outermost.
        int resetWidth = S(SettingsDialogMetrics.ResetButtonWidthPx);
        int cancelWidth = S(SettingsDialogMetrics.CancelButtonWidthPx);
        int tryWidth = S(SettingsDialogMetrics.TryButtonWidthPx);
        int saveWidth = S(SettingsDialogMetrics.SaveButtonWidthPx);
        int gap = S(SettingsDialogMetrics.ButtonGapPx);
        int rightX = x + width - saveWidth;
        resetButton.SetBounds(x, y, resetWidth, buttonRow);
        saveButton.SetBounds(rightX, y, saveWidth, buttonRow);
        tryButton.SetBounds(rightX - gap - tryWidth, y, tryWidth, buttonRow);
        cancelButton.SetBounds(rightX - 2 * gap - tryWidth - cancelWidth, y, cancelWidth, buttonRow);
        ClientSize = new Size(x + width + edge, y + buttonRow + edge);
        // CenterScreen placed the form by its initial default size; re-center on
        // the working area the cap was computed against, so the whole window -
        // capped or not - stays inside it (S2 containment).
        Location = new Point(current.WorkingArea.X + (current.WorkingArea.Width - Width) / 2,
            current.WorkingArea.Y + (current.WorkingArea.Height - Height) / 2);
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
        textHintLabel.BackColor = palette.Bg;
        textHintLabel.ForeColor = palette.TextSecondary;
        hintLabel.BackColor = palette.Bg;
        hintLabel.ForeColor = palette.TextSecondary;
        versionLabel.BackColor = palette.Bg;
        versionLabel.ForeColor = palette.TextSecondary;
        updateStatusLabel.BackColor = palette.Bg;
        updateStatusLabel.ForeColor = palette.TextSecondary;
        preview.SetFrameColor(palette.Border);
        ThemeButton(customColorButton, false);
        ThemeButton(resetButton, false);
        ThemeButton(cancelButton, false);
        ThemeButton(saveButton, true);
        ThemeButton(tryButton, false);
        ThemeButton(checkUpdateButton, false);
        ThemeButton(applyUpdateButton, true);
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

    // Recolors a button in place. The primary action (save) renders as a solid
    // accent block whose label keeps a >= 4.5:1 contrast in every state; all
    // other buttons stay on the surface ramp. Focus cues and the disabled look
    // come from the framework and are never overridden here.
    private void ThemeButton(Button button, bool primary)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        if (primary)
        {
            button.BackColor = palette.Accent;
            button.ForeColor = palette.AccentText;
            button.FlatAppearance.MouseOverBackColor = palette.AccentHover;
            button.FlatAppearance.MouseDownBackColor = palette.AccentPressed;
            button.FlatAppearance.BorderColor = palette.Accent;
        }
        else
        {
            button.BackColor = palette.Surface;
            button.ForeColor = palette.Text;
            button.FlatAppearance.MouseOverBackColor = palette.ButtonHover;
            button.FlatAppearance.MouseDownBackColor = palette.ButtonPressed;
            button.FlatAppearance.BorderColor = palette.Border;
        }
    }

    private void UpdateSwatchSelection()
    {
        int current = draft.MaskColor.ToArgb();
        for (int i = 0; i < swatches.Length; i++)
        {
            bool selected = ((Color)swatches[i].Tag).ToArgb() == current;
            // Thickness + accent color keep the selection visible on any swatch.
            swatches[i].FlatAppearance.BorderSize = selected ? 2 : 1;
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
        // Materialize the reserved update slot's native handle even while the
        // button is hidden: a deferred handle would make the child window set
        // depend on the silent check's network state, while a materialized one
        // keeps the reserved slot a stable, observable fact (V5B).
        if (!applyUpdateButton.IsHandleCreated)
        {
            IntPtr reservedSlot = applyUpdateButton.Handle;
        }
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

    // ---- update footer wiring (B7) -----------------------------------------
    // Every network hop runs on the thread pool and results marshal back with
    // Invoke; the settings window is the only code that ever goes online (U2).
    // The silent check on load stays completely quiet unless a newer release
    // exists - failures surface only through the manual button.

    private void SafeInvoke(MethodInvoker action)
    {
        try
        {
            if (IsHandleCreated && !IsDisposed) Invoke(action);
        }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
    }

    private void BeginCheck(bool manual)
    {
        checkUpdateButton.Enabled = false;
        if (manual) updateStatusLabel.Text = "正在检查更新…";
        System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
        {
            bool userAsked = (bool)state;
            Updater.UpdateInfo found = null;
            string failure = null;
            try { found = Updater.Check(AppInfo.UpdateApiBase); }
            catch (Exception ex) { failure = ex.Message; }
            SafeInvoke(delegate
            {
                if (IsDisposed) return;
                checkUpdateButton.Enabled = true;
                if (found != null)
                {
                    pendingUpdate = found;
                    updateStatusLabel.Text = "发现新版 " + found.Tag;
                    applyUpdateButton.Visible = true;
                }
                else if (userAsked)
                {
                    updateStatusLabel.Text = failure != null
                        ? "检查失败：" + failure
                        : "已是最新版本。";
                }
            });
        }, manual);
    }

    private void BeginApplyUpdate()
    {
        if (pendingUpdate == null) return;
        Updater.UpdateInfo update = pendingUpdate;
        checkUpdateButton.Enabled = false;
        applyUpdateButton.Enabled = false;
        updateStatusLabel.Text = "正在下载…";
        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            string downloaded = null;
            string failure = null;
            try
            {
                downloaded = Updater.DownloadAndVerify(update, delegate(int percent)
                {
                    SafeInvoke(delegate
                    {
                        if (!IsDisposed) updateStatusLabel.Text = "正在下载… " + percent + "%";
                    });
                });
            }
            catch (Exception ex) { failure = ex.Message; }
            if (downloaded == null)
            {
                SafeInvoke(delegate
                {
                    if (IsDisposed) return;
                    updateStatusLabel.Text = "更新失败：" + failure;
                    checkUpdateButton.Enabled = true;
                    applyUpdateButton.Enabled = true;
                });
                return;
            }
            // The swap itself runs on the UI thread: the dialog cannot close
            // mid-replace, so the running exe is never left half-copied (B6).
            SafeInvoke(delegate
            {
                if (IsDisposed) return;
                try
                {
                    Updater.ReplaceSequence(downloaded, Application.ExecutablePath);
                }
                catch (Exception ex)
                {
                    updateStatusLabel.Text = "更新失败：" + ex.Message;
                    checkUpdateButton.Enabled = true;
                    applyUpdateButton.Enabled = true;
                    return;
                }
                finally
                {
                    try { if (File.Exists(downloaded)) File.Delete(downloaded); } catch { }
                }
                MessageBox.Show("已更新到 " + update.Tag + "，关闭本窗口后双击主程序即运行新版。",
                    "一键黑屏 · 设置", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            });
        });
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

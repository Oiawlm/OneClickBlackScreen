using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // Leftover backup from a previous self-update (B6): remove it best-effort
        // in every mode; a locked file simply waits for the next start.
        try { File.Delete(Application.ExecutablePath + ".old"); } catch { }
        if (args.Length == 0) return RunOverlay();
        if (args.Length == 1 && args[0] == "--settings") return RunSettings();
        if (args.Length == 1 && args[0] == "--idle") return RunIdleCheck();
        MessageBox.Show("用法：双击或无参数启动遮罩；附加参数 --settings 打开设置窗口。",
            "一键黑屏", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }

    private static int RunOverlay()
    {
        using (Mutex mutex = new Mutex(false, @"Local\BlackScreen.Mask"))
        {
            bool owned = false;
            try
            {
                try { owned = mutex.WaitOne(0); }
                catch (AbandonedMutexException) { owned = true; }
                if (!owned) return 0;
                MaskConfig config = MaskConfig.Load();
                try
                {
                    using (OverlayGroup group = new OverlayGroup(config))
                        Application.Run(group);
                }
                catch
                {
                    return 1;
                }
                return 0;
            }
            finally
            {
                if (owned) mutex.ReleaseMutex();
            }
        }
    }

    // Scheduled per-minute self check (V5C/C2): exit silently while the feature
    // is off or the session is still active, hand off to the shared overlay path
    // once the idle threshold is reached (the mutex swallows overlapping fires).
    private static int RunIdleCheck()
    {
        MaskConfig config = MaskConfig.Load();
        if (config.IdleMinutes <= 0) return 0;
        if (!IdleWake.ShouldShow(IdleWake.IdleMilliseconds(), config.IdleMinutes * 60000)) return 0;
        return RunOverlay();
    }

    private static int RunSettings()
    {
        using (SettingsDialog dialog = new SettingsDialog())
        {
            Application.Run(dialog);
        }
        return 0;
    }
}

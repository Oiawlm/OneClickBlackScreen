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
                // Update awareness (V10 D7/D8), two strictly separate layers:
                // the display decision reads only the local UpdateAvailable
                // mark (always succeeds, offline-safe, fixed for this mask's
                // lifetime), while the throttled network check runs on a
                // background thread and never blocks the mask from showing.
                string updateHint = ReadUpdateHintTag();
                BeginThrottledUpdateCheck();
                try
                {
                    using (OverlayGroup group = new OverlayGroup(config, updateHint))
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

    // The hint shows the registered tag only while it reads as newer than the
    // running build - anything stale, malformed or absent stays invisible
    // (fail-closed like every other tag decision, B2).
    private static string ReadUpdateHintTag()
    {
        string tag = AppPaths.UpdateAvailableTag();
        if (tag != null && AppInfo.IsNewer(tag, AppInfo.CurrentVersion()))
            return "检测到新版本 " + tag;
        return null;
    }

    // One silent check per day at most (D7): due only when a previous
    // completed check stamped the throttle clock at least 24h ago. The whole
    // body is failure-silent on purpose - a dead network, a 403 or a bad
    // payload leaves both marks untouched and the mask unaffected (U2
    // relaxation: this is the mask process's single throttled GET).
    private static void BeginThrottledUpdateCheck()
    {
        if (!AppPaths.IsUpdateCheckDue(DateTime.UtcNow)) return;
        ThreadPool.QueueUserWorkItem(delegate
        {
            try
            {
                Updater.UpdateInfo found = Updater.Check(AppInfo.UpdateApiBase, OverlayCheckTimeoutMs);
                AppPaths.CompleteUpdateCheck(found == null ? null : found.Tag);
            }
            catch
            {
            }
        });
    }

    // Short patience for the overlay's background check (D7): about three
    // seconds keeps a wedged network from keeping the thread pool busy.
    private const int OverlayCheckTimeoutMs = 3000;

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

    // Entry shared with SettingsProgram (V11 D2): the settings exe's Main is
    // just the WinForms bootstrap plus this call, so both exes run one and
    // the same settings pipeline.
    internal static int RunSettings()
    {
        using (SettingsDialog dialog = new SettingsDialog())
        {
            Application.Run(dialog);
        }
        return 0;
    }
}

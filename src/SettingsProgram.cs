using System;
using System.Windows.Forms;

// Parameter-free settings entry (V11 D2): 一键黑屏-设置.exe opens the settings
// window directly - double-click to settings, zero arguments, and any command
// line this process receives is ignored on purpose. Everything else lives in
// the shared sources; the only difference from the mask exe is this entry
// point, while Program.cs keeps the --settings/--idle dispatch so legacy
// shortcuts and the scheduled task keep working against 一键黑屏.exe.
internal static class SettingsProgram
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        return Program.RunSettings();
    }
}

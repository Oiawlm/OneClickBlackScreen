using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

// Idle auto-wake engine (V5C): the session-wide last input tick via
// GetLastInputInfo plus the per-minute scheduled task that launches
// "<exe> --idle". Pure input-driven (R2/C2 - the scheduler's own IdleTrigger
// also requires a quiet CPU and can starve): no resident process, no
// networking, no admin rights - the task runs as the interactive user only.
internal static class IdleWake
{
    // Single registration point (RESEARCH-V5 §4 cross-track table); the D-track
    // cleanup script deletes this same ASCII name. The BLACKSCREEN_IDLE_TASK_NAME
    // environment variable overrides the name (V7V seam, configuration over
    // code): verification registers under a scratch name so the user's real
    // task is never touched; normal runs see no override and keep this default.
    public const string DefaultTaskName = "OneClickBlackScreen-Idle";

    // The scheduled-task identity; one definition point, env override first,
    // the shipped default otherwise.
    public static string TaskName
    {
        get
        {
            string seam = Environment.GetEnvironmentVariable("BLACKSCREEN_IDLE_TASK_NAME");
            return string.IsNullOrEmpty(seam) ? DefaultTaskName : seam;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }
    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO info);

    // Milliseconds since the last keyboard/mouse input in this session. Both
    // ticks are 32-bit; the signed wrap-around subtraction stays correct across
    // the ~49.7-day rollover (C4/R2).
    public static int IdleMilliseconds()
    {
        LASTINPUTINFO info = new LASTINPUTINFO();
        info.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
        int last = Environment.TickCount;
        if (GetLastInputInfo(ref info)) last = unchecked((int)info.dwTime);
        return unchecked(Environment.TickCount - last);
    }

    // Pure decision function (C4), separated so VerifyIdle can table-test it:
    // a non-positive threshold never shows (the caller gates on IdleMinutes).
    public static bool ShouldShow(int idleMs, int thresholdMs)
    {
        return thresholdMs > 0 && idleMs >= thresholdMs;
    }

    // Task definition XML (C3): a TimeTrigger repeating every minute with no
    // Duration (runs forever), the interactive least-privilege user, battery
    // starts allowed, no execution time limit (PT0S - the default PT72H would
    // kill a long-lived mask via the job object), IgnoreNew so a live mask
    // swallows repeated fires, and StartWhenAvailable after a missed tick.
    public static string BuildTaskXml(string exePath)
    {
        StringBuilder xml = new StringBuilder();
        xml.Append("<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n");
        xml.Append("<Task version=\"1.3\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">\r\n");
        xml.Append("  <Triggers>\r\n");
        xml.Append("    <TimeTrigger>\r\n");
        xml.Append("      <StartBoundary>").Append(XmlEscape(DateTime.Now.ToString(
            "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture))).Append("</StartBoundary>\r\n");
        xml.Append("      <Enabled>true</Enabled>\r\n");
        xml.Append("      <Repetition>\r\n");
        xml.Append("        <Interval>PT1M</Interval>\r\n");
        xml.Append("      </Repetition>\r\n");
        xml.Append("    </TimeTrigger>\r\n");
        xml.Append("  </Triggers>\r\n");
        xml.Append("  <Principals>\r\n");
        xml.Append("    <Principal id=\"Author\">\r\n");
        xml.Append("      <LogonType>InteractiveToken</LogonType>\r\n");
        xml.Append("      <RunLevel>LeastPrivilege</RunLevel>\r\n");
        xml.Append("    </Principal>\r\n");
        xml.Append("  </Principals>\r\n");
        xml.Append("  <Settings>\r\n");
        xml.Append("    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>\r\n");
        xml.Append("    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>\r\n");
        xml.Append("    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>\r\n");
        xml.Append("    <StartWhenAvailable>true</StartWhenAvailable>\r\n");
        xml.Append("    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>\r\n");
        xml.Append("  </Settings>\r\n");
        xml.Append("  <Actions Context=\"Author\">\r\n");
        xml.Append("    <Exec>\r\n");
        // Quoted so a path with spaces stays one argument (C7).
        xml.Append("      <Command>\"").Append(XmlEscape(exePath)).Append("\"</Command>\r\n");
        xml.Append("      <Arguments>--idle</Arguments>\r\n");
        xml.Append("    </Exec>\r\n");
        xml.Append("  </Actions>\r\n");
        xml.Append("</Task>\r\n");
        return xml.ToString();
    }

    // Writes the definition in the encoding schtasks import requires on this
    // machine (UTF-16 with BOM, R2); internal so VerifyIdle can pin the bytes.
    internal static void WriteTaskXml(string path, string xml)
    {
        File.WriteAllText(path, xml, Encoding.Unicode);
    }

    // Idempotent registration: /create with /f overwrites any previous task.
    public static void Ensure(string exePath)
    {
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xml");
        try
        {
            WriteTaskXml(temp, BuildTaskXml(exePath));
            RunSchtasks("/create /tn \"" + TaskName + "\" /xml \"" + temp + "\" /f", true);
        }
        finally
        {
            try { if (File.Exists(temp)) File.Delete(temp); }
            catch { }
        }
    }

    // An absent task counts as removed (C6 silent success on the off switch).
    public static void Remove()
    {
        if (Exists()) RunSchtasks("/delete /tn \"" + TaskName + "\" /f", true);
    }

    // /query exit code 0/1 = registered/not registered (R2).
    public static bool Exists()
    {
        return RunSchtasks("/query /tn \"" + TaskName + "\" /xml", false) == 0;
    }

    // All schtasks children run hidden (no console flash); stderr feeds the
    // exception summary. Streams are drained before WaitForExit - both outputs
    // stay far below the pipe buffer, so the synchronous reads cannot deadlock.
    private static int RunSchtasks(string arguments, bool throwOnFailure)
    {
        ProcessStartInfo start = new ProcessStartInfo("schtasks.exe");
        start.Arguments = arguments;
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        using (Process process = Process.Start(start))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (throwOnFailure && process.ExitCode != 0)
                throw new Exception("schtasks " + arguments + " 退出码 " + process.ExitCode
                    + "：" + OneLine(error.Length > 0 ? error : output));
            return process.ExitCode;
        }
    }

    private static string OneLine(string text)
    {
        StringBuilder builder = new StringBuilder();
        foreach (char c in text ?? "")
        {
            if (c == '\r' || c == '\n')
            {
                if (builder.Length == 0 || builder[builder.Length - 1] != ' ') builder.Append(' ');
            }
            else builder.Append(c);
        }
        string one = builder.ToString().Trim();
        return one.Length > 200 ? one.Substring(0, 200) + "…" : one;
    }

    private static string XmlEscape(string text)
    {
        return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}

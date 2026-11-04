using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// Idle auto-wake verifier (V5C/C9): locks the task XML field set (C3), the
// real register/query/delete round trip on the live scheduler, the silent
// active-session exit of "<exe> --idle" against a temp config, the ShouldShow
// decision table and the live idle reading. Links src\IdleWake.cs directly;
// every registration goes through the BLACKSCREEN_IDLE_TASK_NAME seam under a
// scratch name and is removed again in every outcome - the user's real
// OneClickBlackScreen-Idle task is never touched (V7V).
internal static class VerifyIdle
{
    private static string exePath;

    private delegate bool EnumProc(IntPtr hwnd, IntPtr data);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, int dx, int dy, uint data, UIntPtr extra);

    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    // 1. The definition XML carries the full C3 field set, quotes the exe path,
    //    and is written in the UTF-16 LE + BOM encoding schtasks import needs.
    private static void CheckTaskXml()
    {
        string xml = IdleWake.BuildTaskXml(exePath);
        string[] fields = new string[]
        {
            "<?xml version=\"1.0\" encoding=\"UTF-16\"?>",
            "<TimeTrigger>",
            "<Enabled>true</Enabled>",
            "<Interval>PT1M</Interval>",
            "<LogonType>InteractiveToken</LogonType>",
            "<RunLevel>LeastPrivilege</RunLevel>",
            "<MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>",
            "<DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>",
            "<StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>",
            "<StartWhenAvailable>true</StartWhenAvailable>",
            "<ExecutionTimeLimit>PT0S</ExecutionTimeLimit>",
            "<Command>\"" + exePath + "\"</Command>",
            "<Arguments>--idle</Arguments>"
        };
        foreach (string field in fields)
            Check(xml.IndexOf(field) >= 0, "task xml missing field: " + field);
        Check(xml.IndexOf("<Duration>") < 0, "repetition must not carry a Duration (runs indefinitely)");
        int open = xml.IndexOf("<StartBoundary>");
        int close = xml.IndexOf("</StartBoundary>");
        Check(open >= 0 && close > open, "task xml missing StartBoundary");
        string boundary = xml.Substring(open + 15, close - open - 15);
        DateTime moment;
        Check(DateTime.TryParseExact(boundary, "yyyy-MM-dd'T'HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out moment),
            "StartBoundary is not local ISO format: " + boundary);
        Check(Math.Abs((DateTime.Now - moment).TotalMinutes) <= 1,
            "StartBoundary is not the registration moment: " + boundary);
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xml");
        try
        {
            IdleWake.WriteTaskXml(temp, xml);
            byte[] raw = File.ReadAllBytes(temp);
            Check(raw.Length >= 2 && raw[0] == 0xFF && raw[1] == 0xFE,
                "task file is not UTF-16 LE with BOM");
            Check(Encoding.Unicode.GetString(raw, 2, raw.Length - 2) == xml,
                "UTF-16 bytes do not round-trip the definition");
        }
        finally
        {
            try { if (File.Exists(temp)) File.Delete(temp); }
            catch { }
        }
        Console.WriteLine("PASS 1: task xml carries the C3 field set, quotes the exe, writes UTF-16 BOM");
    }

    // 2. Real scheduler loop: register -> query exits 0 -> delete -> query exits 1.
    private static void CheckRegistrationLoop()
    {
        IdleWake.Ensure(exePath);
        try
        {
            Check(IdleWake.Exists(), "scheduled task not found after Ensure");
        }
        finally
        {
            IdleWake.Remove();
        }
        Check(!IdleWake.Exists(), "scheduled task still present after Remove");
        Console.WriteLine("PASS 2: Ensure -> query exit 0 -> Remove -> query exit 1 (task cleaned up)");
    }

    // 3. Negative path: with idle on but the session active, "--idle" must exit
    //    within the 2s budget without ever showing a window. The child resolves
    //    its config through the USERPROFILE seam (V7V: GetFolderPath expands the
    //    "%USERPROFILE%\AppData\Local" registry template against the process
    //    environment and existence-validates it - the LOCALAPPDATA variable is
    //    ignored by that API; probe evidence in docs/VERIFY-V7V.md), so the
    //    user's real config is never even resolved.
    private static void CheckIdleExitsSilently()
    {
        string root = Path.Combine(Path.GetTempPath(), "VerifyIdle_" + DateTime.Now.Ticks.ToString("x"));
        try
        {
            string profile = Path.Combine(root, "profile");
            string configDir = Path.Combine(profile, "AppData", "Local", "BlackScreen");
            Directory.CreateDirectory(configDir);
            Directory.CreateDirectory(Path.Combine(profile, "AppData", "Roaming"));
            File.WriteAllBytes(Path.Combine(configDir, "settings.ini"),
                Encoding.UTF8.GetBytes("color=#010203\r\ntext=\r\nidle=5\r\n"));
            NudgeMouse();
            ProcessStartInfo start = new ProcessStartInfo(exePath);
            start.Arguments = "--idle";
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.EnvironmentVariables["USERPROFILE"] = profile;
            using (Process process = Process.Start(start))
            {
                Stopwatch watch = Stopwatch.StartNew();
                while (!process.WaitForExit(50))
                {
                    Check(watch.ElapsedMilliseconds <= 2000,
                        "active-session --idle did not exit within 2 s (elapsed "
                            + watch.ElapsedMilliseconds + " ms)");
                    Check(VisibleWindows(process.Id).Count == 0,
                        "active-session --idle showed a visible window");
                }
                Check(process.ExitCode == 0, "active-session --idle exited with " + process.ExitCode);
            }
        }
        finally
        {
            try { if (Directory.Exists(root)) Directory.Delete(root, true); }
            catch { }
        }
        Console.WriteLine("PASS 3: active-session --idle exited within 2 s, exit code 0, zero windows");
    }

    // 4. Decision table for the pure function (C4).
    private static void CheckShouldShowTable()
    {
        int[,] cases = new int[,]
        {
            { 0, 0 }, { 0, 300000 }, { 299999, 300000 }, { 300000, 300000 },
            { 300001, 300000 }, { 3000000, 300000 }, { 12345, 0 }, { 12345, -5 }
        };
        bool[] want = new bool[] { false, false, false, true, true, true, false, false };
        for (int i = 0; i < want.Length; i++)
        {
            bool got = IdleWake.ShouldShow(cases[i, 0], cases[i, 1]);
            Check(got == want[i], "ShouldShow(" + cases[i, 0] + ", " + cases[i, 1]
                + ") returned " + got + ", expected " + want[i]);
        }
        Console.WriteLine("PASS 4: ShouldShow decision table (" + want.Length + " cases, threshold edges included)");
    }

    // 5. Live reading sanity on an active session (synthesized input first so
    //    the check cannot inherit an old idle accumulator).
    private static void CheckIdleReading()
    {
        NudgeMouse();
        Thread.Sleep(50);
        int idle = IdleWake.IdleMilliseconds();
        Check(idle >= 0 && idle < 60000, "live idle reading outside the active-session range: " + idle);
        Console.WriteLine("PASS 5: live IdleMilliseconds() = " + idle + " ms (0 <= reading < 60000)");
    }

    private static void NudgeMouse()
    {
        mouse_event(1 /* MOUSEEVENTF_MOVE */, 1, 0, 0, UIntPtr.Zero);
        mouse_event(1 /* MOUSEEVENTF_MOVE */, -1, 0, 0, UIntPtr.Zero);
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

    [STAThread]
    private static int Main(string[] args)
    {
        VerifyGate.Enter();
        try
        {
            Check(args.Length == 1, "Usage: VerifyIdle.exe <exe>");
            exePath = args[0];
            // Task-name seam (V7V): IdleWake.TaskName reads this override, so
            // Ensure/Exists/Remove below - including the failure-path cleanup in
            // the catch - all target the scratch task, never the user's real one.
            Environment.SetEnvironmentVariable("BLACKSCREEN_IDLE_TASK_NAME", "OneClickBlackScreen-Idle-Verify");
            CheckTaskXml();
            CheckRegistrationLoop();
            CheckIdleExitsSilently();
            CheckShouldShowTable();
            CheckIdleReading();
            Console.WriteLine("PASS TOTAL: 5 idle checks (xml fields / scheduler loop / silent exit / decision table / live reading)");
            return 0;
        }
        catch (Exception ex)
        {
            try { IdleWake.Remove(); }
            catch { }
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }
}

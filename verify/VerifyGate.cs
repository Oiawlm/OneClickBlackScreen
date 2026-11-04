using System;
using System.Threading;

// Session gate shared by all seven verifiers (V7V): the GUI verifiers inject
// global keyboard/mouse input and seize the foreground, and every verifier
// writes fixed paths (verify\bin, _probe) - all session-exclusive resources
// (AGENTS.md §2), so two concurrent verifier runs in one login session
// silently corrupt each other. Every verifier calls Enter() as the first
// statement of Main: the first process owns Local\BlackScreen.VerifyGate
// until it exits (the handle closes with the process, so a crashed holder
// merely abandons the mutex and the next waiter acquires it), while any
// concurrent second run reports BUSY on stderr and exits 2 without touching
// anything. verify.cmd maps exit 2 to [FAIL] VERIFY BUSY / exit 2.
internal static class VerifyGate
{
    public const int BusyExit = 2;

    // Rooted for the whole process lifetime on purpose; the mutex is never
    // released - the kernel drops ownership when the process dies.
    private static Mutex held;

    public static void Enter()
    {
        Mutex gate = new Mutex(false, @"Local\BlackScreen.VerifyGate");
        bool owned = false;
        try { owned = gate.WaitOne(0); }
        catch (AbandonedMutexException) { owned = true; }
        if (!owned)
        {
            Console.Error.WriteLine("VERIFY BUSY: another verifier run holds the session gate (Local\\BlackScreen.VerifyGate)");
            Environment.Exit(BusyExit);
        }
        held = gate;
    }
}

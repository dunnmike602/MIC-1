namespace MLDComputing.Emulators.MIC1.Interop;

using System.Runtime.InteropServices;

internal class MachineHelper
{
#if WINDOWS
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentProcessorNumber();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadAffinityMask(IntPtr hThread, uint dwThreadAffinityMask);

#elif LINUX
    [DllImport("libc")]
    private static extern int sched_getcpu();
#endif
#if WINDOWS
    public static uint GetCurrentCore()
    {
        var core = GetCurrentProcessorNumber();

        return core;
    }

#elif LINUX
    public static uint GetCurrentCore()
    {
        int core = sched_getcpu();
        if (core == -1)
        {
            throw new InvalidOperationException("Failed to get CPU core number.");
        }
        return (uint)core;
    }
#endif
}
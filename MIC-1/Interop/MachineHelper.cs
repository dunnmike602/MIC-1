namespace MLDComputing.Emulators.MIC1.Interop;

using System.Diagnostics;
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

    public static int GetBestCore()
    {
        do
        {
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                var counter = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
                counter.NextValue(); // Warm-up
                Thread.Sleep(100); // Allow sampling
                var usage = counter.NextValue();
                Console.WriteLine($"Core {i}: {usage}% used");
            }

            Console.WriteLine();
            Console.WriteLine("Pick the core you want to use. Press ENTER to scan again.");
            var input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return Convert.ToInt32(input);
            }

        } while (true);

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
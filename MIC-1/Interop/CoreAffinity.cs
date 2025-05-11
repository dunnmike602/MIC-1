namespace MLDComputing.Emulators.MIC1.Interop;

using System.Runtime.InteropServices;

internal class CoreAffinity
{
#if WINDOWS
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll")]
    private static extern UIntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);

    public static void PinToCore(int core)
    {
        if (core < 0 || core >= Environment.ProcessorCount)
            throw new ArgumentOutOfRangeException(nameof(core));

        IntPtr thread = GetCurrentThread();
        UIntPtr mask = (UIntPtr)(1 << core);
        SetThreadAffinityMask(thread, mask);
    }
#endif
}
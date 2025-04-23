namespace MLDComputing.Emulators.MIC1.Core.Constants;

public static class SimulatorConstants
{
    // 64KB = 0x0000 to 0xFFFF
    public const int DefaultMemorySize = 0x10000;

    // 1 MHz processor
    public const int DefaultTargetFrequencyHz = 1_000_000;

    // Match typical monitor refresh
    public const int DefaultRefreshRateHz = 60; 

    public const int MaxSkippedFrames = 5;

    public const float MillisecondsPerSecond = 1000.0f;

    public static class MicroCode
    {
        public const int StoreSize = 512;
    }
}
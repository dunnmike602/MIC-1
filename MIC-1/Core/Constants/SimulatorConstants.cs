namespace MLDComputing.Emulators.MIC1.Core.Constants;

public static class SimulatorConstants
{
    public static class Machine
    {
        // 64KB = 0x0000 to 0xFFFF
        public const int DefaultMemorySize = 0x10000;

        // 1 MHz processor
        public const int DefaultTargetFrequencyHz = 1_000_000;

        // Match typical monitor refresh
        public const int DefaultRefreshRateHz = 30;
    }

    public static class MicroCode
    {
        public const int StoreSize = 512;
    }
}
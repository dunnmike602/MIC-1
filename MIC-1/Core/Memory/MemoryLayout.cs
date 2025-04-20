namespace MLDComputing.Emulators.MIC1.Core.Memory;

public static class MemoryLayout
{
    public static MemoryRegion GetRegion(int address)
    {
        var region = address switch
        {
            >= CodeSegment.Start and <= CodeSegment.End => MemoryRegion.Program,
            >= DataSegment.Start and <= DataSegment.End => MemoryRegion.Data,
            >= StackSegment.Start and <= StackSegment.End => MemoryRegion.Stack,
            _ => throw new ArgumentOutOfRangeException(nameof(address), $"Invalid memory address: 0x{address:X4}")
        };

        return region;
    }
            
    public static bool IsInCode(int address) =>
        address is >= CodeSegment.Start and <= CodeSegment.End;

    public static bool IsInHeap(int address) =>
        address is >= DataSegment.Start and <= DataSegment.End;

    public static bool IsInStack(int address) =>
        address is >= StackSegment.Start and <= StackSegment.End;

    public static class CodeSegment
    {
        public const int End = 0x3FFF;
        public const int Start = 0x0000;
    }

    public static class DataSegment
    {
        public const int End = 0xBFFF;
        public const int Start = 0x4000;
    }

    public static class StackSegment
    {
        public const int End = 0xFFFF;
        public const int Start = 0xC000;
    }
}
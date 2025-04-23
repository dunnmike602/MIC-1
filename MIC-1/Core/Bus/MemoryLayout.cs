namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Runtime.CompilerServices;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInCode(int address)
    {
        return address is >= CodeSegment.Start and <= CodeSegment.End;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInHeap(int address)
    {
        return address is >= DataSegment.Start and <= DataSegment.End;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInStack(int address)
    {
        return address is >= StackSegment.Start and <= StackSegment.End;
    }

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
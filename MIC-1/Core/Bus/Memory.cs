namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exceptions;
using MicroCode;
using static MemoryLayout;

public static unsafe class Memory
{
    private static bool _memoryChecking;

    internal static byte[]? MemoryBuffer;

    private static GCHandle _handle;

    private static byte* _ptr;

    public static void Init(int size, bool memoryChecking)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Memory size must be positive.", nameof(size));
        }

        _memoryChecking = memoryChecking;
        MemoryBuffer = new byte[size];
        _handle = GCHandle.Alloc(MemoryBuffer, GCHandleType.Pinned);
        _ptr = (byte*)_handle.AddrOfPinnedObject();
    }

    public static bool AllowExecuteFromData = false;

    public static bool AllowExecuteFromStack = false;

    public static int Size => MemoryBuffer!.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Access(MicroInstruction mi)
    {
        var op = mi.MEM;
        var mar = Registers.MAR;

        switch (op)
        {
            case MemoryOperation.NoOp:
                break;

            case MemoryOperation.ReadWordToMDRHigh:
                CheckBoundsForWord(mar);
            {
                var high = (ushort)((_ptr[mar] << 8) | _ptr[mar + 1]);
                Registers.MDR = (Registers.MDR & 0x0000FFFF) | (high << 16);
            }
                break;

            case MemoryOperation.ReadWordToMDRLow:
                CheckBoundsForWord(mar);
            {
                var low = (ushort)((_ptr[mar] << 8) | _ptr[mar + 1]);
                Registers.MDR = (int)((Registers.MDR & 0xFFFF0000) | low);
            }
                break;

            case MemoryOperation.WriteWordFromMDRHigh:
                CheckBoundsForWord(mar);
                if (_memoryChecking)
                {
                    ValidateSegment(mar);
                }

            {
                var high = (ushort)(Registers.MDR >> 16);
                _ptr[mar] = (byte)(high >> 8);
                _ptr[mar + 1] = (byte)high;
            }
                break;

            case MemoryOperation.WriteWordFromMDRLow:
                CheckBoundsForWord(mar);
                if (_memoryChecking)
                {
                    ValidateSegment(mar);
                }

            {
                var low = (ushort)(Registers.MDR & 0xFFFF);
                _ptr[mar] = (byte)(low >> 8);
                _ptr[mar + 1] = (byte)low;
            }
                break;


            case MemoryOperation.ReadByteToMBR:
                CheckBoundsForByte(mar);

                if (_memoryChecking)
                {
                    ValidateExecutionSegment(mar, mi.Key == MicroInstructionCode.FETCHReadInstruction);
                }

                Registers.MBR = _ptr[mar];
                break;

            case MemoryOperation.WriteByteFromMBR:
                CheckBoundsForByte(mar);
                if (_memoryChecking)
                {
                    ValidateSegment(mar);
                }

                _ptr[mar] = Registers.MBR;
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LoadBootProgram(int startAddress, params byte[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            _ptr[startAddress + i] = values[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckBoundsForByte(int address)
    {
        if (address < 0 || address > Size)
        {
            throw new MemoryFaultException(address, false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckBoundsForWord(int address)
    {
        if (address < 0 || address + 1 > Size)
        {
            throw new MemoryFaultException(address, true);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateExecutionSegment(int address, bool isInstructionFetch)
    {
        if (!isInstructionFetch)
        {
            return;
        }

        var region = GetRegion(address);

        if ((region == MemoryRegion.Data && !AllowExecuteFromData) ||
            (region == MemoryRegion.Stack && !AllowExecuteFromStack))
        {
            throw new ExecutionProtectionException(address, region);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateSegment(int address)
    {
        var region = GetRegion(address);
        if (region == MemoryRegion.Program)
        {
            throw new WriteProtectException(address);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadByte(int address, bool isInstructionFetch = false)
    {
        CheckBoundsForByte(address);

        if (_memoryChecking)
        {
            ValidateExecutionSegment(address, isInstructionFetch);
        }

        return _ptr[address];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadWord(int address)
    {
        CheckBoundsForWord(address);

        return (_ptr[address] << 8) |
               _ptr[address + 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteByte(int address, byte value)
    {
        CheckBoundsForByte(address);

        if (_memoryChecking)
        {
            ValidateSegment(address);
        }

        _ptr[address] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteWord(int address, int value)
    {
        CheckBoundsForWord(address);

        if (_memoryChecking)
        {
            ValidateSegment(address);
        }

        _ptr[address] = (byte)(value >> 8);
        _ptr[address + 1] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Dispose()
    {
        if (_handle.IsAllocated)
        {
            _handle.Free();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MemoryRegion GetRegion(int address)
    {
        var returnValue = address switch
        {
            >= StackSegment.Bottom and <= StackSegment.Top => MemoryRegion.Stack,
            < StackSegment.Bottom => MemoryRegion.Program,
            _ => MemoryRegion.Data
        };

        return returnValue;
    }
}
namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exceptions;
using MicroCode;
using static MemoryLayout;

public sealed unsafe class Memory : IDisposable
{
    private readonly bool _memoryChecking;
    
    private readonly byte[] _buffer;
    
    private GCHandle _handle;
    
    private readonly byte* _ptr;

    public Memory(int size, bool memoryChecking)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Memory size must be positive.", nameof(size));
        }

        _memoryChecking = memoryChecking;
        _buffer = new byte[size];
        _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        _ptr = (byte*)_handle.AddrOfPinnedObject();
    }

    public bool AllowExecuteFromData = false;

    public bool AllowExecuteFromStack = false;
   
    private bool _disposed;

    public int Size => _buffer.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Access(Registers registers, MicroInstruction mi)
    {
        var op = mi.MEM;
        var mar = registers.MAR;

        switch (op)
        {
            case MemoryOperation.NoOp:
                break;

            case MemoryOperation.ReadWordToMDRHigh:
                CheckBoundsForWord(mar);
            {
                ushort high = (ushort)((_ptr[mar] << 8) | _ptr[mar + 1]);
                registers.MDR = (registers.MDR & 0x0000FFFF) | (high << 16);
            }
                break;

            case MemoryOperation.ReadWordToMDRLow:
                CheckBoundsForWord(mar);
            {
                ushort low = (ushort)((_ptr[mar] << 8) | _ptr[mar + 1]);
                registers.MDR = (int)((registers.MDR & 0xFFFF0000) | low);
            }
                break;

            case MemoryOperation.WriteWordFromMDRHigh:
                CheckBoundsForWord(mar);
                if (_memoryChecking)
                {
                    ValidateSegment(mar);
                }
            {
                ushort high = (ushort)(registers.MDR >> 16);
                _ptr[mar] = (byte)(high >> 8);
                _ptr[mar + 1] = (byte)(high);
            }
                break;

            case MemoryOperation.WriteWordFromMDRLow:
                CheckBoundsForWord(mar);
                if (_memoryChecking)
                {
                    ValidateSegment(mar);
                }
            {
                ushort low = (ushort)(registers.MDR & 0xFFFF);
                _ptr[mar] = (byte)(low >> 8);
                _ptr[mar + 1] = (byte)(low);
            }
                break;


            case MemoryOperation.ReadByteToMBR:
                CheckBoundsForByte(mar);
                
                if (_memoryChecking)
                {
                    ValidateExecutionSegment(mar, mi.Key == MicroInstructionCode.FETCHReadInstruction);
                }

                registers.MBR = _ptr[mar];
                break;

           case MemoryOperation.WriteByteFromMBR:
                CheckBoundsForByte(mar);
                if (_memoryChecking)
                {
                    ValidateSegment(mar);
                }

                _ptr[mar] = registers.MBR;
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LoadProgram(int startAddress, params byte[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            _ptr[startAddress + i] = values[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBoundsForByte(int address)
    { 
        if (address < 0 || address >= Size)
        {
            throw new MemoryFaultException(address, false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBoundsForWord(int address)
    {
        if (address < 0 || address + 1 >= Size)
        {
            throw new MemoryFaultException(address, true);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateExecutionSegment(int address, bool isInstructionFetch)
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
    private void ValidateSegment(int address)
    {
        var region = GetRegion(address);
        if (region == MemoryRegion.Program)
        {
            throw new WriteProtectException(address);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte(int address, bool isInstructionFetch = false)
    {
        CheckBoundsForByte(address);
        
        if (_memoryChecking)
        {
            ValidateExecutionSegment(address, isInstructionFetch);
        }

        return _ptr[address];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadWord(int address)
    {
        CheckBoundsForWord(address);

        return (_ptr[address] << 8) |
               (_ptr[address + 1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(int address, byte value)
    {
        CheckBoundsForByte(address);

        if (_memoryChecking)
        {
            ValidateSegment(address);
        }

        _ptr[address] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteWord(int address, int value)
    {
        CheckBoundsForWord(address);

        if (_memoryChecking)
        {
            ValidateSegment(address);
        }

        _ptr[address] = (byte)(value >> 8);
        _ptr[address + 1] = (byte)value;
    }

    public string[] DumpStackMemory(int sp)
    {
        var stackDump = new List<string>();
        for (var address = StackSegment.End; address >= StackSegment.Start; address--)
        {
            var value = _ptr[address];
            var label = address == sp ? "-> SP" : "";
            stackDump.Add($"{address:D5}-{address:X4}: 0x{value:X2} {label}");
        }

        return stackDump.ToArray();
    }
    
    ~Memory()
    {
        Dispose(false); // Finalizer just calls Dispose(false)
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (_handle.IsAllocated)
        {
            _handle.Free();
        }

        _disposed = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MemoryRegion GetRegion(int address)
    {
        var returnValue = address switch
        {
            >= StackSegment.Start and <= StackSegment.End => MemoryRegion.Stack,
            < StackSegment.Start => MemoryRegion.Program,
            _ => MemoryRegion.Data
        };

        return returnValue;
    }


}
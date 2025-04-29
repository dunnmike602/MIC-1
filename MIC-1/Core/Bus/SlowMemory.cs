namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Runtime.CompilerServices;
using MicroCode;
using static MemoryLayout;

public class SlowMemory
{
    private readonly bool _memoryChecking;

    private readonly byte[] _memory;

    public SlowMemory(int size, bool memoryChecking)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Memory size must be positive.", nameof(size));
        }

        _memoryChecking = memoryChecking;

        _memory = new byte[size];
    }

    public bool AllowExecuteFromData { get; set; } = false;

    public bool AllowExecuteFromStack { get; set; } = false;

    public bool AllowWriteToPrograms { get; set; } = false;

    public int Size => _memory.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Access(Registers registers, MicroInstruction mi)
    {
        var op = mi.MEM;
        var mar = registers.MAR;

        switch (op)
        {
            case MemoryOperation.NoOp:
                break;

            case MemoryOperation.ReadWordToMDR:

                if (_memoryChecking)
                {
                    CheckBoundsForWord(mar);
                }

                registers.MDR = (_memory[mar] << 24) |
                                (_memory[mar + 1] << 16) |
                                (_memory[mar + 2] << 8) |
                                _memory[mar + 3];
                break;

            case MemoryOperation.ReadByteToMBR:
                if (_memoryChecking)
                {
                    var isInstructionFetch = mi.Key == MicroInstructionCode.FETCHReadInstruction;

                    ValidateAddress(mar);

                    ValidateExecutionSegment(mar, isInstructionFetch);
                }

                registers.MBR = _memory[mar];
                break;


            case MemoryOperation.WriteWordFromMDR:
                if (_memoryChecking)
                {
                    CheckBoundsForWord(mar);
                    ValidateSegment(mar);
                }

                var span = _memory.AsSpan(mar, 4);
                span[0] = (byte)(registers.MDR >> 24);
                span[1] = (byte)(registers.MDR >> 16);
                span[2] = (byte)(registers.MDR >> 8);
                span[3] = (byte)registers.MDR;

                break;

            case MemoryOperation.WriteByteFromMBR:
                if (_memoryChecking)
                {
                    ValidateAddress(mar);
                    ValidateSegment(mar);
                }

                _memory[mar] = registers.MBR;
                break;
        }
    }

    public void LoadProgram(int startAddress, params byte[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            _memory[startAddress + i] = values[i];
        }
    }

    private void ValidateAddress(int address)
    {
        if (address < 0 || address > _memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(address), $"Address {address} is out of bounds.");
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
            throw new InvalidOperationException($"Execution not allowed from address 0x{address:X4} ({region})");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateSegment(int address)
    {
        var region = GetRegion(address);

        if (region == MemoryRegion.Program && !AllowWriteToPrograms)
        {
            throw new InvalidOperationException($"Write to program region denied at 0x{address:X4}");
        }
    }

    public string[] DumpStackMemory(int sp)
    {
        var stackDump = new List<string>();

        for (var address = StackSegment.End; address >= StackSegment.Start; address--)
        {
            var value = _memory[address];
            var label = address == sp ? "-> SP" : "";
            stackDump.Add($"{address:D5}-{address:X4}: 0x{value:X2} {label}");
        }

        return stackDump.ToArray();
    }

    private void CheckBoundsForWord(int address)
    {
        if (address < 0 || address + 3 >= _memory.Length - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(address), "Address out of memory bounds.");
        }
    }

    public byte ReadByte(int address, bool isInstructionFetch = false)
    {
        ValidateAddress(address);

        ValidateExecutionSegment(address, isInstructionFetch);

        return _memory[address];
    }

    public int ReadWord(int address)
    {
        // Note that the MIC-1 is big endian
        CheckBoundsForWord(address);

        return _memory[address] << 24 |
               _memory[address + 1] << 16 |
               _memory[address + 2] << 8 |
               _memory[address + 3];
    }

    public void WriteByte(int address, byte value)
    {
        ValidateAddress(address);
        ValidateSegment(address);

        _memory[address] = value;
    }

    public void WriteWord(int address, int value)
    {
        // Note that the MIC-1 is big endian
        CheckBoundsForWord(address);

        ValidateSegment(address);

        _memory[address] = (byte)(value >> 24 & 0xFF); // Most significant byte
        _memory[address + 1] = (byte)(value >> 16 & 0xFF);
        _memory[address + 2] = (byte)(value >> 8 & 0xFF);
        _memory[address + 3] = (byte)(value & 0xFF);         // Least significant byte
    }

}

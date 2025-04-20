using MLDComputing.Emulators.MIC1.Core.Bus;
using MLDComputing.Emulators.MIC1.Core.MicroCode;

namespace MLDComputing.Emulators.MIC1.Core.Memory;

public class Memory
{
    private readonly byte[] _memory;

    public Memory(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Memory size must be positive.", nameof(size));
        }

        _memory = new byte[size];
    }

    public bool AllowExecuteFromData { get; set; } = false;
    public bool AllowExecuteFromStack { get; set; } = false;
    public bool AllowWriteToPrograms { get; set; } = false;
    public int Size => _memory.Length;

    public void Access(Registers registers, MicroInstruction mi)
    {
        switch (mi.MEM)
        {
            case MemoryOperation.NoOp:
                break;

            case MemoryOperation.ReadWordToMDR:
                registers.MDR = ReadWord(registers.MAR);
                break;

            case MemoryOperation.ReadByteToMBR:
                var isInstruction = mi.Key == MicroInstructionCode.FetchReadInstruction ? true : false;
                registers.MBR = ReadByte(registers.MAR, isInstruction);
                break;

            case MemoryOperation.WriteWordFromMDR:
                WriteWord(registers.MAR, registers.MDR);
                break;

            case MemoryOperation.WriteByteFromMBR:
                WriteByte(registers.MAR, registers.MBR);
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

    private void CheckBoundsForWord(int address)
    {
        if (address < 0 || address + 3 >= _memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(address), "Address out of memory bounds.");
        }
    }

    private void ValidateAddress(int address)
    {
        if (address < 0 || address > _memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(address), $"Address {address} is out of bounds.");
        }
    }

    private void ValidateExecutionSegment(int address, bool isInstructionFetch)
    {
        if (!isInstructionFetch)
        {
            return;
        }

        var region = MemoryLayout.GetRegion(address);

        if ((region == MemoryRegion.Data && !AllowExecuteFromData) ||
            (region == MemoryRegion.Stack && !AllowExecuteFromStack))
        {
            throw new InvalidOperationException($"Execution not allowed from address 0x{address:X4} ({region})");
        }
    }

    private void ValidateSegment(int address)
    {
        var region = MemoryLayout.GetRegion(address);

        if (region == MemoryRegion.Program && !AllowWriteToPrograms)
        {
            throw new InvalidOperationException($"Write to program region denied at 0x{address:X4}");
        }
    }
}
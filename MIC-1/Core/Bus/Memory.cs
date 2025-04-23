namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Net;
using System.Runtime.CompilerServices;
using MicroCode;
using static MemoryLayout;

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
        var op = mi.MEM;
        var mar = registers.MAR;
        
        switch (op)
        {
            case MemoryOperation.NoOp:
                break;

            case MemoryOperation.ReadWordToMDR:
                registers.MDR = (_memory[mar] << 24) |
                                (_memory[mar + 1] << 16) |
                                (_memory[mar + 2] << 8) |
                                _memory[mar + 3];
                break;

            case MemoryOperation.ReadByteToMBR:
                registers.MBR = _memory[mar];
                break;
            
            case MemoryOperation.ReadOperandToToMBR:
               // var isInstruction = mi.Key == MicroInstructionCode.FETCHReadInstruction;
                registers.MBR = _memory[mar];
                registers.CurrentOpcode = _memory[mar];
                break;

            case MemoryOperation.WriteWordFromMDR:
                WriteWord(mar, registers.MDR);
                break;

            case MemoryOperation.WriteByteFromMBR:
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

    public byte ReadByte(int address, bool isInstructionFetch = false)
    {
        return _memory[address];
    }
    
    public void WriteWord(int address, int value)
    {
        var span = _memory.AsSpan(address, 4);
        span[0] = (byte)(value >> 24);
        span[1] = (byte)(value >> 16);
        span[2] = (byte)(value >> 8);
        span[3] = (byte)value;
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
}
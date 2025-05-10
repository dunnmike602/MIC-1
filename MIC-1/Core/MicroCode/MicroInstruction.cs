namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

using System.Runtime.InteropServices;
using Bus;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct MicroInstruction
{
    private readonly string? _name;
    
    public MicroInstruction()
    {
        Address = (int)MicroInstructionCode.Uninitialised;
        ALU = ALUOperation.Nop;
        B = RegisterSelectSignal.None;
        C = 0;
        JAM = 0;
        MEM = MemoryOperation.NoOp;
        Key = MicroInstructionCode.Uninitialised;
        OpCode = null;
        MemoryAfterRegisterWrite = true;
    }
    
    public byte JAM { get; init; }

    public bool MemoryAfterRegisterWrite { get; init; } = true;

    public int Address { get; init; }

    public ALUOperation ALU { get; init; }

    public RegisterSelectSignal B { get; init; }

    public int C { get; init; }

    public MemoryOperation MEM { get; init; }

    public MicroInstructionCode Key { get; init; }

    public string Name
    {
        get => _name ?? Key.ToString();
        init => _name = value;
    }

    public string? OpCode { get; init; }

    public override string ToString()
    {
        return Name;
    }
}
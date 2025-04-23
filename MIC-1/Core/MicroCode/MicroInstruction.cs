namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

using Bus;
using IJVM;

public class MicroInstruction
{
    public int Address { get; set; }

    public ALUOperation ALU { get; set; }

    public RegisterSelectSignal B { get; set; }

    public int C { get; set; }

    public byte JAM { get; set; }

    public bool MemoryAfterRegisterWrite { get; set; } = true;

    public MemoryOperation MEM { get; set; }

    public MicroInstructionCode Key { get; set; }

    public string? Name => Key.ToString();

    public string? OpCode { get; set; }
}
namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

using Bus;
using IJVM;
using Memory;

public class MicroInstruction
{
    public int Address { get; set; }

    public ALUOperation ALU { get; set; }

    public RegisterSelectSignal B { get; set; }

    public int C { get; set; }

    public int JAM { get; set; }

    public bool MemoryAfterRegisterWrite { get; set; } = true;

    public MemoryOperation MEM { get; set; }

    public MicroInstructionCode Key { get; set; }

    public string? Name { get; set; }

    public OpCode OpCode { get; set; }

    public override string ToString()
    {
        return $"OpCode: {OpCode.ToString()} Name: {Name ?? "N/A"}";
    }
}
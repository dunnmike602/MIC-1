namespace MLDComputing.Emulators.MIC1.Core.Bus;

public enum MemoryOperation
{
    NoOp,                       // nothing on the bus
    ReadWordToMDR,              // MDR ← Memory[MAR]        (32‑bit or word-sized read)
    ReadByteToMBR,              // MBR ← Memory[MAR]        (8‑bit opcode / operand)
    ReadOperandToToMBR,         // MBR ← Memory[MAR]        (8‑bit opcode / operand) This is special used by the FETCH cycle only so we can track which opcode is being executed
    WriteWordFromMDR,           // Memory[MAR] ← MDR       (32‑bit or word-sized read)
    WriteByteFromMBR            // Memory[MAR] ← MBR       (8‑bit or word-sized read)
}
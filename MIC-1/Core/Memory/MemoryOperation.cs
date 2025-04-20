namespace MLDComputing.Emulators.MIC1.Core.Memory;

public enum MemoryOperation
{
    NoOp,                   // nothing on the bus
    ReadWordToMDR,          // MDR ← Memory[MAR]        (32‑bit or word-sized read)
    ReadByteToMBR,          // MBR ← Memory[MAR]        (8‑bit opcode / operand)
    WriteWordFromMDR,           // Memory[MAR] ← MDR       (32‑bit or word-sized read)
    WriteByteFromMBR            // Memory[MAR] ← MBR       (8‑bit or word-sized read)
}
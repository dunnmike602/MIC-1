namespace MLDComputing.Emulators.MIC1.Core.Bus;

/// <summary>
/// One bit of the C‑bus load mask in a MIC‑1 micro‑instruction.
/// If bit n is set, the corresponding register latches the ALU / C‑bus value
/// at the end of the micro‑cycle.
/// </summary>
[Flags]                         // lets you OR several bits if ever needed
public enum RegisterLoadSignal : int
{
    // 0 … 7  : general ALU‑visible registers
    H = 0,   // Holding register (ALU input A)

    MDR = 1,   // Memory Data Register  (32‑bit in many emulators)
    PC = 2,   // Program Counter
    SP = 3,   // Stack Pointer
    LV = 4,   // Local Variables base pointer
    CPP = 5,   // Constant‑Pool Pointer
    TOS = 6,   // Top‑of‑Stack cache
    OPC = 7,   // Old Program Counter (return address)

    // 8 … 11 : interface / sequencing registers
    MAR = 8,   // Memory Address Register  (drives the address bus)

    MPC = 10,  // Micro‑Program Counter    (next micro‑instruction)
    // Bit 11 spare – can be used for “write flags” or custom extensions
}
namespace MLDComputing.Emulators.MIC1.Core.Bus;

[Flags]
public enum JAMControl : byte
{
    None = 0b000,   // No branching
    JAMZ = 0b010,   // OR MBR if zero flag is set (Z == 1)
    JAMN = 0b100,   // OR MBR if negative flag is set (N == 1)
    JAMC = 0b001    // OR in MBR unconditionally (e.g., for opcode dispatch)
}
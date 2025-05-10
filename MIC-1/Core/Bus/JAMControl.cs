namespace MLDComputing.Emulators.MIC1.Core.Bus;

[Flags]
public enum JAMControl : byte
{
    None = 0b00000000,
    JAMZ = 0b00000001, // original MIC-1 style: jump if zero
    JAMN = 0b00000010, // original MIC-1 style: jump if negative
    JAMC = 0b00000100, // original MIC-1: unconditional opcode dispatch
    JAMZ_EQ = 0b00001000, // new: explicit jump if == 0
    JAMZ_NE = 0b00010000  // new: explicit jump if != 0
}
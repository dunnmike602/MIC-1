namespace MLDComputing.Emulators.MIC1.Core.Enums;

public enum TrapCode : byte
{
    None = 0x00, // No trap
    Halt = 0x01, // HALT instruction or requested stop
    StackOverflow = 0x02, // Stack pointer went beyond allocated range
    StackUnderflow = 0x03, // Stack pointer dropped below base
    DivideByZero = 0x04, // Division by zero attempted
    InvalidInstruction = 0x05, // Opcode not recognized or illegal
    SegmentationFault = 0x06, // Access outside valid memory range
    Breakpoint = 0x07, // Debug breakpoint hit
    SysCall = 0x08, // System call invoked (if supported)
    TimerInterrupt = 0x09, // Timer expired (simulated interrupt)
    KeyboardInterrupt = 0x0A, // Input from keyboard or input device
    IOEvent = 0x0B, // Generic I/O-related trap
    InvalidAddress = 0x0C, // Address computation resulted in invalid jump/
    InvalidMicrocodeAddress = 0x0D, // Microcode Address computation resulted in invalid jump/
    WriteProtectionFault = 0x0E, // Write access violation to protected (read-only) memory,
    InvalidExecutionRegion = 0x0F, // Execution attempted from a non-executable memory region (data/stack)

}
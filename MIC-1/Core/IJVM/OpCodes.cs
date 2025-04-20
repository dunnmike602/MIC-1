namespace MLDComputing.Emulators.MIC1.Core.IJVM;

public enum OpCode : byte
{
    FETCH = 0,

    // Push byte onto stack
    BIPUSH = 0x16,

    // Pop two words from stack; pusht heir sum
    IADD = 0x50,

    // Copy top word on stack and push onto stack
    DUP = 0x24,

    // Unconditional Branch
    GOTO = 0xA7,

    // Reserved for Extension Instructions

    // Set Stack Pointer High Byte and Low Byte
    SETSP = 0x80,

    // Terminate execution of machine
    HALT = 0xFF,
}
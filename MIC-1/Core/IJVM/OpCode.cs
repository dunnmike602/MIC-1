using MLDComputing.Emulators.MIC1.Core.MicroCode;

namespace MLDComputing.Emulators.MIC1.Core.IJVM;

public enum OpCode : byte
{
    FETCH = MicroInstructionCode.FETCHStartFetchDecode & 0xFF,

    // Push byte onto stack
    BIPUSH = MicroInstructionCode.BIPUSHCopyPCToMar & 0xFF,

    // Pop two words from stack; pusht heir sum
    IADD = MicroInstructionCode.IADDLoadFirst & 0xFF,

    // Copy top word on stack and push onto stack
    DUP = MicroInstructionCode.DUPLoadTopAddress & 0xFF,

    // Unconditional Branch
    GOTO = MicroInstructionCode.GOTOInit & 0xFF,

    // Branch IF Eq to Zero
    IFEQ = MicroInstructionCode.IFEQLoadTop & 0xFF,

    // Reserved for Extension Instructions

    // Set Stack Pointer High Byte and Low Byte
    SETSP = MicroInstructionCode.SETSPLoadHigh & 0xFF,

    // Terminate execution of machine
    HALT = MicroInstructionCode.HALT & 0xFF,
}
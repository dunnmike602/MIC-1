using MLDComputing.Emulators.MIC1.Core.MicroCode;

namespace MLDComputing.Emulators.MIC1.Core.IJVM;

public enum OpCode : byte
{
    NOP = MicroInstructionCode.FETCHStartFetchDecode & 0xFF,
    
    FETCH = MicroInstructionCode.FETCHStartFetchDecode & 0xFF,

    // Push byte onto stack
    BIPUSH = MicroInstructionCode.BIPUSHCopyPCToMar & 0xFF,

    // Pop two words from stack; push their sum
    IADD = MicroInstructionCode.IADDLoadFirst & 0xFF,

    // Pop two words from stack; push their sum
    ISUB = MicroInstructionCode.ISUBLoadFirst & 0xFF,

    IAND = MicroInstructionCode.IANDLoadFirst & 0xFF,

    IOR = MicroInstructionCode.IORLoadFirst & 0xFF,

    IXOR = MicroInstructionCode.IXORLoadFirst & 0xFF,

    ISHL = MicroInstructionCode.ISHLLoadFirst & 0xFF,

    ISHR = MicroInstructionCode.ISHRLoadFirst & 0xFF,

    IUSHR = MicroInstructionCode.IUSHRLoadFirst & 0xFF,
    
    // Copy top word on stack and push onto stack
    DUP = MicroInstructionCode.DUPLoadTopAddress & 0xFF,

    // Unconditional Branch
    GOTO = MicroInstructionCode.GOTOInit & 0xFF,

    // Branch IF Eq to Zero
    IFEQ = MicroInstructionCode.IFEQLoadTop & 0xFF,
    

    IFNE = MicroInstructionCode.IFNELoadTop & 0xFF,
    
    // Reserved for Extension Instructions

    // Set Stack Pointer High Byte and Low Byte
    SETSP = MicroInstructionCode.SETSPLoadHigh & 0xFF,

    POP = MicroInstructionCode.POPIncrementSP & 0xFF,
    
    SWAP = MicroInstructionCode.POPIncrementSP & 0xFF,
    
    // Terminate execution of machine
    HALT = MicroInstructionCode.HALT & 0xFF,
    
    // Pseudo Instruction for Managing the TOS
    TOS = MicroInstructionCode.ReloadTOSSetMAR,
}
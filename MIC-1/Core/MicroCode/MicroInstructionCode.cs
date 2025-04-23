namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

public enum MicroInstructionCode
{
    FETCHStartFetchDecode = 0,
    FETCHLoadMAR,
    FETCHReadInstruction,
    FETCHIncPC,
   
    BIPUSHCopyPCToMar = 0x116,
    BIPUSHReadOperand,
    BIPUSHSignExtendAndHold,
    BIPUSHDecrementSP,
    BIPUSHCopySPToMAR,
    BIPUSHWriteToStack,

    DUPLoadTopAddress = 0x124,
    DUPReadTop,
    DUPDecSP,
    DUPAddressForCopy,
    DUPWriteCopy,

    IADDLoadFirst = 0x150,
    IADDReadFirst,
    IADDLoadSecond,
    IADDReadSecond,
    IADDComputeResult,
    IADDDecSP,
    IADDSetResultAddress,
    IADDStoreResult,

    SETSPLoadHigh = 0x180,
    SETSPReadHigh,
    SETSPStoreHigh,
    SETSPIncPCBeforeLow,
    SETSPLoadLow,
    SETSPReadLow,
    SETSPCombineAndStore,
    SETSPIncrementPC,

    IFEQLoadTop = 0x199,
    IFEQReadTopAndIncSP,
    IFEQStoreTopInH,
    IFEQIncPCForLow,
    IFEQCheckZero,
    IFEQLoadLow,
    IFEQReadLow,
    IFEQSkipOperandsAndReturn,

    GOTOInit = 0x1A7,
  
    // Note that 0x1C0–0x1FF are used by the jump vector table and should not be overwritten
    JUMPTableStart = 0x1C0,
    JUMPTableEnd = 0x1DF,

    JUMPDecrementPC = 0x1E0,
    JUMPLoadMARHigh,
    JUMPStoreHigh,
    JUMPIncrementPCForLow,
    JUMPReadLow,
    JUMPCombineOffset,
    
    HALT = 0x1FF,
}
namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

public enum MicroInstructionCode
{
    Uninitialised = -1,
    
    FETCHStartFetchDecode = 0,
    FETCHLoadMAR,
    FETCHReadInstruction,
    FETCHIncPC,
   
    BIPUSHCopyPCToMar = 0x116,
    BIPUSHReadOperand,
    BIPUSHSignExtendAndHold,
    BIPUSHDecrementSP,
    BIPUSHCopySPToMAR,
    BIPUSHLoadHToMDR,
    BIPUSHWriteHigh,
    BIPUSHAdd2ToMAR,
    BIPUSHWriteLow,

    DUPLoadTopAddress = 0x124,
    DUPReadTopHigh,
    DUPAdd2ToMAR,
    DUPReadTopLow,
    DUPDecSP,
    DUPAddressForCopy,
    DUPWriteCopyHigh,
    DUPAdd2ToMARCopy,
    DUPWriteCopyLow,

    IADDLoadFirst = 0x150,
    IADDReadTopHigh,
    IADDAdd2ToMARFirst,
    IADDReadTopLow,
    IADDIncrementSPAfterFirst,
    IADDLoadSecondAddress,
    IADDReadSecondHigh,
    IADDAdd2ToMARSecond,
    IADDReadSecondLow,
    IADDComputeResult,
    IADDDecrementSPAfterAdd,
    IADDSetResultAddress,
    IADDWriteResultHigh,
    IADDAdd2ToMARWrite,
    IADDWriteResultLow,

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
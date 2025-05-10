namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

public enum MicroInstructionCode
{
    Uninitialised = -1,
    
    FETCHStartFetchDecode = 0,
    FETCHLoadMAR = 1,
    FETCHReadInstruction = 2,
    FETCHIncPC = 3,
    
    BIPUSHCopyPCToMar = 0x116,
    BIPUSHReadOperand = 20,
    BIPUSHSignExtendAndHold = 21,
    BIPUSHDecrementSP = 22,
    BIPUSHCopySPToMAR = 23,
    BIPUSHLoadHToMDR = 24,
    BIPUSHWriteHigh = 25,
    BIPUSHAdd2ToMAR = 26,
    BIPUSHWriteLow = 27,
    BIPUSHCacheTOS = 28,

    DUPLoadTopAddress = 0x124,
    DUPReadTopHigh = 40,
    DUPAdd2ToMAR= 41,
    DUPReadTopLow = 42,
    DUPDecSP = 43,
    DUPAddressForCopy = 44,
    DUPWriteCopyHigh = 45,
    DUPAdd2ToMARCopy= 46,
    DUPWriteCopyLow = 47,
    DUPCacheTOS = 48,
    
    SETSPLoadHigh = 0x180,
    SETSPReadHigh = 50,
    SETSPStoreHigh = 51,
    SETSPIncPCBeforeLow = 52,
    SETSPLoadLow = 53,
    SETSPReadLow = 54,
    SETSPCombineAndStore = 55,
    SETSPIncrementPC = 56,

    IFEQLoadTop = 0x199,
    IFEQReadTopHigh = 60,
    IFEQAdd2ToMAR = 61,
    IFEQReadTopLow = 62,
    IFEQStoreTopInH = 63,
    IFEQIncrementSP = 64,
    IFEQIncPCForLow = 65,
    IFEQLoadLow = 66,
    IFEQReadLow = 67,
    IFEQCheckZero = 68,
    IFEQSkipOperandsAndReturn = 69,

    IFNELoadTop = 0x19A,
    IFNEReadTopHigh = 80,
    IFNEAdd2ToMAR = 81,
    IFNEReadTopLow = 82,
    IFNEStoreTopInH = 83,
    IFNEIncrementSP = 84,
    IFNEIncPCForLow = 85,
    IFNELoadLow = 86,
    IFNEReadLow = 87,
    IFNECheckZero = 88 ,
    IFNESkipOperandsAndReturn = 89,

    IADDLoadFirst = 0x150,
    IADDLoadNext = 100,

    ISUBLoadFirst = 0x164,
    ISUBLoadNext = 120,

    IANDLoadFirst = 0x17E,
    IANDLoadNext = 140,

    IORLoadFirst = 0x1B0,
    IORLoadNext = 160,

    IXORLoadFirst = 0x182,
    IXORLoadNext = 180,
    
    GOTOInit = 0x1A7,
  
    // Note that 0x1C0–0x1FF are used by the jump vector table and should not be overwritten
    JUMPTableStart = 0x1C0,
    JUMPTableEnd = 0x1DF,

    JUMPDecrementPC = 0x1E0,
    JUMPLoadMARHigh = 0x1E1,
    JUMPStoreHigh = 0x1E2,
    JUMPIncrementPCForLow = 0x1E3,
    JUMPReadLow = 0x1E4,
    JUMPCombineOffset = 0x1E5,
    
    HALT = 0x1FF,
}
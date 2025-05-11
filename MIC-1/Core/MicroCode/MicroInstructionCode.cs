namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

public enum MicroInstructionCode
{
    Uninitialised = -1,
    
    FETCHStartFetchDecode = 0,
    FETCHLoadMAR = 1,
    FETCHReadInstruction = 2,
    FETCHIncPC = 3,
    
    BIPUSHCopyPCToMar = 0x116,
    BIPUSHReadOperand = 5,
    BIPUSHSignExtendAndHold = 6,
    BIPUSHDecrementSP = 7,
    BIPUSHCopySPToMAR = 8,
    BIPUSHLoadHToMDR = 9,
    BIPUSHWriteHigh = 10,
    BIPUSHAdd2ToMAR = 11,
    BIPUSHWriteLow = 12,
    BIPUSHCacheTOS = 13,

    DUPLoadTopAddress = 0x124,
    DUPReadTopHigh = 14,
    DUPAdd2ToMAR= 15,
    DUPReadTopLow = 16,
    DUPDecSP = 17,
    DUPAddressForCopy = 18,
    DUPWriteCopyHigh = 19,
    DUPAdd2ToMARCopy= 20,
    DUPWriteCopyLow = 21,
    
    SETSPLoadHigh = 0x180,
    SETSPReadHigh = 23,
    SETSPStoreHigh = 24,
    SETSPIncPCBeforeLow = 25,
    SETSPLoadLow = 26,
    SETSPReadLow = 27,
    SETSPCombineAndStore = 28,
    SETSPIncrementPC = 29,

    IFEQLoadTop = 0x199,
    IFEQReadTopHigh = 30,
    IFEQAdd2ToMAR = 31,
    IFEQReadTopLow = 32,
    IFEQStoreTopInH = 33,
    IFEQIncrementSP = 34,
    IFEQIncPCForLow = 35,
    IFEQLoadLow = 36,
    IFEQReadLow = 37,
    IFEQCheckZero = 38,
    IFEQSkipOperandsAndReturn = 39,

    IFNELoadTop = 0x19A,
    IFNEReadTopHigh = 45,
    IFNEAdd2ToMAR = 46,
    IFNEReadTopLow = 47,
    IFNEStoreTopInH = 48,
    IFNEIncrementSP = 49,
    IFNEIncPCForLow = 50,
    IFNELoadLow = 51,
    IFNEReadLow = 52,
    IFNECheckZero = 53 ,
    IFNESkipOperandsAndReturn = 54,

    IADDLoadFirst = 0x150,
    IADDLoadNext = 60,

    ISUBLoadFirst = 0x164,
    ISUBLoadNext = 75,

    IANDLoadFirst = 0x17E,
    IANDLoadNext = 90,

    IORLoadFirst = 0x1B0,
    IORLoadNext = 105,

    IXORLoadFirst = 0x182,
    IXORLoadNext = 120,

    ISHLLoadFirst = 0x178,
    ISHLLoadNext = 135,
    
    ISHRLoadFirst = 0x17A,
    ISHRLoadNext = 150,
    
    IUSHRLoadFirst = 0x17C,
    IUSHRLoadNext = 165,

    // Common routine for managing the TOS
    ReloadTOSSetMAR = 180,
    ReloadTOSReadHigh = 181,
    ReloadTOSIncMAR = 182,
    ReloadTOSReadLow = 183,
    ReloadTOSSetTOS = 184, 
    
    POPIncrementSP = 0x157,

    SWAPLoadSPPlus4 = 0x5F,
    SWAPReadSPPlus4High = 190,
    SWAPAdd2ToMAR1 = 191,
    SWAPReadSPPlus4Low = 192,
    SWAPSaveValue2ToH = 193,
    SWAPLoadTopAddress = 194,
    SWAPReadTopHigh = 195,
    SWAPAdd2ToMAR2 = 196,
    SWAPReadTopLow = 197,
    SWAPWriteValue1ToSPPlus4 = 198,
    SWAPWriteValue1ToSPPlus6 = 199,
    SWAPWriteValue2ToSP = 200,
    SWAPWriteValue2ToSPPlus2 = 201,

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
namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

public enum MicroInstructionCode
{
    FetchStartFetchDecode = 0x0000,
    FetchLoadMAR,
    FetchReadInstruction,
    FetchIncPC,

    BIPUSHCopyPCToMar = 0x0016,
    BIPUSHReadOperand,
    BIPUSHSignExtendAndHold,
    BIPUSHDecrementSP,
    BIPUSHCopySPToMAR,
    BIPUSHWriteToStack,

    DUPLoadTopAddress = 0x24,
    DUPReadTop,
    DUPDecSP,
    DUPAddressForCopy,
    DUPWriteCopy,

    IADDLoadFirst = 0x50,
    IADDReadFirst,
    IADDLoadSecond,
    IADDReadSecond,
    IADDComputeResult,
    IADDDecSP,
    IADDSetResultAddr,
    IADDStoreResult,

    GOTOLoadMAR = 0xA7,
    GOTOReadHigh,
    GOTOIncrementPC,
    GOTOStoreHigh,
    GOTOLoadMARLow,
    GOTOIncrementPCFinal,
    GOTOReadLow,
    GOTOCombineOffset,
    GOTOReturnToFetch,

    SETSPLoadHigh = 0x80,
    SETSPReadHigh,
    SETSPStoreHigh,
    SETSPIncPCBeforeLow,
    SETSPLoadLow,
    SETSPReadLow,
    SETSPCombineAndStore,
    SETSPIncrementPC,

    // HALT to terminate a program
    HALT = 0x0FF,
}
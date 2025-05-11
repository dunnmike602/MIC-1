namespace MLDComputing.Emulators.MIC1.Core.Bus;

public enum MemoryOperation
{
    NoOp,
    ReadWordToMDRHigh,   
    ReadWordToMDRLow,    
    WriteWordFromMDRHigh, 
    WriteWordFromMDRLow,
    ReadByteToMBR,
    WriteByteFromMBR,
    WriteWordFromHHigh,
    WriteWordFromHLow
}
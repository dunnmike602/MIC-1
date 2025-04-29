namespace MLDComputing.Emulators.MIC1.Exceptions;

using Core.Enums;
using Interfaces;

public class MemoryFaultException(long address, bool isWordAccess)
    : InvalidOperationException(FormatMemoryAccessViolationMessage(address, isWordAccess)), IFaultException
{
    public TrapCode TrapCode { get; set; } = TrapCode.SegmentationFault;

    private static string FormatMemoryAccessViolationMessage(long address, bool isWordAccess)
    {
        return $"{(isWordAccess ? "Word" : "Byte")} Memory access violation (Address: 0x{address:X4})";
    }
}
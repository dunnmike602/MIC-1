namespace MLDComputing.Emulators.MIC1.Exceptions;

using Core.Bus;
using Core.Enums;
using Interfaces;

public class WriteProtectException(long address) : InvalidOperationException(GetValue(address)), IFaultException
{
    public TrapCode TrapCode { get; set; } = TrapCode.WriteProtectionFault;

    private static string GetValue(long address)
    {
        return
            $"Write to program region {MemoryLayout.CodeSegment.Start:X4} to {MemoryLayout.CodeSegment.End:X4} denied (Address: 0x{address:X4})";
    }
}
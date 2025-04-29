namespace MLDComputing.Emulators.MIC1.Exceptions;

using Core.Bus;
using Core.Enums;
using Interfaces;

public class ExecutionProtectionException(long address, MemoryRegion region)
    : InvalidOperationException($"Execution not allowed from address 0x{address:X4} ({region})"), IFaultException
{
    public TrapCode TrapCode { get; set; } = TrapCode.InvalidExecutionRegion;
}
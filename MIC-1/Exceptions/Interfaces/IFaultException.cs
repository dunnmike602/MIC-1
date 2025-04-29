namespace MLDComputing.Emulators.MIC1.Exceptions.Interfaces;

using Core.Enums;

public interface IFaultException
{
    public TrapCode TrapCode { get; set; }
}
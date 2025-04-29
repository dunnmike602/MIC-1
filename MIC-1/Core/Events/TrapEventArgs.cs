namespace MLDComputing.Emulators.MIC1.Core.Events;

using Enums;

public class TrapEventArgs(string? message, TrapCode trapCode, string? information) : EventArgs
{
    public string? Message = message;

    public TrapCode TrapCode = trapCode;

    public string? Information = information;
}
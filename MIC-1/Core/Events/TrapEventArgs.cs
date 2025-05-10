namespace MLDComputing.Emulators.MIC1.Core.Events;

using Enums;

public class TrapEventArgs(string? message, TrapCode trapCode, string? information, string registerSnapshot, DateTime dateTime) : EventArgs
{
    public DateTime DateTime = dateTime;

    public TrapCode TrapCode = trapCode;
    
    public string? Message = message;
    
    public string? Information = information;

    public string RegisterSnapshot = registerSnapshot;
 
}
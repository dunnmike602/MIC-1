namespace MLDComputing.Emulators.MIC1.Core.Events;

using Enums;
using MicroCode;

public class ExecuteEventArgs(
    MicroInstruction? instruction,
    long cycleCount,
    long ijvmCycleCount,
    long elapsedTicks,
    ExecutionEventCode eventCode)
    : EventArgs
{
    public MicroInstruction? Instruction = instruction;
    
    public long CycleCount = cycleCount;

    public long IJVMCycleCount = ijvmCycleCount;

    public long ElapsedTicks = elapsedTicks;

    public ExecutionEventCode Event = eventCode;
}
namespace MLDComputing.Emulators.MIC1.Core.Events;

using Bus;
using MicroCode;

public class ExecuteEventArgs : EventArgs
{
    public MicroInstruction Instruction;

    public Registers Registers;
    
    public long CycleCount;

    public long IJVMCycleCount;

    public long ElapsedTicks;

    public ExecuteEventArgs(MicroInstruction instruction, Registers registers, long cycleCount, long ijvmCycleCount, long elapsedTicks)
    {
        Instruction = instruction;
        Registers = registers;
        CycleCount = cycleCount;
        IJVMCycleCount = ijvmCycleCount;
        ElapsedTicks = elapsedTicks;
    }
}
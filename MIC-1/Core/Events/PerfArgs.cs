namespace MLDComputing.Emulators.MIC1.Core.Events;

public class PerfArgs(DateTime dateTime, long cycleCount, long processorTicks) : EventArgs
{
    public DateTime DateTime = dateTime;

    public long CycleCount = cycleCount;

    public long ProcessorTick = processorTicks;
}
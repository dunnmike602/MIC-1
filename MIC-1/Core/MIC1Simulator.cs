namespace MLDComputing.Emulators.MIC1.Core;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Bus;
using Constants;
using Enums;
using Events;
using Extensions;
using MicroCode;
using Exceptions;
using Exceptions.Interfaces;
using Interop;
using static Bus.MemoryLayout;

public sealed class MIC1Simulator : IDisposable
{
    private bool _haltDetected;

    private readonly MicroInstruction[] _microCodeControlStore = MicroCodeStoreBuilder.Build();

    public readonly int VideoFrameRate;
    
    public readonly int TargetProcessorSpeed;

    public long[] PerOpcodeCycles = new long[SimulatorConstants.MicroCode.StoreSize];

    private bool _executingInstruction;

    public bool ShowDetailedStats;

    public bool UnThrottled;

    public bool MemoryChecking;

    public int MemorySize;

    public long CycleCount;

    public long IJVMCycleCount;

    public long InstructionCycleCount;

    public long VideoFrameIntervalTicks;
    
    public long PipeLineLength;

    public long FrameCount;

    public long FrameRate;

    public long PerformanceCountInterval;

    public Stopwatch TotalElapsedTime = new();

    public Stopwatch TotalElapsedIdleTime = new();

    public bool EnableExecutionEvents;

    private bool _disposed;

    public event EventHandler<ExecuteEventArgs>? ExecutionEvent;

    public event EventHandler<TrapEventArgs>? TrapEvent;

    public event EventHandler<PerfArgs>? PerfEvent;
    
    public uint CurrentCore { get; } = MachineHelper.GetCurrentCore();

    public bool HaltDetected
    {
        get => _haltDetected;
        set
        {
            _haltDetected = value;

            if (_haltDetected)
            {
                TotalElapsedIdleTime.Start();
                TotalElapsedTime.Stop();
            }
            else
            {
                TotalElapsedIdleTime.Stop();
                TotalElapsedTime.Start();
            }
        }
    }

    private void OnTrap(TrapCode trapCode, string message, string? information)
    {
        TrapEvent?.Invoke(this, new TrapEventArgs(message, trapCode, information, Registers.GetRegisterSnapshot(), DateTime.Now));
    }

    private void OnExecutionEvent(MicroInstruction? mi, ExecutionEventCode eventCode)
    {
        ExecutionEvent?.Invoke(this,
            new ExecuteEventArgs(mi, CycleCount, IJVMCycleCount, TotalElapsedTime.ElapsedTicks, eventCode));
    }

    private void OnPerfEvent(DateTime dateTime, long cycleCount, long processorTick)
    {
        PerfEvent?.Invoke(this,
            new PerfArgs(dateTime, cycleCount, processorTick));
    }
    
    public MIC1Simulator(bool enableExecutionEvents = true, 
        int memorySize = SimulatorConstants.Machine.DefaultMemorySize,
        int targetProcessorSpeed = SimulatorConstants.Machine.DefaultTargetFrequencyHz, 
        int videoFrameRate = SimulatorConstants.Machine.DefaultRefreshRateHz,
        bool unThrottled = false, 
        bool memoryChecking = true,
        bool showDetailedStats = true,
        bool inHaltMode = true,
        int performanceCountInterval = 2)
    {
        // Machine Setup
        MemorySize = memorySize;
        VideoFrameRate = videoFrameRate;
        TargetProcessorSpeed = targetProcessorSpeed;
        UnThrottled = unThrottled;
        MemoryChecking = memoryChecking;
        
        // Dependant variable for machine speed
        VideoFrameIntervalTicks = (long)((1 / (float)videoFrameRate) * Stopwatch.Frequency);
        PipeLineLength = GetNumberOfInstructionsPerVideoRefresh();
        
        // Stats Setup
        ShowDetailedStats = showDetailedStats;
        EnableExecutionEvents = enableExecutionEvents;
        PerformanceCountInterval = performanceCountInterval;
        
        HaltDetected = inHaltMode;
        
        Init();
    }

    private int GetNumberOfInstructionsPerVideoRefresh()
    {
        var cyclesPerTick = TargetProcessorSpeed / (float)Stopwatch.Frequency;

        return (int)(cyclesPerTick * VideoFrameIntervalTicks);
    }
    
    public void Init()
    {
        Registers.Reset();
        Memory.Init(MemorySize, MemoryChecking);

        PerOpcodeCycles = new long[SimulatorConstants.MicroCode.StoreSize];

        CycleCount = 0;
        FrameCount = 0;
        InstructionCycleCount = 0;
        IJVMCycleCount = 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(CancellationToken ct, bool clearState = false, int pc = 0, params byte[] bytes)
    {
        try
        {
            RunInternal(clearState, pc, bytes, ct);
        }
        finally
        {
            Memory.Dispose();
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RunInternal(bool clearState, int pc, byte[] bytes, CancellationToken ct)
    {
        if (clearState)
        {
            Init();
        }

        Registers.PC = pc;

        if (bytes.Length > 0)
        {
            Memory.LoadBootProgram(pc, bytes);
        }

        Registers.MPC = (int)MicroInstructionCode.FETCHStartFetchDecode;


        var performanceCycleCount = CycleCount;
        var performanceElapsedTicks = TotalElapsedTime.Elapsed;
        
        while (!ct.IsCancellationRequested)
        {
            if (UnThrottled)
            {
                if (!_haltDetected)
                {
                    RunInUnthrottledMode();
                }
                else
                {
                    SleepCore();
                }
            }
            else
            {
                if (!_haltDetected)
                {
                    RunInThrottledMode();
                }
                else
                {
                    SleepCore();
                }
            }

            if (EnableExecutionEvents && !_haltDetected)
            {
                var sinceLastEvent = TotalElapsedTime.Elapsed - performanceElapsedTicks;

                if (sinceLastEvent.TotalSeconds >= PerformanceCountInterval)
                {
                    OnPerfEvent(DateTime.UtcNow, CycleCount - performanceCycleCount,
                        sinceLastEvent.Ticks);

                    performanceCycleCount = CycleCount;
                    performanceElapsedTicks = TotalElapsedTime.Elapsed;
                }
            }
        }

        TotalElapsedTime.Stop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RunInThrottledMode()
    {
        // Throttled peg to frame rate
        var frameStartTicks = Stopwatch.GetTimestamp();
        var cycles = 0;
        
        while (cycles <= PipeLineLength & !_haltDetected)
        {
            Execute();
            cycles++;
        }

        FrameCount++;

        var frameEndTicks = Stopwatch.GetTimestamp();
        var frameDurationTicks = frameEndTicks - frameStartTicks;

        // Target frame time in ticks (e.g., 60 FPS = 1/60s)
        var targetTicksPerFrame = (long)(Stopwatch.Frequency / VideoFrameRate);
        var quantumToWait = targetTicksPerFrame - frameDurationTicks;

        if (quantumToWait > 0)
        {
            var waitUntil = Stopwatch.GetTimestamp() + quantumToWait;
            while (Stopwatch.GetTimestamp() < waitUntil)
            {
                Thread.SpinWait(1);
            }
        }

        // Optional: calculate actual frame rate
        var elapsedSeconds = (double)(Stopwatch.GetTimestamp() - frameStartTicks) / Stopwatch.Frequency;
        FrameRate = (long)(1.0 / elapsedSeconds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RunInUnthrottledMode()
    { 
        Execute();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SleepCore()
    {
        // Cycle through waits until processor is started again
        Thread.Sleep(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void JumpToNext(MicroInstruction mi)
    {
        // Start with the base next address
        var next = mi.Address;

        if ((mi.JAM & (byte)JAMControl.JAMC) != 0)
        {
            next |= 0x100 | Registers.MBR;
        }

        // Check for JAMZ_EQ (Jump if Zero) or JAMZ_NE (Jump if Not Zero)
        if ((mi.JAM & (byte)JAMControl.JAMZ_EQ) != 0 && ALU.IsZero)
        {
            next = (int)MicroInstructionCode.JUMPTableStart | (Registers.MBR & 0x3F);
        }
        else if ((mi.JAM & (byte)JAMControl.JAMZ_NE) != 0 && !ALU.IsZero)
        {
            next = (int)MicroInstructionCode.JUMPTableStart | (Registers.MBR & 0x3F);
        }

        Registers.MPC = next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckForTermination()
    {
        if (MemoryChecking && !IsInStack(Registers.SP))
        {
            TrapCode trapCode;
            string message;
            if (Registers.SP > StackSegment.Top)
            {
                trapCode = TrapCode.StackUnderflow;
                message = "ABNORMAL TERMINATION - STACK UNDERFLOW DETECTED";
            }
            else
            {
                trapCode = TrapCode.StackOverflow;
                message = "ABNORMAL TERMINATION - STACK OVERFLOW DETECTED";
            }

            var information =
                $"SP = 0x{"SP".FormatRegister(Registers.SP, NumberFormat.Hex)} is outside valid stack range " +
                $"(0x{StackSegment.Bottom:X4}–0x{StackSegment.Top:X4})";

            if (EnableExecutionEvents)
            {
                OnTrap(trapCode, message, information);
            }

            HaltDetected = true;
        }

        if (Registers.MPC != (int)MicroInstructionCode.HALT)
        {
            return;
        }
        
        if (ShowDetailedStats)
        {
            // Processor accounting HALT is a special instruction so it is trapped and logged separately
            IJVMCycleCount++;
        }

        HaltDetected = true;

        if (EnableExecutionEvents)
        {
            OnExecutionEvent(null, ExecutionEventCode.Halted);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void Execute()
    {
        fixed (MicroInstruction* pMicroStore = _microCodeControlStore)
        {
            ref readonly var mi = ref _microCodeControlStore[Registers.MPC];

            if (mi.Key == MicroInstructionCode.Uninitialised)
            {
                if (EnableExecutionEvents)
                {
                    OnTrap(TrapCode.InvalidMicrocodeAddress,
                        $"ABNORMAL TERMINATION - NO MICROCODE ROUTINE FOUND AT: 0x{Registers.MPC:X2}.", null);
                }

                HaltDetected = true;
                return;
            }

            if (EnableExecutionEvents)
            {
                OnExecutionEvent(mi, ExecutionEventCode.InstructionStarted);
            }

            // Every time through this execute we do one micro instruction only so that is 1 cycle
            CycleCount++;

            if (ShowDetailedStats)
            {
                PerformIJVMAccounting();
            }

            CheckForTermination();

            var aluResult = ALU.Calculate(mi);

            try
            {
                if (mi.MemoryAfterRegisterWrite)
                {
                    Registers.SetRegisterFromALU(aluResult, mi.C);
                    Memory.Access(mi);
                }
                else
                {
                    Memory.Access(mi);
                    Registers.SetRegisterFromALU(aluResult, mi.C);
                }
            }
            catch (Exception e) when (e is MemoryFaultException or WriteProtectException
                                          or ExecutionProtectionException)
            {
                var trapCode = ((IFaultException)e).TrapCode;

                TrapEvent?.Invoke(this, new TrapEventArgs(e.Message, trapCode, null, Registers.GetRegisterSnapshot(), DateTime.Now));
                HaltDetected = true;
                return;
            }

            JumpToNext(mi);

            if (EnableExecutionEvents)
            {
                OnExecutionEvent(mi, ExecutionEventCode.InstructionEnded);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PerformIJVMAccounting()
    {
        // We are in the Fetch Decode Cycle
        if (Registers.MPC is >= (int)MicroInstructionCode.FETCHStartFetchDecode
            and <= (int)MicroInstructionCode.FETCHReadInstruction)
        {
            // Update FETCH accounting this is not part of each IJVM opcode
            PerOpcodeCycles[(int)MicroInstructionCode.FETCHStartFetchDecode]++;
            _executingInstruction = false;
        }
        // We are Just Entering the instruction
        else if (Registers.MPC is >= (int)MicroInstructionCode.FETCHStartFetchDecode
                 and <= (int)MicroInstructionCode.FETCHIncPC)
        {
            PerOpcodeCycles[(int)MicroInstructionCode.FETCHStartFetchDecode] += 1;
            _executingInstruction = true;
            IJVMCycleCount++;

            // We can pick up the IJVM Instruction here
            Registers.CurrentOpcode = Registers.MBR;
        }
        // We are executing an instruction
        else if (_executingInstruction)
        {
            PerOpcodeCycles[Registers.CurrentOpcode]++;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Memory.Dispose();
    }

    public void Start()
    {
        HaltDetected = false;

        if (EnableExecutionEvents)
        {
            OnExecutionEvent(null, ExecutionEventCode.Started);
        }
    }
}
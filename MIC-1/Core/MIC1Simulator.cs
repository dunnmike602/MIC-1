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
using static Bus.MemoryLayout;

public sealed class MIC1Simulator : IDisposable
{
    private bool _haltDetected;

    private readonly MicroInstruction[] _microCodeControlStore = MicroCodeStoreBuilder.Build();

    public readonly bool CollectInstructionStats;

    public readonly int TargetHz;

    public long[] PerOpcodeCycles = new long[SimulatorConstants.MicroCode.StoreSize];

    private bool _executingInstruction;

    public readonly bool ShowDetailedStats;

    public bool UnThrottled;

    public bool MemoryChecking;

    public int MemorySize;

    public long CycleCount;

    public long IJVMCycleCount;

    public long InstructionCycleCount;

    public Memory Memory = new(1, false);

    public Registers Registers = new();

    public Stopwatch TotalElapsedTime = new();

    public bool EnableExecutionEvents;

    private bool _disposed;

    public event EventHandler<ExecuteEventArgs>? ExecutionStarted;

    public event EventHandler<ExecuteEventArgs>? ExecutionEnded;

    public event EventHandler<TrapEventArgs>? Trap;

    private void OnTrap(TrapCode trapCode, string message, string? information)
    {
        if (EnableExecutionEvents)
        {
            Trap?.Invoke(this, new TrapEventArgs(message, trapCode, information));
        }
    }

    private void OnExecutionStarted(MicroInstruction mi)
    {
        if (EnableExecutionEvents)
        {
            ExecutionStarted?.Invoke(this,
                new ExecuteEventArgs(mi, Registers, CycleCount, IJVMCycleCount, TotalElapsedTime.ElapsedTicks));
        }
    }

    private void OnExecutionEnded(MicroInstruction mi)
    {
        if (EnableExecutionEvents)
        {
            ExecutionEnded?.Invoke(this,
                new ExecuteEventArgs(mi, Registers, CycleCount, IJVMCycleCount, TotalElapsedTime.ElapsedTicks));
        }
    }

    public MIC1Simulator(bool enableExecutionEvents = true, int memorySize = SimulatorConstants.DefaultMemorySize,
        int targetHz = SimulatorConstants.DefaultTargetFrequencyHz,
        bool unThrottled = false, bool memoryChecking = true,
        bool collectInstructionStats = true, bool showDetailedStats = true)
    {
        CollectInstructionStats = collectInstructionStats;
        ShowDetailedStats = showDetailedStats;
        EnableExecutionEvents = enableExecutionEvents;
        TargetHz = targetHz;
        MemorySize = memorySize;
        UnThrottled = unThrottled;
        MemoryChecking = memoryChecking;

        Init(true);
    }

    public void Init(bool clearState)
    {
        if (clearState)
        {
            Registers = new Registers();
            Memory = new Memory(MemorySize, MemoryChecking);

            TotalElapsedTime = new Stopwatch();

            PerOpcodeCycles = new long[SimulatorConstants.MicroCode.StoreSize];

            CycleCount = 0;
            InstructionCycleCount = 0;
            IJVMCycleCount = 0;
            _haltDetected = false;
            TotalElapsedTime.Restart();
        }
    }

    public void Run(bool clearState = false, int pc = 0, params byte[] bytes)
    {
        try
        {
            RunInternal(clearState, pc, bytes);
        }
        finally
        {
            Memory.Dispose();
        }

    }

    private void RunInternal(bool clearState, int pc, byte[] bytes)
    {
        Init(clearState);
        Registers.PC = pc;

        if (bytes.Length > 0)
        {
            Memory.LoadProgram(pc, bytes);
        }

        Registers.MPC = (int)MicroInstructionCode.FETCHStartFetchDecode;

        var ticksPerCycle = Stopwatch.Frequency / TargetHz;
        var startTicks = Stopwatch.GetTimestamp();

        while (!_haltDetected)
        {
            if (UnThrottled)
            {
                Execute();
                continue;
            }

            var currentTicks = Stopwatch.GetTimestamp();
            var elapsedTicks = currentTicks - startTicks;
            var expectedCycles = elapsedTicks / ticksPerCycle;

            while (CycleCount < expectedCycles && !_haltDetected)
            {
                Execute();
            }

            if (CycleCount >= expectedCycles)
            {
                Thread.SpinWait(50); // light CPU back-off
            }
        }
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

        // Check for JAMZ — if enabled and H == 0 (low byte only), OR MBR into MPC
        if ((mi.JAM & (byte)JAMControl.JAMZ) != 0 && ALU.IsZero)
        {
            // This is start of the jump table that will have 64 bytes set to execute the same instruction
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
            if (Registers.SP > StackSegment.End)
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
                $"(0x{StackSegment.Start:X4}–0x{StackSegment.End:X4})";

            OnTrap(trapCode, message, information);

            _haltDetected = true;
        }

        if (Registers.MPC != (int)MicroInstructionCode.HALT)
        {
            return;
        }

        OnTrap(TrapCode.Halt, "NORMAL TERMINATION - HALT DETECTED", null);

        if (ShowDetailedStats)
        {
            // Processor accounting HALT is a special instruction so it is trapped and logged separately
            IJVMCycleCount++;
        }

        _haltDetected = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void Execute()
    {
        ref var reg = ref Registers;

        fixed (MicroInstruction* pMicroStore = _microCodeControlStore)
        {
            ref readonly var mi = ref _microCodeControlStore[reg.MPC];

            if (mi.Key == MicroInstructionCode.Uninitialised)
            {
                OnTrap(TrapCode.InvalidMicrocodeAddress,
                    $"ABNORMAL TERMINATION - NO MICROCODE ROUTINE FOUND AT: 0x{Registers.MPC:X2}.", null);
                _haltDetected = true;
                return;
            }

            OnExecutionStarted(mi);

            // Every time through this execute we do one micro instruction only so that is 1 cycle
            CycleCount++;

            if (ShowDetailedStats)
            {
                PerformIJVMAccounting();
            }

            CheckForTermination();

            var aluResult = ALU.Calculate(reg, mi);

            try
            {
                if (mi.MemoryAfterRegisterWrite)
                {
                    reg.SetRegisterFromALU(aluResult, mi.C);
                    Memory.Access(Registers, mi);
                }
                else
                {
                    Memory.Access(Registers, mi);
                    reg.SetRegisterFromALU(aluResult, mi.C);
                }
            }
            catch (Exception e) when (e is MemoryFaultException or WriteProtectException
                                          or ExecutionProtectionException)
            {
                var trapCode = ((IFaultException)e).TrapCode;

                Trap?.Invoke(this, new TrapEventArgs(e.Message, trapCode, null));
                _haltDetected = true;
                return;
            }

            JumpToNext(mi);

            OnExecutionEnded(mi);
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
}
namespace MLDComputing.Emulators.MIC1.Core;

using System.Diagnostics;
using Bus;
using Constants;
using Display;
using Enums;
using IJVM;
using Memory;
using MicroCode;

public class MIC1Simulator
{
    private readonly NumberFormat _numberFormat;

    private readonly int _targetHz;

    private readonly int _refreshRate;

    private bool _haltDetected;

    // Control Store (Microcode Storage)
    private Dictionary<MicroInstructionCode, MicroInstruction> _microCodeControlStore = [];

    public Dictionary<OpCode, long> PerOpcodeCycles = new();

    private int _cyclesPerFrame;

    private float _msPerFrame;

    private Stopwatch _totalTime = new Stopwatch();

    private bool _unThrottled;

    public long CycleCount { get; private set; }

    public long IJVMCycleCount { get; private set; }

    public long InstructionCycleCount { get; private set; }

    public Memory.Memory Memory { get; set; }

    public Registers Registers { get; set; } = new();

    public MIC1Simulator(bool verboseLogging = true, int memorySize = SimulatorConstants.DefaultMemorySize,
        NumberFormat numberFormat = NumberFormat.Hex, int targetHz = SimulatorConstants.DefaultTargetFrequencyHz,
        int refreshRate = SimulatorConstants.DefaultRefreshRateHz)
    {
        _numberFormat = numberFormat;
        _targetHz = targetHz;
        _refreshRate = refreshRate;

        Memory = new Memory.Memory(memorySize);

        Init(memorySize, verboseLogging);
    }

    private void Init(int memorySize, bool verboseLogging)
    {
        _cyclesPerFrame = _targetHz / _targetHz;
        _msPerFrame = SimulatorConstants.MillisecondsPerSecond / _refreshRate;
        
        DebugConsole.Verbose = verboseLogging;

        InitializeMicrocode();
    }

    public async Task Run(bool unThrottled = false)
    {
        _unThrottled = unThrottled;

        _haltDetected = false;

        _totalTime.Restart();

        // Set MicroProgramCounter to 0 to repeatedly run the Fetch/Decode cycle
        while (!_haltDetected)
        {
            Registers.MPC = (int)MicroInstructionCode.FetchStartFetchDecode;

            await ProcessNextInstruction();
        }

        _totalTime.Stop();
    }

    private void JumpToNext(MicroInstruction mi)
    {
        // base address field (9 or 12 bits depending on your store size)
        var next = mi.Address;

        // JAM bit 0 → opcode dispatch
        if ((mi.JAM & 0b1) != 0)
        {
            next |= Registers.MBR & 0xFF; // OR in the opcode
        }

        Registers.MPC = next;
    }

    private void CheckForTermination()
    {
        if (!MemoryLayout.IsInStack(Registers.SP))
        {
            DebugConsole.WriteLine("STACK OVERFLOW DETECTED");
            DebugConsole.WriteLine(
                $"SP = 0x{Registers.FormatRegister("SP", Registers.SP, _numberFormat)} is outside valid stack range " +
                $"(0x{MemoryLayout.StackSegment.Start:X4}–0x{MemoryLayout.StackSegment.End:X4})");

            _haltDetected = true;
            return;
        }

        if (Registers.MPC != (int)MicroInstructionCode.HALT)
        {
            return;
        }

        DebugConsole.WriteLine("HALT encountered – normal termination.");
        _haltDetected = true;
    }
    
    private void Execute()
    {
        CycleCount++;
        InstructionCycleCount++;

        var mi = DecodeNext();

        DebugConsole.WriteLine(
            $"Starting execution of {mi} {Registers.FormatRegister("MPC", Registers.MPC, _numberFormat)}, {Registers.FormatRegister("PC", Registers.PC, _numberFormat)}");

        CheckForTermination();

        var aluResult = ALU.Calculate(Registers, mi, CycleCount);

        if (mi.MemoryAfterRegisterWrite)
        {
            Registers.SetRegisterValue(mi, aluResult);
            Memory.Access(Registers, mi);
        }
        else
        {
            Memory.Access(Registers, mi);
            Registers.SetRegisterValue(mi, aluResult);
        }
        
        JumpToNext(mi);

        DebugConsole.WriteLine(
            $"Ending execution of {mi} {Registers.FormatRegister("MPC", Registers.MPC, _numberFormat)}, {Registers.FormatRegister("PC", Registers.PC, _numberFormat)}");

    }

    private MicroInstruction? DecodeNext()
    {
        var currentMicrocodeAddress = Registers.MPC;

        var microCodeKey = (MicroInstructionCode)currentMicrocodeAddress;

        if (!_microCodeControlStore.TryGetValue(microCodeKey, out var mi))
        {
            throw new InvalidOperationException(
                $"No microcode routine found for OPCODE: 0x{(int)microCodeKey:X2}.");
        }

        return mi;
    }

    private void InitializeMicrocode()
    {
        _microCodeControlStore = MicroCodeStoreBuilder.Build();
    }

    private async Task ProcessNextInstruction()
    {
        InstructionCycleCount = 0;

        var frameTimer = new Stopwatch();
        
        do
        {
            long frameCount = 0;
            long totalCycles = 0;

            frameTimer.Restart();

            var cyclesExecuted = 0;

            // Handle skipped frame catch-up (bounded)
            var framesToSimulate = 1;

            if (!_unThrottled)
            {
                var elapsed = frameTimer.Elapsed.TotalMilliseconds;
                framesToSimulate = Math.Max(1,
                    Math.Min(SimulatorConstants.MaxSkippedFrames, (int)(elapsed / _msPerFrame)));
            }

            for (var i = 0; i < framesToSimulate; i++)
            {
                var frameCycles = 0;

                while (frameCycles < _cyclesPerFrame && !_haltDetected)
                {
                    Execute();
                    frameCycles++;
                }

                cyclesExecuted += frameCycles;
            }

            totalCycles += cyclesExecuted;
            frameCount++;

            // Optional: display info once per frame
            if (frameCount % _refreshRate == 0)
            {
                DebugConsole.WriteLine(
                    $"[Frame {frameCount}] Total Cycles: {totalCycles}, Elapsed Time: {_totalTime.Elapsed.TotalSeconds:F2}s");
            }

            // Throttle to match real-time
            if (_unThrottled)
            {
                return;
            }

            var elapsedMs = frameTimer.Elapsed.TotalMilliseconds;
            var delay = _msPerFrame - elapsedMs;

            if (delay > 0)
            {
                await Task.Delay((int)delay);
            }
        } while (!_haltDetected && Registers.MPC != (int)MicroInstructionCode.FetchStartFetchDecode);

        UpdateIJVMCycles();
    }

    private void UpdateIJVMCycles()
    {
        IJVMCycleCount++;

        var opcode = (OpCode)Registers.MBR;

        PerOpcodeCycles.TryAdd(opcode, 0);

        PerOpcodeCycles[opcode] += InstructionCycleCount;
    }
}
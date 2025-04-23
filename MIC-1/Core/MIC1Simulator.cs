namespace MLDComputing.Emulators.MIC1.Core;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Bus;
using Constants;
using Display;
using Enums;
using MicroCode;

public class MIC1Simulator
{
    private readonly int _targetHz;

    private readonly int _refreshRate;

    private bool _haltDetected;

    private MicroInstruction[] _microCodeControlStore = [];

    public long[] PerOpcodeCycles = new long[SimulatorConstants.MicroCode.StoreSize];

    private int _cyclesPerFrame;

    private float _msPerFrame;

    public bool UnThrottled { get; }

    public LogOptions Options { get; }

    public long CycleCount { get; private set; }

    public long IJVMCycleCount { get; private set; }

    public long InstructionCycleCount { get; private set; }

    public NumberFormat Format { get; }
    
    public Memory Memory { get; set; }

    public Registers Registers { get; set; } = new();

    public Stopwatch TotalElapsedTime { get; } = new();

    public MIC1Simulator(LogOptions logOptions = LogOptions.All, int memorySize = SimulatorConstants.DefaultMemorySize,
        NumberFormat numberFormat = NumberFormat.Hex, int targetHz = SimulatorConstants.DefaultTargetFrequencyHz,
        int refreshRate = SimulatorConstants.DefaultRefreshRateHz, bool unThrottled = false)
    {
        Format = numberFormat;
        _targetHz = targetHz;
        _refreshRate = refreshRate;
        Options = logOptions;
        UnThrottled = unThrottled;
        Memory = new Memory(memorySize);
    }

    private void Init()
    {
        CycleCount = 0;
        InstructionCycleCount = 0;
        IJVMCycleCount = 0;
        _haltDetected = false;
        _cyclesPerFrame = _targetHz / _targetHz;
        _msPerFrame = SimulatorConstants.MillisecondsPerSecond / _refreshRate;

        DebugConsole.Verbose = Options.HasFlag(LogOptions.ConsoleMessages);

        InitializeMicrocode();

        TotalElapsedTime.Restart();
    }

    public async Task Run()
    {
        Init();

        // Set MicroProgramCounter to 0 to repeatedly run the Fetch/Decode cycle
        while (!_haltDetected)
        {
            Registers.MPC = (int)MicroInstructionCode.FETCHStartFetchDecode;

            await ProcessNextInstruction();
        }

        TotalElapsedTime.Stop();
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
        if (!MemoryLayout.IsInStack(Registers.SP))
        {
            DebugConsole.WriteLine("STACK OVERFLOW DETECTED");
            DebugConsole.WriteLine(
                $"SP = 0x{Registers.FormatRegister("SP", Registers.SP, Format)} is outside valid stack range " +
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
            $"Starting execution of {mi} {Registers.FormatRegister("MPC", Registers.MPC, Format)}, {Registers.FormatRegister("PC", Registers.PC, Format)}");

        CheckForTermination();

        var aluResult = ALU.Calculate(Registers, mi, CycleCount);

        if (mi.MemoryAfterRegisterWrite)
        {
            Registers.SetRegisterFromALU(aluResult, mi.C);
            Memory.Access(Registers, mi);
        }
        else
        {
            Memory.Access(Registers, mi);
            Registers.SetRegisterFromALU(aluResult, mi.C);
        }

        JumpToNext(mi);

        DebugConsole.WriteLine(
            $"Ending execution of {mi} {Registers.FormatRegister("MPC", Registers.MPC, Format)}, {Registers.FormatRegister("PC", Registers.PC, Format)}");
    }

    private MicroInstruction DecodeNext()
    {
        var mi = _microCodeControlStore[Registers.MPC];

        if (mi == null)
        {
            throw new InvalidOperationException(
                $"No microcode routine found at: 0x{Registers.MPC:X2}.");
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

            var cyclesExecuted = 0;

            var framesToSimulate = UnThrottled
                ? 1
                : Math.Max(1, Math.Min(SimulatorConstants.MaxSkippedFrames,
                    (int)(frameTimer.Elapsed.TotalMilliseconds / _msPerFrame)));

            if (framesToSimulate > 1)
            {
                int a = 1;
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

            if (frameCount % _refreshRate == 0)
            {
                DebugConsole.WriteLine($"[Frame {frameCount}] Total Cycles: {totalCycles}");
            }

            // Timing should come AFTER simulation
            var elapsedMs = frameTimer.Elapsed.TotalMilliseconds;
            var delay = _msPerFrame - elapsedMs;

            if (!UnThrottled && delay > 0)
            {
                Console.WriteLine(delay);
                await Task.Delay((int)delay);
            }

            frameTimer.Restart();

        } while (!_haltDetected && Registers.MPC != (int)MicroInstructionCode.FETCHStartFetchDecode);


        UpdateIJVMCycles();
    }

    private void UpdateIJVMCycles()
    {
        IJVMCycleCount++;

        if(Options.HasFlag(LogOptions.DetailedStats))
        {
            PerOpcodeCycles[Registers.CurrentOpcode] += InstructionCycleCount;
        }
    }
}
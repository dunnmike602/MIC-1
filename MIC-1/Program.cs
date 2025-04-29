namespace MLDComputing.Emulators.MIC1;

using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Core;
using Core.Constants;
using Core.Events;
using Core.IJVM;
using Ipc;

public class Program
{
#if WINDOWS
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentProcessorNumber();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadAffinityMask(IntPtr hThread, uint dwThreadAffinityMask);

#elif LINUX
    [DllImport("libc")]
    private static extern int sched_getcpu();
#endif


    public static async Task Main(string[] args)
    {
        try
        {
            await RunSimulator();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fatal Terminator of MIC1 Simulator. Message: {e.Message}");
        }
    }

    private static void WriteStatusLine(string label, string value, float speed)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"\r{label}: {value,-20}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($" Speed: {speed,10}");
        Console.ResetColor();
    }

    private static async Task RunSimulator()
    {
        using var mic1 = new MIC1Simulator(false, unThrottled: true, memoryChecking: false, showDetailedStats: false);

        //    HandleNormalEvents(mic1);

        //   HandleTraps(mic1);

        try
        {
            for (var i = 0; i < 2500; i++)
            {
                mic1.Init(true);

                LoadIFEQTest(mic1);
          
                await Task.Run(() => mic1.Run(pc: 1000));

                var cycleCount = mic1.CycleCount;
                var iJVMCycleCount = mic1.IJVMCycleCount;
                var elapsedTicks = mic1.TotalElapsedTime.ElapsedTicks;
                var elapsedSeconds = mic1.TotalElapsedTime.ElapsedMilliseconds * 1000;
             
                var total = (cycleCount /
                             ((float)elapsedTicks / Stopwatch.Frequency));

                Console.WriteLine($"SPEED=" + total / 1e6);
               // PrintStats(mic1, cycleCount, iJVMCycleCount, elapsedSeconds, elapsedTicks);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


        // TestCores(mic1);
    }

    private static void TestCores(MIC1Simulator mic1)
    {
        var logicalProcessorCount = Environment.ProcessorCount;
        Console.WriteLine($"Running on {logicalProcessorCount} logical processors.");

        for (var core = 0; core < logicalProcessorCount; core++)
        {
            Console.WriteLine($"\nRunning on Core {core}");

#if WINDOWS
            // Set affinity to one core (bitmask: 1 shifted by core number)
            var mask = (uint)(1 << core);
            SetThreadAffinityMask(GetCurrentThread(), mask);
#endif
            // Optional: Give the OS a moment to switch core (very minor delay)
            Thread.Sleep(50);

            // Run your simulator!
            mic1.Init(true);

            LoadIFEQTest(mic1);
            mic1.Run(pc: 1000);

            //PrintStats(mic1, (uint?)core);
        }
    }

    private static void HandleTraps(MIC1Simulator mic1)
    {
        var client = new LogPipeClient();
        var clientConnected = client.Connect();

        if (clientConnected)
        {
            mic1.Trap += (sender, args) =>
            {
                client.SendLog($"{args.TrapCode}: {args.Message}");
                client.SendLog(string.Empty);
                client.SendLog("Additional Information-------------------------------");
                client.SendLog($"{args.Information ?? "N/A"}");
                client.SendLog(string.Empty);
            };
        }
    }

    private static readonly Subject<ExecuteEventArgs> ExecutionSubject = new();

    private static void HandleNormalEvents(MIC1Simulator mic1)
    {
        ExecutionSubject
            .Buffer(TimeSpan.FromMilliseconds(1000)) // Batch over 1 second
            .Where(batch => batch.Any()) // Skip empty
            .Subscribe(batch =>
            {
                var processorSpeed = batch.Last().CycleCount /
                                     ((float)batch.Last().ElapsedTicks / Stopwatch.Frequency);
                WriteStatusLine("Started:", batch.Last().Instruction.Key.ToString(), processorSpeed / 1000);
            });

        // Hook the actual event to push to subject
        mic1.ExecutionStarted += (sender, args) => ExecutionSubject.OnNext(args);
    }

    private static void LoadIFEQTest(MIC1Simulator mic1)
    {
        // @formatter:off
        mic1.Memory.LoadProgram(1000,
            (byte)OpCode.SETSP,     // 1000 - Set the stack pointer
            0xFF,                   // 1001 - high byte
            0xFF,                   // 1002 - low byte

            (byte)OpCode.BIPUSH,    // 1003 - Push 100
            120,                    // 1004

            // Label: LoopStart
            (byte)OpCode.DUP,       // 1005 - duplicate top of stack

            (byte)OpCode.IFEQ,      // 1006 - if TOS == 0, jump to HALT (0x1010)
            0x00,                   // 1007 - high byte
            0x07,                   // 1008 - low byte (+9)

            (byte)OpCode.BIPUSH,    // 1009 - Push -1
            0xFF,                   // 1010 - two's complement of -1
            // If your BIPUSH sign-extends correctly, this is -1

            (byte)OpCode.IADD,      // 1011 - counter + (-1)

            (byte)OpCode.GOTO,      // 1012 - jump to LoopStart (0x1005)
            0xFF,                   // 1013 - high byte of offset (-9)
            0xF7,                   // 1014 - low byte of offset

            (byte)OpCode.HALT       // 1015 - done
        );
        // @formatter:on
    }

    private static void LoadStackTest(MIC1Simulator mic1)
    {
        // @formatter:off
        mic1.Memory.LoadProgram(1000,
            (byte)OpCode.SETSP,   // 0xD000 - Set the stack pointer
            0xFF,                 // 0xD001 - high byte
            0xFF,                 // 0xD002 - low byte

            (byte)OpCode.BIPUSH,  // 0xD003 - push 42 onto stack
            42,                   // 0xD004 - value to push

            (byte)OpCode.DUP,     // 0xD005 - duplicate top of stack

            (byte)OpCode.HALT     // 0xD006 - stop
        );
        // @formatter:on
    }

    private static void PrintStats(MIC1Simulator mic1, long cycleCount, long iJVMCycleCount, long totalSeconds, long elapsedTicks, uint? core = null)
    {
        core ??= GetCurrentCore();

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("-------------------------Simulation Report-------------------------");
        Console.WriteLine("{0,-30} {1:N0}", "Running on Core:", core);
        Console.WriteLine("{0,-30} {1:N0}", "Total Microcode Cycles:", cycleCount);
        Console.WriteLine("{0,-30} {1:N0}", "Total IJVM Cycles:", iJVMCycleCount);
        Console.WriteLine("{0,-30} {1:F3} sec", "Total elapsed time:", totalSeconds);

        var effectiveHz = cycleCount /
                          ((float)elapsedTicks / Stopwatch.Frequency);

        Console.WriteLine("{0,-30} {1:N0} ({2:F3} MHz)", "Effective μInstr/sec:", effectiveHz,
            effectiveHz / 1e6);
        Console.WriteLine("{0,-30} {1:N0} MHz", "Target speed MHZ:", SimulatorConstants.DefaultTargetFrequencyHz / 1e6);

        Console.WriteLine();

        Console.WriteLine("{0,-30} {1}", "Instr", "Cycles");

        for (var i = 0; i <= (int)OpCode.HALT; i++) // Replace `Last` with your max enum value
        {
            var opcode = (OpCode)i;
            if (mic1.PerOpcodeCycles[i] != 0)
            {
                Console.WriteLine("{0,-30} {1}", opcode, mic1.PerOpcodeCycles[i]);
            }
        }

        Console.WriteLine("-------------------------------------------------------------------");
    }

 
#if WINDOWS
        private static uint GetCurrentCore()
        {
            var core = GetCurrentProcessorNumber();

            return core;
        }

#elif LINUX
       public static uint GetCurrentCore()
    {
        int core = sched_getcpu();
        if (core == -1)
        {
            throw new InvalidOperationException("Failed to get CPU core number.");
        }
        return (uint)core;
    }
#endif



}
namespace MLDComputing.Emulators.MIC1;

using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Controller;
using Core;
using Core.Bus;
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


    public static void Main(string[] args)
    {
        try
        {
            RunSimulator();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fatal Terminator of MIC1 Simulator. Message: {e.Message}");
        }
    }


    private static void WriteStatusLine(string label, string? value, float speed)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"\r{label}: {value,-20}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($" Speed: {speed,10}");
        Console.ResetColor();
    }

    private static void RunSimulator()
    {
        using var emulatorCts = new CancellationTokenSource();
        using var mic1 = new MIC1Simulator(unThrottled: true,
            memoryChecking: false, showDetailedStats: false, enableExecutionEvents: false);

        // take a snapshot of the token so we don’t capture emulatorCts
        var token = emulatorCts.Token;

        // take a snapshot of the emulator so we don’t capture the field/outer var
        var runner = mic1;

        try
        {
            LoadStackTest();

            // the lambda now only closes over `runner` and `token`
            var micTask = Task.Run(() => runner.Run(token, pc: 1000), token);

            var app = new Mic1Controller(mic1);
            app.Run();

            emulatorCts.Cancel();
            micTask.Wait(emulatorCts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static void HandleTraps(MIC1Simulator mic1)
    {
        var client = new LogPipeClient();
        var clientConnected = client.Connect();

        if (clientConnected)
        {
            mic1.TrapEvent += (sender, args) =>
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
                WriteStatusLine("Started:", batch.Last().Instruction?.Key.ToString(), processorSpeed / 1000000);
            });

        // Hook the actual event to push to subject
        mic1.ExecutionEvent += (sender, args) => ExecutionSubject.OnNext(args);
    }

    private static void LoadISUBTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.SETSP,
            0xFF,              
            0xFF,
            (byte)OpCode.BIPUSH,
            15,
            (byte)OpCode.BIPUSH,
            8,
            (byte)OpCode.ISUB,
            (byte)OpCode.HALT
        );
        // @formatter:on
    }

    private static void LoadNOPTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.NOP,    // 1000 - DO NOTHING
            (byte)OpCode.NOP,    // 1001 - DO NOTHING
            (byte)OpCode.HALT    // 1002 - done
        );
        // @formatter:on
    }

    private static void LoadShiftTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.BIPUSH, 0x01,        // 1000 - shift count (1 bit)
            (byte)OpCode.BIPUSH, 0x0F,        // 1002 - value 15
            (byte)OpCode.ISHL,               // 1004 - 15 << 1 = 30

          (byte)OpCode.BIPUSH, 0x01,        // 1005 - shift count (1 bit)
         (byte)OpCode.BIPUSH, 0xF0,        // 1007 - value -16 (0xF0 = -16)
       (byte)OpCode.ISHR,               // 1009 - -16 >> 1 = -8 (signed)

            (byte)OpCode.BIPUSH, 0x01,        // 1010 - shift count (1 bit)
            (byte)OpCode.BIPUSH, 0xF0,        // 1012 - value -16 (again)
            (byte)OpCode.IUSHR,              // 1014 - 0xFFFFFFF0 >>> 1 = 0x7FFFFFF8

            (byte)OpCode.HALT                 // 1015 - done
        );
        // @formatter:on
    }

    private static void LoadBIPUSHTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.BIPUSH,    // 1000 - Push 100
            120,                    // 1001
            (byte)OpCode.HALT       // 1002 - done
        );
        // @formatter:on
    }

    private static void LoadAddTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.SETSP,     // 1000 - Set the stack pointer
            0xFF,                   // 1001 - high byte
            0xFF,                   // 1002 - low byte

            (byte)OpCode.BIPUSH,    // 1003 - Push 100
            120,                    // 1004

            (byte)OpCode.BIPUSH,    // 1005 - Push -1
            0x5,                    // 1006 
            (byte)OpCode.IADD,
            (byte)OpCode.GOTO,      // Add and push back
            0xFF, // high byte
            0xF6, // low byte 
            (byte)OpCode.HALT       // 1008 - done
        );
    }

    private static void LoadTrapTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            1   
        );
    }
    
    private static void LoadIFEQTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
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

    private static void LoadANDTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.BIPUSH,    // Push 00001111 (binary)   
            0x0F,
            (byte)OpCode.BIPUSH,
            0xF1,                   // Push 00001111 (binary)   

            (byte)OpCode.IAND,

            (byte)OpCode.HALT       // 1015 - done
        );
        // @formatter:on
    }

    private static void LoadORTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.BIPUSH,    // Push 00001111 (binary)   
            0xF0,
            (byte)OpCode.BIPUSH,
            0x0F,                   // Push 00001111 (binary)   

            (byte)OpCode.IOR,

            (byte)OpCode.HALT       // 1015 - done
        );
        // @formatter:on
    }

    private static void LoadXORTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.BIPUSH,    // Push 00001111 (binary)   
            0b111,
            (byte)OpCode.BIPUSH,
            0b111,                   // Push 00001111 (binary)   

            (byte)OpCode.IXOR,

            (byte)OpCode.HALT       // 1015 - done
        );
        // @formatter:on
    }

    private static void LoadIFNETest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
              (byte)OpCode.SETSP,     // 1000 - Set the stack pointer
              0xFF,                   // 1001 - high byte
              0xFB,                   // 1002 - low byte, need a dummy value for FETCH

              (byte)OpCode.BIPUSH,     // 1003 - Push 120
              120,

              // Label: LoopStart
              (byte)OpCode.DUP,        // 1005 - duplicate top of stack

              (byte)OpCode.BIPUSH,     // 1006 - Push -1
              0xFF,                    // 1007 - two's complement of -1

              (byte)OpCode.IADD,       // 1008 - counter + (-1)

              (byte)OpCode.DUP,        // 1009 - duplicate new top of stack (after decrement)

              (byte)OpCode.IFNE,       // 1010 - if TOS != 0, jump back to LoopStart (0x1005)
              0xFF,                    // 1011 - high byte of offset (-11)
              0xF5,                    // 1012 - low byte of offset

              (byte)OpCode.HALT        // 1013 - done
          );
        // @formatter:on
    }

    private static void LoadSwapTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.SETSP,   // 0xD000 - Set the stack pointer
            0xFF,                 // 0xD001 - high byte
            0xFF,                 // 0xD002 - low byte

            (byte)OpCode.BIPUSH,  // 0xD003 - push 42 onto stack
            42,                   // 0xD004 - value to push
            
            (byte)OpCode.BIPUSH,  // 0xD003 - push 42 onto stack
            24,                   // 0xD004 - value to push
            
            (byte)OpCode.SWAP,     // 0xD005 - duplicate top of stack
            (byte)OpCode.HALT     // 0xD006 - stop
        );
        // @formatter:on
    }
    
    private static void LoadStackTest()
    {
        // @formatter:off
        Memory.LoadBootProgram(1000,
            (byte)OpCode.SETSP,   // 0xD000 - Set the stack pointer
            0xFF,                 // 0xD001 - high byte
            0xBB,                 // 0xD002 - low byte

            (byte)OpCode.BIPUSH,  // 0xD003 - push 42 onto stack
            42,                   // 0xD004 - value to push

            (byte)OpCode.DUP,     // 0xD005 - duplicate top of stack
            (byte)OpCode.POP,     // 0xD005 - duplicate top of stack
            (byte)OpCode.HALT     // 0xD006 - stop
        );
        // @formatter:on
    }

    private static void PrintStats(MIC1Simulator mic1, long cycleCount, long iJVMCycleCount, float totalSeconds,
        long elapsedTicks, uint? core = null)
    {
        core ??= GetCurrentCore();

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("-------------------------Simulation Report-------------------------");
        Console.WriteLine("{0,-30} {1:N0}", "Running on Core:", core);
        Console.WriteLine("{0,-30} {1:N0}", "Total Microcode Cycles:", cycleCount);
        Console.WriteLine("{0,-30} {1:N0}", "Total IJVM Cycles:", iJVMCycleCount);
        Console.WriteLine("{0,-30} {1:F6} sec", "Total elapsed time:", totalSeconds);

        var effectiveHz = cycleCount /
                          ((float)elapsedTicks / Stopwatch.Frequency);

        Console.WriteLine("{0,-30} {1:N0} ({2:F3} MHz)", "Effective μInstr/sec:", effectiveHz,
            effectiveHz / 1e6);
        Console.WriteLine("{0,-30} {1:N0} MHz", "Target speed MHZ:",
            SimulatorConstants.Machine.DefaultTargetFrequencyHz / 1e6);

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
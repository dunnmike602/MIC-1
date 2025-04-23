namespace MLDComputing.Emulators.MIC1;

using Core;
using Core.Constants;
using Core.Enums;
using Core.IJVM;

public class Program
{
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

    private static async Task RunSimulator()
    {
        var mic1 = new MIC1Simulator(LogOptions.DetailedStats,  numberFormat: NumberFormat.Hex,
            unThrottled:false)
        {
            Registers =
            {
                PC = 1000
            }
        };

        LoadIFEQTest(mic1);

        try
        {
         //   for (int i = 0; i < 100; i++)
            {
                mic1.Registers.PC = 1000;
                await mic1.Run();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        PrintStackTop(mic1);
        PrintStats(mic1);
    }

    private static void LoadIFEQTest(MIC1Simulator mic1)
    {
        // @formatter:off
        mic1.Memory.LoadProgram(1000,
            (byte)OpCode.SETSP,     // 1000 - Set the stack pointer
            0xFF,                   // 1001 - high byte
            0xFF,                   // 1002 - low byte

            (byte)OpCode.BIPUSH,    // 1003 - Push 100
            2,                    // 1004

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

    private static void PrintStackTop(MIC1Simulator mic1, int count = 2)
    {
        Console.WriteLine("Stack Top:");
        var sp = mic1.Registers.SP;

        for (var i = mic1.Registers.SP + 8; i >= mic1.Registers.SP; i--)
        {
            // Read full word (4 bytes)
            var value = mic1.Memory.ReadByte(i);
            Console.WriteLine($"[0x{i:X4}] = {value}");
        }
    }

    private static void PrintStats(MIC1Simulator mic1)
    {
        Console.WriteLine("-------------------------Simulation Report-------------------------");
        Console.WriteLine("{0,-30} {1:N0}", "Total Microcode Cycles:", mic1.CycleCount);
        Console.WriteLine("{0,-30} {1:N0}", "Total IJVM Cycles:", mic1.IJVMCycleCount);
        Console.WriteLine("{0,-30} {1:F3} sec", "Total elapsed time:", mic1.TotalElapsedTime.Elapsed.TotalSeconds);
        var effectiveHz = mic1.CycleCount / mic1.TotalElapsedTime.Elapsed.TotalSeconds;
        Console.WriteLine("{0,-30} {1:N0} ({2:F3} MHz)", "Effective μInstr/sec:", effectiveHz,
            effectiveHz / 1_000_000.0);
        Console.WriteLine("{0,-30} {1:N0} Hz", "Target speed:", SimulatorConstants.DefaultTargetFrequencyHz);

        Console.WriteLine();

        Console.WriteLine("{0,-30} {1}", "Instr", "Cycles");

        foreach (OpCode opcode in Enum.GetValues(typeof(OpCode)))
        {
            var value = (int)opcode; // Or use (byte)opcode if you want the byte value
            if (mic1.PerOpcodeCycles[value] != 0)
            {
                Console.WriteLine("{0,-30} {1}", opcode, mic1.PerOpcodeCycles[value]);
            }
        }

        Console.WriteLine("-------------------------------------------------------------------");
    }
}
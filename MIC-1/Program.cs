using MLDComputing.Emulators.MIC1.Core;
using MLDComputing.Emulators.MIC1.Core.IJVM;
using MLDComputing.Emulators.MIC1.Core.MicroCode;
using MLDComputing.Emulators.MIC1.Display;

namespace MLDComputing.Emulators.MIC1;

using Core.Enums;

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
        var mic1 = new MIC1Simulator(verboseLogging: false, numberFormat: NumberFormat.Hex)
        {
            Registers =
            {
                PC = 1000
            }
        };

        LoadStackTest(mic1);

        await mic1.Run();

        PrintStackTop(mic1);
        PrintStats(mic1);
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
        Console.WriteLine("---------------Processor Stats---------------");
        Console.WriteLine($"Total MicroCode Processor Cycles={mic1.CycleCount}");
        Console.WriteLine($"Total IJVM Processor Cycles={mic1.IJVMCycleCount}");
        Console.WriteLine();

        foreach (var cycle in mic1.PerOpcodeCycles)
        {
            Console.WriteLine($"{cycle.Key}={cycle.Value} Cycles");
        }

        Console.WriteLine("-------------END Processor Stats-------------");
    }
}
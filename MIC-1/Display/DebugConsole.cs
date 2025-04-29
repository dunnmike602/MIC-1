
namespace MLDComputing.Emulators.MIC1.Display;

using System;
using System.Runtime.CompilerServices;
using Core.Bus;
using Core.Enums;
using Core.Extensions;
using Core.MicroCode;

public static class DebugConsole
{
    public static bool Verbose { get; set; } = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteLine(string message)
    {
        if (Verbose)
        {
            Console.WriteLine(message);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(string message)
    {
        if (Verbose)
        {
            Console.Write(message);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ShowExecuteMessage(string message, MicroInstruction mi, Registers registers, NumberFormat format, long cycle)
    {
        if (Verbose)
        {
            Console.WriteLine($"Cycle={cycle}: {message}{mi} {"MPC".FormatRegister(registers.MPC, format)}, {"PC".FormatRegister(registers.PC, format)}");
        }
    }
}
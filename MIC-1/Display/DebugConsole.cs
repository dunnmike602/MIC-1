
namespace MLDComputing.Emulators.MIC1.Display;

using System;

public static class DebugConsole
{
    public static bool Verbose { get; set; } = true;

    public static void WriteLine(string message)
    {
        if (Verbose)
        {
            Console.WriteLine(message);
        }
    }

    public static void Write(string message)
    {
        if (Verbose)
        {
            Console.Write(message);
        }
    }

    public static void WriteLine() => WriteLine(string.Empty);
}
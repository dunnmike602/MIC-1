namespace MLDComputing.Emulators.MIC1.Core.Enums;

[Flags]
public enum LogOptions
{
    None = 0,
    ConsoleMessages = 1 << 0,
    DetailedStats = 1 << 1, 
    All = ConsoleMessages | DetailedStats
}
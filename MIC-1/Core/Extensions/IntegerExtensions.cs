namespace MLDComputing.Emulators.MIC1.Core.Extensions;

using Bus;

public static class IntegerExtensions
{
    public static bool IsBitSet(this int source, RegisterLoadSignal signal)
    {
        var mask = 1 << (int)signal;
        return (source & mask) != 0;
    }
}
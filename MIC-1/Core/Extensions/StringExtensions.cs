namespace MLDComputing.Emulators.MIC1.Core.Extensions;

using Enums;

public static class StringExtensions
{
    public static string FormatRegister(this string registerName, int value, NumberFormat format)
    {
        var returnValue = format switch
        {
            NumberFormat.Decimal => $"{registerName} = {value}",
            NumberFormat.Hex => $"{registerName} = 0x{value:X}",
            NumberFormat.Binary => $"{registerName} = 0b{Convert.ToString(value, 2).PadLeft(32, '0')}",
            NumberFormat.Octal => $"{registerName} = 0o{Convert.ToString(value, 8)}",
            _ => $"{registerName} = {value}"
        };

        return returnValue;
    }
}
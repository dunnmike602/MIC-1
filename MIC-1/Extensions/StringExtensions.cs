namespace MLDComputing.Emulators.MIC1.Extensions;

using Core.Enums;

public static class StringExtensions
{
    public static bool IsInteger(this string? input)
    {
        return int.TryParse(input, out _);
    }
    
    public static string FormatRegister(this string registerName, int value, NumberFormat format)
    {

        var returnValue = format switch
        {
            NumberFormat.Hex => $"{registerName} = 0x{value:X}",
            NumberFormat.Binary => $"{registerName} = 0b{Convert.ToString(value, 2).PadLeft(32, '0')}",
            NumberFormat.Octal => $"{registerName} = 0o{Convert.ToString(value, 8)}",
            _ => $"{registerName} = {value}"
        };

        return returnValue;
    }

    public static string FormatValue(this int value, NumberFormat format)
    {
        var returnValue = format switch
        {
            NumberFormat.Hex => $"0x{value:X}",
            NumberFormat.Binary => $"0b{Convert.ToString(value, 2).PadLeft(16, '0')}",
            NumberFormat.Octal => $"0o{Convert.ToString(value, 8)}",
            _ => $"{value}"
        };

        return returnValue;
    }

    public static string FormatValue(this byte value, NumberFormat format)
    {
        return ((int)value).FormatValue(format);
    }
}
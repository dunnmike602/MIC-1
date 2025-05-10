namespace MLDComputing.Emulators.MIC1.Extensions;

public static class ByteExtensions
{
    public static char ToPrintableChar(this byte value)
    {
        // ASCII printable characters range from 32 (space) to 126 (~)
        return value >= 32 && value <= 126 ? (char)value : '-';
    }
}
namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Runtime.CompilerServices;
using MicroCode;

public static class ALU
{
    public static bool IsZero { get; private set; }
    public static bool IsNegative { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Calculate(Registers registers, MicroInstruction mi)
    {
        var a = registers.H;

        // Fast path for extremely common ops that don’t require both inputs
        switch (mi.ALU)
        {
            case ALUOperation.PassA:
                SetFlags(a);
                return a;

            case ALUOperation.Zero:
                SetFlags(0);
                return 0;

            case ALUOperation.One:
                SetFlags(1);
                return 1;

            case ALUOperation.Nop:
                SetFlags(0);
                return 0;

            case ALUOperation.SignExtend8:
                int extended = (sbyte)(GetBInput(registers, mi.B) & 0xFF);
                SetFlags(extended);
                return extended;
        }

        // Load B only when needed
        var b = GetBInput(registers, mi.B);
        int result;

        switch (mi.ALU)
        {
            case ALUOperation.PassB:
                result = b;
                break;

            case ALUOperation.APlusB:
                result = a + b;
                break;

            case ALUOperation.AMinusB:
                result = a - b;
                break;

            case ALUOperation.BMinusA:
                result = b - a;
                break;

            case ALUOperation.IncrementA:
                result = a + 1;
                break;

            case ALUOperation.IncrementB:
                result = b + 1;
                break;

            case ALUOperation.DecrementA:
                result = a - 1;
                break;

            case ALUOperation.DecrementB:
                result = b - 1;
                break;

            case ALUOperation.NegateA:
                result = ~a;
                break;

            case ALUOperation.NegateB:
                result = ~b;
                break;

            case ALUOperation.IncrementBBy4:
                result = b + 4;
                break;

            case ALUOperation.DecrementBBy4:
                result = b - 4;
                break;

            case ALUOperation.CombineOffset:
                var offset = (short)(((a & 0xFF) << 8) | (b & 0xFF));
                result = registers.PC + offset;
                break;

            case ALUOperation.CombineHighLow:
                result = (ushort)(((a & 0xFF) << 8) | (b & 0xFF));
                break;

            case ALUOperation.IncrementBBy2:
                result = b + 2;
                break;

            case ALUOperation.DecrementBBy2:
                result = b - 2;
                break;

            case ALUOperation.IncrementBBy3:
                result = b + 3;
                break;

            default:
                result = 0;
                break;
        }

        SetFlags(result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetFlags(int value)
    {
        IsZero = value == 0;
        IsNegative = (value & 0x80000000) != 0;
    }

    public static void PrintCombinedOffset(int highByte, int lowByte)
    {
        // Combine high and low into a 16-bit signed offset
        var signedOffset = (short)((highByte << 8) | (lowByte & 0xFF));
        int extended = signedOffset; // Implicit sign-extension

        Console.WriteLine("🔍 CombineOffset Debug:");
        Console.WriteLine($"  High Byte (H): 0x{highByte:X2}  ({Convert.ToString(highByte, 2).PadLeft(8, '0')})");
        Console.WriteLine($"  Low Byte (MDR): 0x{lowByte:X2}  ({Convert.ToString(lowByte, 2).PadLeft(8, '0')})");
        Console.WriteLine(
            $"  Combined (16-bit): 0x{(ushort)signedOffset:X4}  ({Convert.ToString((ushort)signedOffset, 2).PadLeft(16, '0')})");
        Console.WriteLine($"  Sign-Extended (32-bit int): {extended}  (0x{extended:X8})");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBInput(Registers registers, RegisterSelectSignal selector)
    {
        return selector switch
        {
            RegisterSelectSignal.PC => registers.PC,
            RegisterSelectSignal.SP => registers.SP,
            RegisterSelectSignal.MAR => registers.MAR,
            RegisterSelectSignal.MBR => registers.MBR,
            RegisterSelectSignal.MDR => registers.MDR,
            RegisterSelectSignal.H => registers.H,
            RegisterSelectSignal.LV => registers.LV,
            RegisterSelectSignal.CPP => registers.CPP,
            RegisterSelectSignal.TOS => registers.TOS,
            RegisterSelectSignal.OPC => registers.OPC,
            _ => 0
        };
    }
}
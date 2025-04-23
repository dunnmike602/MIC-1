namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Runtime.CompilerServices;
using Display;
using MicroCode;

public static class ALU
{
    public static bool IsZero { get; private set; }
    public static bool IsNegative { get; private set; }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Calculate(Registers registers, MicroInstruction mi, long cycleCount)
    {
        var a = registers.H;
        var b = GetBInput(registers, mi.B);
        var result = 0;

        switch (mi.ALU)
        {
            case ALUOperation.Nop:
                break;

            case ALUOperation.PassA:
                result = a;
                break;

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

            case ALUOperation.Zero:
                result = 0;
                break;

            case ALUOperation.One:
                result = 1;
                break;

            case ALUOperation.IncrementBBy4:
                result = b + 4;
                break;

            case ALUOperation.DecrementBBy4:
                result = b - 4;
                break;

            case ALUOperation.CombineOffset:
            {
                var offset = (short)(((a & 0xFF) << 8) | (b & 0xFF));
                result = registers.PC + offset; // Sign-extended to 32-bit
                break;
            }
            case ALUOperation.CombineHighLow:
            {
                var offset = (ushort)(((a & 0xFF) << 8) | (b & 0xFF));
                result = offset;
                break;
            }
            case ALUOperation.SignExtend8:
                // Interpret the lowest 8 bits as a signed byte, then auto-extend to 32-bit int
                result = (sbyte)(b & 0xFF);
                break;

            case ALUOperation.IncrementBBy2:
                result = b + 2;
                break;
            
            case ALUOperation.IncrementBBy3:
                result = b + 3;
                break;

            case ALUOperation.DecrementBBy2:
                result = b - 2;
                break;
        }
      
        IsZero = result == 0;
        IsNegative = (result & 0x80000000) != 0;

        return result;
    }

    public static void PrintCombinedOffset(int highByte, int lowByte)
    {
        // Combine high and low into a 16-bit signed offset
        var signedOffset = (short)((highByte << 8) | (lowByte & 0xFF));
        int extended = signedOffset; // Implicit sign-extension

        DebugConsole.WriteLine("🔍 CombineOffset Debug:");
        DebugConsole.WriteLine($"  High Byte (H): 0x{highByte:X2}  ({Convert.ToString(highByte, 2).PadLeft(8, '0')})");
        DebugConsole.WriteLine($"  Low Byte (MDR): 0x{lowByte:X2}  ({Convert.ToString(lowByte, 2).PadLeft(8, '0')})");
        DebugConsole.WriteLine(
            $"  Combined (16-bit): 0x{(ushort)signedOffset:X4}  ({Convert.ToString((ushort)signedOffset, 2).PadLeft(16, '0')})");
        DebugConsole.WriteLine($"  Sign-Extended (32-bit int): {extended}  (0x{extended:X8})");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBInput(Registers registers, RegisterSelectSignal selector)
    {
        return selector switch
        {
            RegisterSelectSignal.MDR => registers.MDR,
            RegisterSelectSignal.PC => registers.PC,
            RegisterSelectSignal.SP => registers.SP,
            RegisterSelectSignal.LV => registers.LV,
            RegisterSelectSignal.CPP => registers.CPP,
            RegisterSelectSignal.TOS => registers.TOS,
            RegisterSelectSignal.OPC => registers.OPC,
            RegisterSelectSignal.MAR => registers.MAR,
            RegisterSelectSignal.MBR => registers.MBR,
            RegisterSelectSignal.H => registers.H,
            _ => 0
        };
    }
}
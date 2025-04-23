using System.Runtime.CompilerServices;
using MLDComputing.Emulators.MIC1.Core.Enums;
using MLDComputing.Emulators.MIC1.Display;

namespace MLDComputing.Emulators.MIC1.Core.Bus;

public class Registers
{
    // Constructor to initialize the registers to a default state

    // 	Constant Pool Pointer (32bit)  Points to the runtime constant pool (e.g., for method/field info)
    public int CPP { get; set; }

    // 	Holding Register (32bit) Used to store intermediate values for ALU operations
    public int H { get; set; }

    // 	Local Variables Pointer (32bit)  Points to the start of local variables for the current frame
    public int LV { get; set; }

    // Memory Address Register (32bit): Holds the address for memory operations
    public int MAR { get; set; }

    // Memory Buffer Register (8bit): Holds data to/from memory. buffer for the current instruction
    public byte MBR { get; set; } = 0;

    // Data Register (32bit): Holds data being read from or written to memory
    public int MDR { get; set; }

    // Micro Program Counter (32bit) Program Counter for MicroCode
    public int MPC { get; set; }

    // Old Program Counter (32bit): Used to store return address on method call
    public int OPC { get; set; }

    // Program Counter (32bit): Holds the address of the current IJVM instruction
    public int PC { get; set; }

    public int SP { get; set; } = MemoryLayout.StackSegment.End;

    // 	Top of Stack Cache (32bit)  Caches the top value of the stack for faster access
    public int TOS { get; set; }

    // Tracks the current OpCode, the MIC-1 architecture allows for 256 microcde slots so this will fit in a byte
    public byte CurrentOpcode { get; set; }

    // For debugging purposes, print all registers
    public void PrintRegisters()
    {
        DebugConsole.WriteLine("---- CPU Registers ----");
        DebugConsole.WriteLine($"H   : 0x{H:X8}");
        DebugConsole.WriteLine($"SP  : 0x{SP:X8}");
        DebugConsole.WriteLine($"LV  : 0x{LV:X8}");
        DebugConsole.WriteLine($"CPP : 0x{CPP:X8}");
        DebugConsole.WriteLine($"TOS : 0x{TOS:X8}");
        DebugConsole.WriteLine($"PC  : 0x{PC:X8}");
        DebugConsole.WriteLine($"OPC : 0x{OPC:X8}");
        DebugConsole.WriteLine($"MPC : 0x{MPC:X8}");
        DebugConsole.WriteLine($"MAR : 0x{MAR:X8}");
        DebugConsole.WriteLine($"MDR : 0x{MDR:X2}");
        DebugConsole.WriteLine($"MBR : 0x{MBR:X2}");
        DebugConsole.WriteLine("------------------------");
    }

    public string FormatRegister(string registerName, int value, NumberFormat format)
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRegisterFromALU(int aluResult, int cBitmask)
    {
        switch (cBitmask)
        {
            case 1 << (int)RegisterLoadSignal.H:
                H = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.MDR:
                MDR = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.MAR:
                MAR = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.PC:
                PC = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.SP:
                SP = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.LV:
                LV = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.CPP:
                CPP = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.TOS:
                TOS = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.OPC:
                OPC = aluResult;
                break;
            case 1 << (int)RegisterLoadSignal.MPC:
                MPC = aluResult;
                break;
        }
    }

}
namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Runtime.CompilerServices;

public class Registers
{
    // Constructor to initialize the registers to a default state

    // 	Constant Pool Pointer (32bit)  Points to the runtime constant pool (e.g., for method/field info)
    public int CPP;

    // 	Holding Register (32bit) Used to store intermediate values for ALU operations
    public int H;

    // 	Local Variables Pointer (32bit)  Points to the start of local variables for the current frame
    public int LV;

    // Memory Address Register (32bit): Holds the address for memory operations
    public int MAR;

    // Memory Buffer Register (8bit): Holds data to/from memory. buffer for the current instruction
    public byte MBR = 0;

    // Data Register (32bit): Holds data being read from or written to memory
    public int MDR;

    // Micro Program Counter (32bit) Program Counter for MicroCode
    public int MPC;

    // Old Program Counter (32bit): Used to store return address on method call
    public int OPC;

    // Program Counter (32bit): Holds the address of the current IJVM instruction
    public int PC;

    public int SP = MemoryLayout.StackSegment.End;

    // 	Top of Stack Cache (32bit)  Caches the top value of the stack for faster access
    public int TOS;

    // Tracks the current OpCode, the MIC-1 architecture allows for 256 microcode slots so this will fit in a byte
    public byte CurrentOpcode;

    // For debugging purposes, print all registers
    public void PrintRegisters()
    {
        Console.WriteLine("---- CPU Registers ----");
        Console.WriteLine($"H   : 0x{H:X8}");
        Console.WriteLine($"SP  : 0x{SP:X8}");
        Console.WriteLine($"LV  : 0x{LV:X8}");
        Console.WriteLine($"CPP : 0x{CPP:X8}");
        Console.WriteLine($"TOS : 0x{TOS:X8}");
        Console.WriteLine($"PC  : 0x{PC:X8}");
        Console.WriteLine($"OPC : 0x{OPC:X8}");
        Console.WriteLine($"MPC : 0x{MPC:X8}");
        Console.WriteLine($"MAR : 0x{MAR:X8}");
        Console.WriteLine($"MDR : 0x{MDR:X2}");
        Console.WriteLine($"MBR : 0x{MBR:X2}");
        Console.WriteLine("------------------------");
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRegisterFromALU(int aluResult, int cBitmask)
    {
        if (cBitmask == 1 << (int)RegisterLoadSignal.H)
        {
            H = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.MDR)
        {
            MDR = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.MAR)
        {
            MAR = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.PC)
        {
            PC = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.SP)
        {
            SP = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.LV)
        {
            LV = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.CPP)
        {
            CPP = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.TOS)
        {
            TOS = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.OPC)
        {
            OPC = aluResult;
            return;
        }

        if (cBitmask == 1 << (int)RegisterLoadSignal.MPC)
        {
            MPC = aluResult;
        }
    }
}
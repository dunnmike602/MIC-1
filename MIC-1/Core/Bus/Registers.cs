namespace MLDComputing.Emulators.MIC1.Core.Bus;

using System.Runtime.CompilerServices;

public static class Registers
{
    // 	Constant Pool Pointer (32bit)  Points to the runtime constant pool (e.g., for method/field info)
    public static int CPP;

    // 	Holding Register (32bit) Used to store intermediate values for ALU operations
    public static int H;

    // 	Local Variables Pointer (32bit)  Points to the start of local variables for the current frame
    public static int LV;

    // Memory Address Register (32bit): Holds the address for memory operations
    public static int MAR;

    // Memory Buffer Register (8bit): Holds data to/from memory. buffer for the current instruction
    public static byte MBR = 0;

    // Data Register (32bit): Holds data being read from or written to memory
    public static int MDR;

    // Micro Program Counter (32bit) Program Counter for MicroCode
    public static int MPC;

    // Old Program Counter (32bit): Used to store return address on method call
    public static int OPC;

    // Program Counter (32bit): Holds the address of the current IJVM instruction
    public static int PC;

    public static int SP;

    // 	Top of Stack Cache (32bit)  Caches the top value of the stack for faster access
    public static int TOS;

    // This is convenience register to track the actual location of the stack in memory as it can be moved
    // Allows the stackdump utility to function correctly
    public static int StackStart;
    
    // Tracks the current OpCode, the MIC-1 architecture allows for 256 microcode slots so this will fit in a byte
    public static byte CurrentOpcode;

    public static string GetRegisterSnapshot()
    {
        return $"CPP={CPP}, H={H}, LV={LV}, MAR={MAR}, MBR={MBR}, MDR={MDR}, MPC={MPC}, OPC={OPC}, PC={PC}";
    }
    
    public static void Reset()
    {
        CPP = 0;
        H = 0;
        LV = 0;
        MAR = 0;
        MBR = 0;
        MDR = 0;
        MPC = 0;
        OPC = 0;
        PC = 0;
        SP = MemoryLayout.StackSegment.Top;
        StackStart = MemoryLayout.StackSegment.Top;
        TOS = 0;
        CurrentOpcode = 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetRegisterFromALU(int aluResult, int cBitmask)
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
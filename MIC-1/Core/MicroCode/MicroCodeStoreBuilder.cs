namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

using Bus;
using Constants;
using IJVM;

public static class MicroCodeStoreBuilder
{
    public static MicroInstruction[] Build()
    {
        var microCodeControlArray = new MicroInstruction[SimulatorConstants.MicroCode.StoreSize];

        microCodeControlArray[(int)MicroInstructionCode.FETCHStartFetchDecode] = new MicroInstruction
        {
            Key = MicroInstructionCode.FETCHStartFetchDecode,
            Address = (int)MicroInstructionCode.FETCHLoadMAR,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.FETCH)
        };

        microCodeControlArray[(int)MicroInstructionCode.FETCHLoadMAR] = new MicroInstruction
        {
            Key = MicroInstructionCode.FETCHLoadMAR,
            Address = (int)MicroInstructionCode.FETCHReadInstruction,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.PassA,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.FETCH)
        };

        microCodeControlArray[(int)MicroInstructionCode.FETCHReadInstruction] = new MicroInstruction
        {
            Key = MicroInstructionCode.FETCHReadInstruction,
            Address = (int)MicroInstructionCode.FETCHIncPC,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadOperandToToMBR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.FETCH)
        };

        microCodeControlArray[(int)MicroInstructionCode.FETCHIncPC] = new MicroInstruction
        {
            Key = MicroInstructionCode.FETCHIncPC,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            MEM = MemoryOperation.NoOp,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            JAM = (byte)JAMControl.JAMC,
            OpCode = nameof(OpCode.FETCH)
        };

        microCodeControlArray[(int)MicroInstructionCode.HALT] = new MicroInstruction
        {
            Key = MicroInstructionCode.HALT,
            Address = (int)MicroInstructionCode.HALT,
            JAM = (byte)JAMControl.None,
            ALU = ALUOperation.Nop,
            B = RegisterSelectSignal.None,
            C = 0,
            MEM = MemoryOperation.NoOp,
            OpCode = nameof(OpCode.HALT)
        };

        microCodeControlArray[(int)MicroInstructionCode.BIPUSHCopyPCToMar] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHCopyPCToMar,
            Address = (int)MicroInstructionCode.BIPUSHReadOperand,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        microCodeControlArray[(int)MicroInstructionCode.BIPUSHReadOperand] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHReadOperand,
            Address = (int)MicroInstructionCode.BIPUSHSignExtendAndHold,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.ReadByteToMBR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        microCodeControlArray[(int)MicroInstructionCode.BIPUSHSignExtendAndHold] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHSignExtendAndHold,
            Address = (int)MicroInstructionCode.BIPUSHDecrementSP,
            B = RegisterSelectSignal.MBR,
            ALU = ALUOperation.SignExtend8,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        microCodeControlArray[(int)MicroInstructionCode.BIPUSHDecrementSP] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHDecrementSP,
            Address = (int)MicroInstructionCode.BIPUSHCopySPToMAR,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.DecrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        microCodeControlArray[(int)MicroInstructionCode.BIPUSHCopySPToMAR] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHCopySPToMAR,
            Address = (int)MicroInstructionCode.BIPUSHWriteToStack,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        microCodeControlArray[(int)MicroInstructionCode.BIPUSHWriteToStack] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHWriteToStack,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            B = RegisterSelectSignal.H,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MDR,
            MEM = MemoryOperation.WriteWordFromMDR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        microCodeControlArray[(int)MicroInstructionCode.DUPLoadTopAddress] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPLoadTopAddress,
            Address = (int)MicroInstructionCode.DUPReadTop,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        microCodeControlArray[(int)MicroInstructionCode.DUPReadTop] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPReadTop,
            Address = (int)MicroInstructionCode.DUPDecSP,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        microCodeControlArray[(int)MicroInstructionCode.DUPDecSP] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPDecSP,
            Address = (int)MicroInstructionCode.DUPAddressForCopy,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.DecrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        microCodeControlArray[(int)MicroInstructionCode.DUPAddressForCopy] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPAddressForCopy,
            Address = (int)MicroInstructionCode.DUPWriteCopy,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        microCodeControlArray[(int)MicroInstructionCode.DUPWriteCopy] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPWriteCopy,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.WriteWordFromMDR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        microCodeControlArray[(int)MicroInstructionCode.IADDLoadFirst] = new MicroInstruction
        {
            Key = MicroInstructionCode.IADDLoadFirst,
            Address = (int)MicroInstructionCode.IADDReadFirst,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IADD)
        };

        microCodeControlArray[(int)MicroInstructionCode.IADDReadFirst] = new MicroInstruction
        {
            Key = MicroInstructionCode.IADDReadFirst,
            Address = (int)MicroInstructionCode.IADDLoadSecond,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.IncrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.ReadWordToMDR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IADD)
        };

        microCodeControlArray[(int)MicroInstructionCode.IADDLoadSecond] = new MicroInstruction
        {
            Key = MicroInstructionCode.IADDLoadSecond,
            Address = (int)MicroInstructionCode.IADDReadSecond,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IADD)
        };

        microCodeControlArray[(int)MicroInstructionCode.IADDReadSecond] = new MicroInstruction
        {
            Key = MicroInstructionCode.IADDReadSecond,
            Address = (int)MicroInstructionCode.IADDComputeResult,
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.ReadWordToMDR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IADD)
        };

        microCodeControlArray[(int)MicroInstructionCode.IADDComputeResult] = new MicroInstruction
        {
            Key = MicroInstructionCode.IADDComputeResult,
            Address = (int)MicroInstructionCode.IADDDecSP,
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.APlusB,
            C = 1 << (int)RegisterLoadSignal.MDR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IADD)
        };

        microCodeControlArray[(int)MicroInstructionCode.IADDDecSP] = new MicroInstruction
        {
            Key = MicroInstructionCode.IADDDecSP,
            Address = (int)MicroInstructionCode.IADDSetResultAddress,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.DecrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IADD)
        };

        microCodeControlArray[(int)MicroInstructionCode.IADDSetResultAddress] = new MicroInstruction
        {
            Key = MicroInstructionCode.IADDSetResultAddress,
            Address = (int)MicroInstructionCode.IADDStoreResult,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IADD)
        };

        microCodeControlArray[(int)MicroInstructionCode.IADDStoreResult] = new MicroInstruction
        {
            Key = MicroInstructionCode.IADDStoreResult,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.WriteWordFromMDR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IADD)
        };

        microCodeControlArray[(int)MicroInstructionCode.GOTOInit] = new MicroInstruction
        {
            Key = MicroInstructionCode.GOTOInit,
            Address = (int)MicroInstructionCode.JUMPLoadMARHigh,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.NoOp,
            JAM = 0,
            OpCode = nameof(OpCode.GOTO)
        };

        microCodeControlArray[(int)MicroInstructionCode.SETSPLoadHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.SETSPLoadHigh,
            Address = (int)MicroInstructionCode.SETSPReadHigh,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        microCodeControlArray[(int)MicroInstructionCode.SETSPReadHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.SETSPReadHigh,
            Address = (int)MicroInstructionCode.SETSPStoreHigh,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadByteToMBR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        microCodeControlArray[(int)MicroInstructionCode.SETSPStoreHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.SETSPStoreHigh,
            Address = (int)MicroInstructionCode.SETSPIncPCBeforeLow,
            B = RegisterSelectSignal.MBR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        microCodeControlArray[(int)MicroInstructionCode.SETSPIncPCBeforeLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.SETSPIncPCBeforeLow,
            Address = (int)MicroInstructionCode.SETSPLoadLow,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        microCodeControlArray[(int)MicroInstructionCode.SETSPLoadLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.SETSPLoadLow,
            Address = (int)MicroInstructionCode.SETSPReadLow,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        microCodeControlArray[(int)MicroInstructionCode.SETSPReadLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.SETSPReadLow,
            Address = (int)MicroInstructionCode.SETSPCombineAndStore,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadByteToMBR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        microCodeControlArray[(int)MicroInstructionCode.SETSPCombineAndStore] = new MicroInstruction
        {
            Key = MicroInstructionCode.SETSPCombineAndStore,
            Address = (int)MicroInstructionCode.SETSPIncrementPC,
            B = RegisterSelectSignal.MBR,
            ALU = ALUOperation.CombineHighLow,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        microCodeControlArray[(int)MicroInstructionCode.SETSPIncrementPC] = new MicroInstruction
        {
            Key = MicroInstructionCode.SETSPIncrementPC,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        // Step 1: MAR ← SP
        microCodeControlArray[(int)MicroInstructionCode.IFEQLoadTop] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQLoadTop,
            Address = (int)MicroInstructionCode.IFEQReadTopAndIncSP,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.SETSP)
        };

        // Step 2: MDR ← M[MAR]; SP ← SP + 4
        microCodeControlArray[(int)MicroInstructionCode.IFEQReadTopAndIncSP] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQReadTopAndIncSP,
            Address = (int)MicroInstructionCode.IFEQStoreTopInH,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.IncrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.ReadWordToMDR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 3: H ← MDR
        microCodeControlArray[(int)MicroInstructionCode.IFEQStoreTopInH] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQStoreTopInH,
            Address = (int)MicroInstructionCode.IFEQIncPCForLow,
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = 0,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 3: PC ← PC + 1 to get to low byte of offset
        microCodeControlArray[(int)MicroInstructionCode.IFEQIncPCForLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQIncPCForLow,
            Address = (int)MicroInstructionCode.IFEQLoadLow,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 4: MAR ← PC (low byte)
        microCodeControlArray[(int)MicroInstructionCode.IFEQLoadLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQLoadLow,
            Address = (int)MicroInstructionCode.IFEQReadLow,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };


        // Step 5: MBR ← M[MAR]
        microCodeControlArray[(int)MicroInstructionCode.IFEQReadLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQReadLow,
            Address = (int)MicroInstructionCode.IFEQCheckZero,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadByteToMBR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 6: Conditional jump based on H == 0
        microCodeControlArray[(int)MicroInstructionCode.IFEQCheckZero] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQCheckZero,
            Address = (int)MicroInstructionCode.IFEQSkipOperandsAndReturn,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.PassA,
            C = 0,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.JAMZ, // JAMZ: Use zero flag to possibly override MPC bits
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 7: Default no Jump skip over the last operand and carry on
        microCodeControlArray[(int)MicroInstructionCode.IFEQSkipOperandsAndReturn] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQSkipOperandsAndReturn,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        AddJumpInstructions(microCodeControlArray);

        LoadJumpTable(microCodeControlArray);

        return microCodeControlArray;
    }

    private static void LoadJumpTable(MicroInstruction[] microCodeControlArray)
    {
        for (var i = (int)MicroInstructionCode.JUMPTableStart; i <= (int)MicroInstructionCode.JUMPTableEnd; i++)
        {
            microCodeControlArray[i] = microCodeControlArray[(int)MicroInstructionCode.JUMPDecrementPC];
        }
    }

    private static void AddJumpInstructions(MicroInstruction[] microCodeControlArray)
    {
        microCodeControlArray[(int)MicroInstructionCode.JUMPDecrementPC] = new MicroInstruction
        {
            Key = MicroInstructionCode.JUMPDecrementPC,
            Address = (int)MicroInstructionCode.JUMPLoadMARHigh,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.DecrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = "BRANCH"
        };

        microCodeControlArray[(int)MicroInstructionCode.JUMPLoadMARHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.JUMPLoadMARHigh,
            Address = (int)MicroInstructionCode.JUMPStoreHigh,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.ReadByteToMBR, // Fetch HH
            JAM = (byte)JAMControl.None,
            OpCode = "BRANCH"
        };

        microCodeControlArray[(int)MicroInstructionCode.JUMPStoreHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.JUMPStoreHigh,
            Address = (int)MicroInstructionCode.JUMPIncrementPCForLow,
            B = RegisterSelectSignal.MBR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = "BRANCH"
        };

        microCodeControlArray[(int)MicroInstructionCode.JUMPIncrementPCForLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.JUMPIncrementPCForLow,
            Address = (int)MicroInstructionCode.JUMPReadLow,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = "BRANCH"
        };

        microCodeControlArray[(int)MicroInstructionCode.JUMPReadLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.JUMPReadLow,
            Address = (int)MicroInstructionCode.JUMPCombineOffset,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.ReadByteToMBR, 
            JAM = (byte)JAMControl.None,
            OpCode = "BRANCH"
        };

        microCodeControlArray[(int)MicroInstructionCode.JUMPCombineOffset] = new MicroInstruction
        {
            Key = MicroInstructionCode.JUMPCombineOffset,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            B = RegisterSelectSignal.MBR,
            ALU = ALUOperation.CombineOffset,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = "BRANCH"
        };
    }
}
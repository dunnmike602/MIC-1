namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

using System.Drawing;
using Bus;
using Constants;
using IJVM;

public static class MicroCodeStoreBuilder
{
    public static MicroInstruction[] Build()
    {
        var microCodeControlArray = CreateInitialisedArray();

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
            MEM = MemoryOperation.ReadByteToMBR,
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

        // Step 1: Copy PC to MAR
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

        // Step 2: Read byte into MBR and increment PC
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

        // Step 3: Sign-extend MBR and store in H
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

        // Step 4: Decrement SP by 4
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

        // Step 5: Copy SP to MAR
        microCodeControlArray[(int)MicroInstructionCode.BIPUSHCopySPToMAR] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHCopySPToMAR,
            Address = (int)MicroInstructionCode.BIPUSHLoadHToMDR,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        // Step 6: Copy H into MDR
        microCodeControlArray[(int)MicroInstructionCode.BIPUSHLoadHToMDR] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHLoadHToMDR,
            Address = (int)MicroInstructionCode.BIPUSHWriteHigh,
            B = RegisterSelectSignal.H,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MDR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        // Step 7: Write MDR high 16 bits to memory
        microCodeControlArray[(int)MicroInstructionCode.BIPUSHWriteHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHWriteHigh,
            Address = (int)MicroInstructionCode.BIPUSHAdd2ToMAR,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.WriteWordFromMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        // Step 8: Increment MAR by 2
        microCodeControlArray[(int)MicroInstructionCode.BIPUSHAdd2ToMAR] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHAdd2ToMAR,
            Address = (int)MicroInstructionCode.BIPUSHWriteLow,
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        // Step 10: Write MDR low 16 bits to memory
        microCodeControlArray[(int)MicroInstructionCode.BIPUSHWriteLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHWriteLow,
            Address = (int)MicroInstructionCode.BIPUSHCacheTOS,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.WriteWordFromMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };

        // Step 14: TOS ← MDR
        microCodeControlArray[(int)MicroInstructionCode.BIPUSHCacheTOS] = new MicroInstruction
        {
            Key = MicroInstructionCode.BIPUSHCacheTOS,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.TOS,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.BIPUSH)
        };


        // Step 1: Load SP into MAR
        microCodeControlArray[(int)MicroInstructionCode.DUPLoadTopAddress] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPLoadTopAddress,
            Address = (int)MicroInstructionCode.DUPReadTopHigh,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        // Step 2: Read high 16 bits of top stack word into MDR
        microCodeControlArray[(int)MicroInstructionCode.DUPReadTopHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPReadTopHigh,
            Address = (int)MicroInstructionCode.DUPAdd2ToMAR,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        // Step 3: Increment MAR by 2
        microCodeControlArray[(int)MicroInstructionCode.DUPAdd2ToMAR] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPAdd2ToMAR,
            Address = (int)MicroInstructionCode.DUPReadTopLow,
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        // Step 4: Read low 16 bits of top stack word into MDR
        microCodeControlArray[(int)MicroInstructionCode.DUPReadTopLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPReadTopLow,
            Address = (int)MicroInstructionCode.DUPDecSP,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        // Step 5: Decrement SP by 4
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

        // Step 6: Load new SP into MAR
        microCodeControlArray[(int)MicroInstructionCode.DUPAddressForCopy] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPAddressForCopy,
            Address = (int)MicroInstructionCode.DUPWriteCopyHigh,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        // Step 7: Write high 16 bits from MDR into memory
        microCodeControlArray[(int)MicroInstructionCode.DUPWriteCopyHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPWriteCopyHigh,
            Address = (int)MicroInstructionCode.DUPAdd2ToMARCopy,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.WriteWordFromMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        // Step 8: Increment MAR by 2
        microCodeControlArray[(int)MicroInstructionCode.DUPAdd2ToMARCopy] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPAdd2ToMARCopy,
            Address = (int)MicroInstructionCode.DUPWriteCopyLow,
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        // Step 9: Write low 16 bits from MDR into memory
        microCodeControlArray[(int)MicroInstructionCode.DUPWriteCopyLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPWriteCopyLow,
            Address = (int)MicroInstructionCode.DUPCacheTOS, // Done, go fetch next instruction
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.WriteWordFromMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        // Step 14: TOS ← MDR
        microCodeControlArray[(int)MicroInstructionCode.DUPCacheTOS] = new MicroInstruction
        {
            Key = MicroInstructionCode.DUPCacheTOS,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode,
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.TOS,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.DUP)
        };

        RegisterBinaryStackOpSequence(microCodeControlArray, MicroInstructionCode.IADDLoadFirst,
            MicroInstructionCode.IADDLoadNext, nameof(OpCode.IADD),
            ALUOperation.APlusB);

        RegisterBinaryStackOpSequence(microCodeControlArray, MicroInstructionCode.ISUBLoadFirst,
            MicroInstructionCode.ISUBLoadNext, nameof(OpCode.ISUB),
            ALUOperation.BMinusA);

        RegisterBinaryStackOpSequence(microCodeControlArray, MicroInstructionCode.IANDLoadFirst,
            MicroInstructionCode.IANDLoadNext, nameof(OpCode.IAND),
            ALUOperation.AAndB);


        RegisterBinaryStackOpSequence(microCodeControlArray, MicroInstructionCode.IORLoadFirst,
            MicroInstructionCode.IORLoadNext, nameof(OpCode.IOR),
            ALUOperation.AOrB);
        
        RegisterBinaryStackOpSequence(microCodeControlArray, MicroInstructionCode.IXORLoadFirst,
            MicroInstructionCode.IXORLoadNext, nameof(OpCode.IXOR),
            ALUOperation.AXorB);
        
        microCodeControlArray[(int)MicroInstructionCode.GOTOInit] = new MicroInstruction
        {
            Key = MicroInstructionCode.GOTOInit,
            Address = (int)MicroInstructionCode.JUMPLoadMARHigh,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
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
            Address = (int)MicroInstructionCode.IFEQReadTopHigh,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 2: Read high 16 bits
        microCodeControlArray[(int)MicroInstructionCode.IFEQReadTopHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQReadTopHigh,
            Address = (int)MicroInstructionCode.IFEQAdd2ToMAR,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 3: MAR += 2
        microCodeControlArray[(int)MicroInstructionCode.IFEQAdd2ToMAR] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQAdd2ToMAR,
            Address = (int)MicroInstructionCode.IFEQReadTopLow,
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 4: Read low 16 bits
        microCodeControlArray[(int)MicroInstructionCode.IFEQReadTopLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQReadTopLow,
            Address = (int)MicroInstructionCode.IFEQStoreTopInH,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 5: H ← MDR
        microCodeControlArray[(int)MicroInstructionCode.IFEQStoreTopInH] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQStoreTopInH,
            Address = (int)MicroInstructionCode.IFEQIncrementSP,
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 6: SP ← SP + 4
        microCodeControlArray[(int)MicroInstructionCode.IFEQIncrementSP] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQIncrementSP,
            Address = (int)MicroInstructionCode.IFEQIncPCForLow,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.IncrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 7: PC ← PC + 1 (fetch offset)
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

        // Step 8: MAR ← PC
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

        // Step 9: MBR ← M[MAR]
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

        // Step 10: Conditional jump if H == 0
        microCodeControlArray[(int)MicroInstructionCode.IFEQCheckZero] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQCheckZero,
            Address = (int)MicroInstructionCode.IFEQSkipOperandsAndReturn,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.PassA,
            C = 0,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.JAMZ_EQ,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 11: Default PC += 1 if not jumping
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

        // Step 1: MAR ← SP
        microCodeControlArray[(int)MicroInstructionCode.IFEQLoadTop] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQLoadTop,
            Address = (int)MicroInstructionCode.IFEQReadTopHigh,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 2: Read high 16 bits
        microCodeControlArray[(int)MicroInstructionCode.IFEQReadTopHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQReadTopHigh,
            Address = (int)MicroInstructionCode.IFEQAdd2ToMAR,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 3: MAR += 2
        microCodeControlArray[(int)MicroInstructionCode.IFEQAdd2ToMAR] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQAdd2ToMAR,
            Address = (int)MicroInstructionCode.IFEQReadTopLow,
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 4: Read low 16 bits
        microCodeControlArray[(int)MicroInstructionCode.IFEQReadTopLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQReadTopLow,
            Address = (int)MicroInstructionCode.IFEQStoreTopInH,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 5: H ← MDR
        microCodeControlArray[(int)MicroInstructionCode.IFEQStoreTopInH] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQStoreTopInH,
            Address = (int)MicroInstructionCode.IFEQIncrementSP,
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 6: SP ← SP + 4
        microCodeControlArray[(int)MicroInstructionCode.IFEQIncrementSP] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQIncrementSP,
            Address = (int)MicroInstructionCode.IFEQIncPCForLow,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.IncrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 7: PC ← PC + 1 (fetch offset)
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

        // Step 8: MAR ← PC
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

        // Step 9: MBR ← M[MAR]
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

        // Step 10: Conditional jump if H == 0
        microCodeControlArray[(int)MicroInstructionCode.IFEQCheckZero] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQCheckZero,
            Address = (int)MicroInstructionCode.IFEQSkipOperandsAndReturn,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.PassA,
            C = 0,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.JAMZ_EQ,
            OpCode = nameof(OpCode.IFEQ)
        };

        // Step 11: Default PC += 1 if not jumping
        microCodeControlArray[(int)MicroInstructionCode.IFEQSkipOperandsAndReturn] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFEQSkipOperandsAndReturn,
            Address = ((int)MicroInstructionCode.IFEQSkipOperandsAndReturn) +1, // Jumping to a routine that loads the TOS here
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFEQ)
        };

        GenerateReloadTOS(microCodeControlArray, ((int)MicroInstructionCode.IFEQSkipOperandsAndReturn) + 1,
            nameof(OpCode.IFEQ));

        // Step 1: MAR ← SP
        microCodeControlArray[(int)MicroInstructionCode.IFNELoadTop] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNELoadTop,
            Address = (int)MicroInstructionCode.IFNEReadTopHigh,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 2: Read high 16 bits
        microCodeControlArray[(int)MicroInstructionCode.IFNEReadTopHigh] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNEReadTopHigh,
            Address = (int)MicroInstructionCode.IFNEAdd2ToMAR,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 3: MAR += 2
        microCodeControlArray[(int)MicroInstructionCode.IFNEAdd2ToMAR] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNEAdd2ToMAR,
            Address = (int)MicroInstructionCode.IFNEReadTopLow,
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 4: Read low 16 bits
        microCodeControlArray[(int)MicroInstructionCode.IFNEReadTopLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNEReadTopLow,
            Address = (int)MicroInstructionCode.IFNEStoreTopInH,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 5: H ← MDR
        microCodeControlArray[(int)MicroInstructionCode.IFNEStoreTopInH] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNEStoreTopInH,
            Address = (int)MicroInstructionCode.IFNEIncrementSP,
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 6: SP ← SP + 4
        microCodeControlArray[(int)MicroInstructionCode.IFNEIncrementSP] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNEIncrementSP,
            Address = (int)MicroInstructionCode.IFNEIncPCForLow,
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.IncrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 7: PC ← PC + 1 (fetch offset)
        microCodeControlArray[(int)MicroInstructionCode.IFNEIncPCForLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNEIncPCForLow,
            Address = (int)MicroInstructionCode.IFNELoadLow,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 8: MAR ← PC
        microCodeControlArray[(int)MicroInstructionCode.IFNELoadLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNELoadLow,
            Address = (int)MicroInstructionCode.IFNEReadLow,
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 9: MBR ← M[MAR]
        microCodeControlArray[(int)MicroInstructionCode.IFNEReadLow] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNEReadLow,
            Address = (int)MicroInstructionCode.IFNECheckZero,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadByteToMBR,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 10: Conditional jump if H == 0
        microCodeControlArray[(int)MicroInstructionCode.IFNECheckZero] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNECheckZero,
            Address = (int)MicroInstructionCode.IFNESkipOperandsAndReturn,
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.PassA,
            C = 0,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.JAMZ_NE,
            OpCode = nameof(OpCode.IFNE)
        };

        // Step 11: Default PC += 1 if not jumping
        microCodeControlArray[(int)MicroInstructionCode.IFNESkipOperandsAndReturn] = new MicroInstruction
        {
            Key = MicroInstructionCode.IFNESkipOperandsAndReturn,
            Address = ((int)MicroInstructionCode.IFNESkipOperandsAndReturn) +1, // Jumping to a routine that loads the TOS here
            B = RegisterSelectSignal.PC,
            ALU = ALUOperation.IncrementB,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = nameof(OpCode.IFNE)
        };

        GenerateReloadTOS(microCodeControlArray, ((int)MicroInstructionCode.IFNESkipOperandsAndReturn) + 1,
            nameof(OpCode.IFNE));

        AddJumpInstructions(microCodeControlArray);

        LoadJumpTable(microCodeControlArray);

        return microCodeControlArray;
    }

    private static MicroInstruction[] CreateInitialisedArray()
    {
        var microCodeControlArray = new MicroInstruction[SimulatorConstants.MicroCode.StoreSize];

        for (var i = 0; i < SimulatorConstants.MicroCode.StoreSize; i++)
        {
            microCodeControlArray[i] = new MicroInstruction(); 
        }

        return microCodeControlArray;
    }


    private static void LoadJumpTable(MicroInstruction[] microCodeControlArray)
    {
        for (var i = (int)MicroInstructionCode.JUMPTableStart; i <= (int)MicroInstructionCode.JUMPTableEnd; i++)
        {
            microCodeControlArray[i] = microCodeControlArray[(int)MicroInstructionCode.JUMPDecrementPC];
        }
    }

    private static void RegisterBinaryStackOpSequence(
        MicroInstruction[] microCodeControlArray,
        MicroInstructionCode startIndex,
        MicroInstructionCode nextIndex,
        string opcodeName,
        ALUOperation aluCompute
    )
    {
        // Second instruction is in a different place in the code store
        var step = (int)nextIndex;

        microCodeControlArray[(int)startIndex] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(startIndex),
            Name = $"{opcodeName}CopyTOSToH",
            B = RegisterSelectSignal.TOS,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.H,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}PopTOS",
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.IncrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}SetMARToSP",
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}ReadHighWordA",
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };


        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}IncMARForLowWordA",
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}ReadLowWordA",
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}PopSecondOperand",
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.IncrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };
        
        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}ComputeResult",
            B = RegisterSelectSignal.MDR,
            ALU = aluCompute,
            C = 1 << (int)RegisterLoadSignal.MDR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}DecrementSPForPush",
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.DecrementBBy4,
            C = 1 << (int)RegisterLoadSignal.SP,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}SetMARForResultWrite",
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}WriteHighResult",
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.WriteWordFromMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}IncMARForLowResult",
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}WriteLowResult",
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.WriteWordFromMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Key = (MicroInstructionCode)(step - 1),
            Name = $"{opcodeName}UpdateTOSWithResult",
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.TOS,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opcodeName,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode
        };
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
            Address = ((int)MicroInstructionCode.JUMPCombineOffset) + 1,
            B = RegisterSelectSignal.MBR,
            ALU = ALUOperation.CombineOffset,
            C = 1 << (int)RegisterLoadSignal.PC,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = "BRANCH"
        };

        GenerateReloadTOS(microCodeControlArray, ((int)MicroInstructionCode.JUMPCombineOffset) + 1,
            "BRANCH");
    }

    public static void GenerateReloadTOS(MicroInstruction[] microCodeControlArray, int startIndex, string opCodeName)
    {
        var step = startIndex;

        microCodeControlArray[step++] = new MicroInstruction
        {
            Name = $"{opCodeName}ReloadTOS_SetMAR",
            Key = (MicroInstructionCode)(step - 1),
            B = RegisterSelectSignal.SP,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opCodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Name = $"{opCodeName}ReloadTOS_ReadHigh",
            Key = (MicroInstructionCode)(step - 1),
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRHigh,
            JAM = (byte)JAMControl.None,
            OpCode = opCodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Name = $"{opCodeName}ReloadTOS_IncMAR",
            Key = (MicroInstructionCode)(step - 1),
            B = RegisterSelectSignal.MAR,
            ALU = ALUOperation.IncrementBBy2,
            C = 1 << (int)RegisterLoadSignal.MAR,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opCodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Name = $"{opCodeName}ReloadTOS_ReadLow",
            Key = (MicroInstructionCode)(step - 1),
            B = RegisterSelectSignal.None,
            ALU = ALUOperation.Nop,
            C = 0,
            MEM = MemoryOperation.ReadWordToMDRLow,
            JAM = (byte)JAMControl.None,
            OpCode = opCodeName,
            Address = step
        };

        microCodeControlArray[step++] = new MicroInstruction
        {
            Name = $"{opCodeName}ReloadTOS_SetTOS",
            Key = (MicroInstructionCode)(step - 1),
            B = RegisterSelectSignal.MDR,
            ALU = ALUOperation.PassB,
            C = 1 << (int)RegisterLoadSignal.TOS,
            MEM = MemoryOperation.NoOp,
            JAM = (byte)JAMControl.None,
            OpCode = opCodeName,
            Address = (int)MicroInstructionCode.FETCHStartFetchDecode
        };
    }
}
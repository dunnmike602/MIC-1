namespace MLDComputing.Emulators.MIC1.Core.MicroCode;

using Bus;
using Constants;
using IJVM;
using Memory;

public static class MicroCodeStoreBuilder
{
    public static MicroInstruction[] Build()
    {
        var microCodeControlArray = new MicroInstruction[SimulatorConstants.MicroCodeStoreSize];

        microCodeControlArray[(int)MicroInstructionCode.FetchStartFetchDecode] = new MicroInstruction
        {
            Address = (int)MicroInstructionCode.FetchLoadMAR, // next MicroInstruction
            B = RegisterSelectSignal.PC, // select PC as source
            ALU = ALUOperation.PassB, // ALU passes B input (PC)
            C = 1 << (int)RegisterLoadSignal.H, // store result in H
            MEM = MemoryOperation.NoOp,
            JAM = 0,
            OpCode = OpCode.FETCH
        };

        var microCodeControlStore = new Dictionary<MicroInstructionCode, MicroInstruction>
        {

            {
                MicroInstructionCode.FetchLoadMAR,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.FetchReadInstruction,
                    B = RegisterSelectSignal.None,
                    ALU = ALUOperation.PassA, // ALU passes A input (H)
                    C = 1 << (int)RegisterLoadSignal.MAR, // store result in MAR
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.FETCH
                }
            },
            {
                MicroInstructionCode.FetchReadInstruction,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.FetchIncPC,
                    B = RegisterSelectSignal.None,
                    ALU = ALUOperation.Nop, // Still passing from H (which holds PC)
                    C = 0,
                    MEM = MemoryOperation.ReadByteToMBR, // Trigger memory read: MBR ← M[MAR] instruction loaded into 8 bit register
                    JAM = 0,
                    OpCode = OpCode.FETCH
                }
            },
            {
                MicroInstructionCode.FetchIncPC,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.FetchStartFetchDecode, // Will be overridden
                    MEM = MemoryOperation.NoOp,
                    B = RegisterSelectSignal.PC, // put PC on the B‑bus
                    ALU = ALUOperation.IncrementB, // ALU does  B + 1
                    C = 1 << (int)RegisterLoadSignal.PC, // latch result back into PC
                    JAM = 1,
                    OpCode = OpCode.FETCH
                }
            },
            // Pseudo Op HALT
            {
                MicroInstructionCode.HALT,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode
                        .HALT, // Stay at HALT forever (or loop to 0x000 if you prefer)
                    JAM = 0,
                    ALU = ALUOperation.Nop,
                    B = RegisterSelectSignal.None,
                    C = 0,
                    MEM = MemoryOperation.NoOp,
                    OpCode = OpCode.HALT
                }
            },
            {
                // MAR->PC
                MicroInstructionCode.BIPUSHCopyPCToMar,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.BIPUSHReadOperand,
                    B = RegisterSelectSignal.PC,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.BIPUSH
                }
            },
            {
                MicroInstructionCode.BIPUSHReadOperand,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.BIPUSHSignExtendAndHold, // next step
                    B = RegisterSelectSignal.PC, // select PC for increment
                    ALU = ALUOperation.IncrementB, // PC + 1
                    C = 1 << (int)RegisterLoadSignal.PC, // write back new PC
                    MEM = MemoryOperation.ReadByteToMBR, // read operand byte → MBR
                    JAM = 0,
                    OpCode = OpCode.BIPUSH
                }
            },
            {
                MicroInstructionCode.BIPUSHSignExtendAndHold, 
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.BIPUSHDecrementSP, // Next step: reserve space on stack
                    B = RegisterSelectSignal.MBR, // Read operand byte from MBR
                    ALU = ALUOperation.SignExtend8, // Sign-extend 8-bit → 32-bit
                    C = 1 << (int)RegisterLoadSignal.H, // Store result in H
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.BIPUSH
                }
            },
            {
                MicroInstructionCode.BIPUSHDecrementSP,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.BIPUSHCopySPToMAR, // next step
                    B = RegisterSelectSignal.SP, // put SP on B-bus
                    ALU = ALUOperation.DecrementBBy4, // compute SP - 1
                    C = 1 << (int)RegisterLoadSignal.SP, // write result back to SP
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.BIPUSH
                }
            },
            {
                MicroInstructionCode.BIPUSHCopySPToMAR,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.BIPUSHWriteToStack, // next step
                    B = RegisterSelectSignal.SP, // select SP
                    ALU = ALUOperation.PassB, // ALU just passes SP through
                    C = 1 << (int)RegisterLoadSignal.MAR, // load result into MAR
                    MEM = MemoryOperation.NoOp,
                    JAM = 0
                }
            },
            {
                MicroInstructionCode.BIPUSHWriteToStack,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.FetchStartFetchDecode, // return to fetch
                    B = RegisterSelectSignal.H,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MDR,
                    MEM = MemoryOperation.WriteWordFromMDR,
                    JAM = 0,
                    OpCode = OpCode.BIPUSH
                }
            },
            // 0x40: MAR ← SP
            {
                MicroInstructionCode.DUPLoadTopAddress, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.DUPReadTop,
                    B = RegisterSelectSignal.SP,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.DUP
                }
            },

            // 0x41: MBR ← M[MAR]
            {
                MicroInstructionCode.DUPReadTop, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.DUPDecSP,
                    B = 0,
                    ALU = ALUOperation.Nop,
                    C = 0,
                    MEM = MemoryOperation.ReadWordToMDR,
                    JAM = 0,
                    OpCode = OpCode.DUP
                }
            },

            // 0x42: SP ← SP - 1
            {
                MicroInstructionCode.DUPDecSP, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.DUPAddressForCopy,
                    B = RegisterSelectSignal.SP,
                    ALU = ALUOperation.DecrementBBy4,
                    C = 1 << (int)RegisterLoadSignal.SP,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.DUP
                }
            },

            // 0x43: MAR ← SP
            {
                MicroInstructionCode.DUPAddressForCopy, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.DUPWriteCopy,
                    B = RegisterSelectSignal.SP,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.DUP
                }
            },

            // 0x44: M[MAR] ← MBR
            {
                MicroInstructionCode.DUPWriteCopy, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.FetchStartFetchDecode, // return to fetch
                    B = 0,
                    ALU = ALUOperation.Nop,
                    C = 0,
                    MEM = MemoryOperation.WriteWordFromMDR,
                    JAM = 0,
                    OpCode = OpCode.DUP
                }
            },
            // 0x50: MAR ← SP
            {
                MicroInstructionCode.IADDLoadFirst, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.IADDReadFirst,
                    B = RegisterSelectSignal.SP,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.IADD
                }
            },

            // 0x51: MDR ← M[MAR]; SP ← SP + 4
            {
                MicroInstructionCode.IADDReadFirst, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.IADDLoadSecond,
                    B = RegisterSelectSignal.SP,
                    ALU = ALUOperation.IncrementBBy4,
                    C = 1 << (int)RegisterLoadSignal.SP,
                    MEM = MemoryOperation.ReadWordToMDR, // MDR ← M[MAR]
                    JAM = 0,
                    OpCode = OpCode.IADD
                }
            },

            // 0x52: MAR ← SP
            {
                MicroInstructionCode.IADDLoadSecond, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.IADDReadSecond,
                    B = RegisterSelectSignal.SP,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.IADD
                }
            },

            // 0x53: H ← M[MAR]
            {
                MicroInstructionCode.IADDReadSecond, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.IADDComputeResult,
                    B = RegisterSelectSignal.MDR,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.H,
                    MEM = MemoryOperation.ReadWordToMDR,
                    JAM = 0,
                    OpCode = OpCode.IADD
                }
            },

            // 0x54: MDR ← H + MDR
            {
                MicroInstructionCode.IADDComputeResult, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.IADDDecSP,
                    B = RegisterSelectSignal.MDR,
                    ALU = ALUOperation.APlusB,
                    C = 1 << (int)RegisterLoadSignal.MDR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.IADD
                }
            },

            // 0x55: SP ← SP - 4
            {
                MicroInstructionCode.IADDDecSP, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.IADDSetResultAddr,
                    B = RegisterSelectSignal.SP,
                    ALU = ALUOperation.DecrementBBy4,
                    C = 1 << (int)RegisterLoadSignal.SP,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.IADD
                }
            },

            // 0x56: MAR ← SP
            {
                MicroInstructionCode.IADDSetResultAddr, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.IADDStoreResult,
                    B = RegisterSelectSignal.SP,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.IADD
                }
            },

            // 0x57: M[MAR] ← MDR
            {
                MicroInstructionCode.IADDStoreResult, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.FetchStartFetchDecode, // return to fetch
                    B = 0,
                    ALU = ALUOperation.Nop,
                    C = 0,
                    MEM = MemoryOperation.WriteWordFromMDR,
                    JAM = 0,
                    OpCode = OpCode.IADD
                }
            },
            {
                MicroInstructionCode.GOTOLoadMAR, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.GOTOReadHigh,
                    B = RegisterSelectSignal.PC,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.GOTOReadHigh, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.GOTOIncrementPC,
                    B = 0,
                    ALU = ALUOperation.Nop,
                    C = 0,
                    MEM = MemoryOperation.ReadByteToMBR,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.GOTOIncrementPC,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.GOTOStoreHigh,
                    B = RegisterSelectSignal.PC,
                    ALU = ALUOperation.IncrementB,
                    C = 1 << (int)RegisterLoadSignal.PC,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.GOTOStoreHigh, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.GOTOLoadMARLow,
                    B = RegisterSelectSignal.MBR,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.H,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.GOTOLoadMARLow, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.GOTOReadLow,
                    B = RegisterSelectSignal.PC,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.GOTOReadLow, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.GOTOIncrementPCFinal,
                    B = 0,
                    ALU = ALUOperation.Nop,
                    C = 0,
                    MEM = MemoryOperation.ReadByteToMBR,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.GOTOIncrementPCFinal,
                new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.GOTOCombineOffset,
                    B = RegisterSelectSignal.PC, // PC on B-bus
                    ALU = ALUOperation.IncrementB, // PC + 1
                    C = 1 << (int)RegisterLoadSignal.PC, // Store back into PC
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.GOTOCombineOffset, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.GOTOReturnToFetch,
                    B = RegisterSelectSignal.MBR,
                    ALU = ALUOperation.CombineOffset, // custom ALU op: sign-extend and combine H + MDR
                    C = 1 << (int)RegisterLoadSignal.PC,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.GOTOReturnToFetch, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.FetchStartFetchDecode, // return to fetch
                    ALU = ALUOperation.Nop,
                    B = 0,
                    C = 0,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.GOTO
                }
            },
            {
                MicroInstructionCode.SETSPLoadHigh, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.SETSPReadHigh,
                    B = RegisterSelectSignal.PC,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.SETSP
                }
            },

            {
                MicroInstructionCode.SETSPReadHigh, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.SETSPStoreHigh,
                    B = 0,
                    ALU = ALUOperation.Nop,
                    C = 0,
                    MEM = MemoryOperation.ReadByteToMBR,
                    JAM = 0,
                    OpCode = OpCode.SETSP
                }
            },

            {
                MicroInstructionCode.SETSPStoreHigh, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.SETSPIncPCBeforeLow,
                    B = RegisterSelectSignal.MBR,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.H,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.SETSP
                }
            },

            {
                MicroInstructionCode.SETSPIncPCBeforeLow, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.SETSPLoadLow,
                    B = RegisterSelectSignal.PC,
                    ALU = ALUOperation.IncrementB,
                    C = 1 << (int)RegisterLoadSignal.PC,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0
                }
            },

            {
                MicroInstructionCode.SETSPLoadLow, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.SETSPReadLow,
                    B = RegisterSelectSignal.PC,
                    ALU = ALUOperation.PassB,
                    C = 1 << (int)RegisterLoadSignal.MAR,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.SETSP
                }
            },

            {
                MicroInstructionCode.SETSPReadLow, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.SETSPCombineAndStore,
                    B = 0,
                    ALU = ALUOperation.Nop,
                    C = 0,
                    MEM = MemoryOperation.ReadByteToMBR,
                    JAM = 0,
                    OpCode = OpCode.SETSP
                }
            },

            {
                MicroInstructionCode.SETSPCombineAndStore, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.SETSPIncrementPC,
                    B = RegisterSelectSignal.MBR,
                    ALU = ALUOperation.CombineHighLow,
                    C = 1 << (int)RegisterLoadSignal.SP,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.SETSP
                }
            },

            {
                MicroInstructionCode.SETSPIncrementPC, new MicroInstruction
                {
                    Address = (int)MicroInstructionCode.FetchStartFetchDecode,
                    B = RegisterSelectSignal.PC,
                    ALU = ALUOperation.IncrementB,
                    C = 1 << (int)RegisterLoadSignal.PC,
                    MEM = MemoryOperation.NoOp,
                    JAM = 0,
                    OpCode = OpCode.SETSP
                }
            }
        };

        LabelInstructions(microCodeControlStore);

        return microCodeControlStore;
    }

    private static void LabelInstructions(Dictionary<MicroInstructionCode, MicroInstruction> microCodeControlStore)
    {
        foreach (var key in microCodeControlStore.Keys)
        {
            var value = microCodeControlStore[key];

            value.Name = key.ToString();

            value.Key = key;
        }
    }
}
namespace Tests;

using MLDComputing.Emulators.MIC1.Core;
using MLDComputing.Emulators.MIC1.Core.IJVM;
using FluentAssertions;

[TestClass]
public sealed class SimulatorTests
{
    [TestMethod]
    public async Task Run_WhenValidProgramIsLoaded_ItWillRun()
    {
        // Arrange
        var mic1 = new MIC1Simulator
        {
            Registers =
            {
                PC = 1000
            }
        };

        LoadStackTest(mic1);

        // Act
        await mic1.Run();

        // Assert
        mic1.CycleCount.Should().Be(36);
    }

    private static void LoadStackTest(MIC1Simulator mic1)
    {
            // @formatter:off
            mic1.Memory.LoadProgram(1000,
                (byte)OpCode.SETSP,   // 0xD000 - Set the stack pointer
                0xFF,                 // 0xD001 - high byte
                0xFF,                 // 0xD002 - low byte

                (byte)OpCode.BIPUSH,  // 0xD003 - push 42 onto stack
                42,                   // 0xD004 - value to push

                (byte)OpCode.DUP,     // 0xD005 - duplicate top of stack

                (byte)OpCode.HALT     // 0xD006 - stop
            );
        // @formatter:on
    }
}
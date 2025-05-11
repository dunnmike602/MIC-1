🧠 MIC-1 Simulator (.NET/C#)
This project is a high-performance, event-driven emulator of the MIC-1 microprogrammed architecture as described by Andrew S. Tanenbaum in Structured Computer Organization. Implemented in modern C# under the MLDComputing.Emulators.MIC1.Core namespace, it is designed for both educational use and advanced emulation of IJVM programs with deep debugging support.

✨ Features
✅ Fully microcoded MIC-1 execution engine with cycle-accurate simulation

✅ Supports a customizable microinstruction control store with MicroCodeStoreBuilder

✅ Implements Tanenbaum’s IJVM instruction set, including arithmetic, logic, branching, and stack operations

✅ Accurate 32-bit stack management with big-endian layout

✅ Supports execution throttling to match a real processor’s clock speed and frame rate

✅ Unthrottled mode for benchmark or batch execution

✅ Built-in event hooks for:

Execution start/stop

Microinstruction cycles

Performance sampling (PerfEvent)

Trap and fault handling (TrapEvent)

✅ Per-instruction and per-cycle statistics gathering

✅ Visual TOS management and HALT tracking

✅ Extensible trap and memory protection model

🏗 Architecture Highlights
Core Simulation Engine: The MIC1Simulator class manages the instruction fetch-decode-execute loop, microinstruction transitions, memory interactions, and trap conditions.

Memory Model: Supports configurable memory size with optional bounds and write protection.

Registers: Emulates MIC-1 registers including PC, SP, MAR, MDR, MBR, MPC, and the H register.

ALU Pipeline: Uses an inline ALU with condition flag tracking and JAM control logic.

Event Model: Exposes hooks for real-time execution and debugging tools to observe simulator state and flow.

🖥 Example Use
csharp
Copy
Edit
var sim = new MIC1Simulator(memorySize: 65536, targetProcessorSpeed: 1_000_000);
sim.Run(CancellationToken.None, clearState: true, pc: 0x100, bytes: bootProgram);
You can also subscribe to events:

csharp
Copy
Edit
sim.ExecutionEvent += (s, e) => Console.WriteLine($"Executed: {e.EventCode}, Cycles: {e.CycleCount}");
sim.TrapEvent += (s, e) => Console.WriteLine($"Trap: {e.TrapCode} - {e.Message}");
📊 Performance & Profiling
Targeted performance is configurable (e.g., 1 MHz emulation with 60Hz video sync)

Tracks per-opcode microcycle counts

Can output live statistics on IJVM instruction frequency and execution times

🧩 Extensibility
The microcode store can be customized to support new instructions (e.g., SWAP, SETSP, DUP, etc.)

You can extend the IJVM instruction set, modify control store behavior, or implement new trap conditions

🧠 MIC-1 Simulator (.NET/C#)
This project is a high-performance, event-driven emulator of the MIC-1 microprogrammed architecture, as described by Andrew S. Tanenbaum in Structured Computer Organization. Built in C#, it offers cycle-accurate simulation, execution introspection, and microcode customization through the MLDComputing.Emulators.MIC1.Core namespace.

✨ Features
🧩 Fully customizable microcode execution engine

🎯 Supports Tanenbaum’s IJVM instruction set

📈 Real-time statistics tracking and performance analysis

🔁 Execution modes: throttled (frame-accurate) or unthrottled (raw performance)

🧪 Memory bounds and execution protection with trap events

🧠 Hardware-level simulation of the MIC-1 data path, ALU, and register file

📦 Implemented IJVM Instructions
This simulator currently supports the following IJVM and extended instructions, defined under:

Instruction	Description
NOP	No operation
FETCH	Internal opcode fetch routine
BIPUSH	Push a signed byte (extended to 32-bit)
IADD	Integer addition (top 2 stack values)
ISUB	Integer subtraction
IAND	Bitwise AND
IOR	Bitwise OR
IXOR	Bitwise XOR
ISHL	Shift left
ISHR	Arithmetic shift right
IUSHR	Logical shift right
DUP	Duplicate top of stack
GOTO	Unconditional branch
IFEQ	Branch if top of stack == 0
IFNE	Branch if top of stack ≠ 0
POP	Discard top of stack
SWAP	Swap top two 32-bit stack values
SETSP	Custom instruction to set the stack pointer
HALT	Stop processor execution
TOS	Pseudo-op to reload the top of stack pointer

IFLT (Branch if top of stack < 0)

IF_ICMPEQ (Compare top two stack values and branch if equal)

🖥 Example Usage
csharp
Copy
Edit
var sim = new MIC1Simulator(memorySize: 65536, targetProcessorSpeed: 1_000_000);
sim.Run(CancellationToken.None, clearState: true, pc: 0x100, bytes: bootProgram);

MIC-1 Controller Console
The project includes a fully interactive command-line controller under the namespace:

csharp
Copy
Edit
MLDComputing.Emulators.MIC1.Controller
This acts as a front-end interface for driving the simulator, inspecting internal state, debugging, and performance monitoring. It allows users to interactively control the MIC-1 runtime through a set of powerful text-based commands.

✅ Controller Features
🧠 Interactive REPL loop with command auto-completion and history

🛠 Simulator controls: start, stop, reset, exit, set (toggles for memory check, events, etc.)

🪛 System introspection:

reg: Show CPU register values

info: System settings and memory layout

dumpmem: Dump memory regions

dumpstack: Stack dump as raw bytes or 2×16-bit word pairs

🔬 Performance monitoring:

perf: Set performance interval

perfmon: Live performance tracking with ESC to stop

⚠️ Error tracking:

showerrors: View most recent simulation faults

clearerrors: Clear error log

📊 Statistics and profiling:

stats: Show instruction counts and microcycle stats

💬 Built-in command help system with help [command]

🖥️ Example Usage
Launch the interactive controller:

csharp
Copy
Edit
var sim = new MIC1Simulator();
var controller = new Mic1Controller(sim);
controller.Run();
Then use commands like:

text
Copy
Edit
> start
> dumpstack hex sw
> stats d
> perfmon
> stop
🧰 Commands Overview
Command	Description
start/stop	Begin or pause execution
reset	Reset simulator state
set	Toggle features (memory check, stats)
reg	Show register contents
dumpmem	Display memory in multiple formats
dumpstack	Display stack in byte or word mode
stats	Show detailed IJVM execution metrics
perfmon	Start live performance sampling
help	View command help
exit	Cleanly shut down the controller

This console interface makes it easy to drive experiments, debug microcode, or observe performance trends while executing IJVM programs on the MIC-1 simulator.

📘 References
Andrew S. Tanenbaum, Structured Computer Organization – MIC-1 and IJVM chapters

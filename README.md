# MIC-1 Simulator (.NET/C#)

This project is a high-performance, event-driven emulator of the **MIC-1 microprogrammed architecture**, as described by **Andrew S. Tanenbaum** in *Structured Computer Organization*. Built in modern C# under the `MLDComputing.Emulators.MIC1.Core` namespace, it offers accurate microinstruction execution, event tracing, and runtime instrumentation.

---

## ✨ Features

- 🧩 Fully customizable **microcode execution engine**
- 🎯 Supports **Tanenbaum’s IJVM instruction set**
- 📈 Real-time statistics tracking and performance monitoring
- 🔁 Execution modes: **throttled** (synchronized with video frame rate) or **unthrottled**
- 🧪 Memory bounds and execution protection with trap diagnostics
- 🧠 Emulates MIC-1 components including register file, ALU, microinstruction store, and stack

---

## 📦 Implemented IJVM Instructions

The following IJVM instructions are implemented using the `MLDComputing.Emulators.MIC1.Core.IJVM.OpCode` enumeration:

| Instruction | Description                                |
|-------------|--------------------------------------------|
| `NOP`       | No operation                               |
| `FETCH`     | Internal microcode fetch step              |
| `BIPUSH`    | Push a signed byte (extended to 32-bit)    |
| `IADD`      | Integer addition (pop 2, push result)      |
| `ISUB`      | Integer subtraction                        |
| `IAND`      | Bitwise AND                                |
| `IOR`       | Bitwise OR                                 |
| `IXOR`      | Bitwise XOR                                |
| `ISHL`      | Bitwise left shift                         |
| `ISHR`      | Arithmetic shift right                     |
| `IUSHR`     | Logical shift right                        |
| `DUP`       | Duplicate top of stack                     |
| `GOTO`      | Unconditional branch                       |
| `IFEQ`      | Branch if top of stack == 0                |
| `IFNE`      | Branch if top of stack ≠ 0                 |
| `POP`       | Pop and discard top of stack               |
| `SWAP`      | Swap top two 32-bit stack values           |
| `SETSP`     | Set stack pointer (custom extension)       |
| `HALT`      | Halt processor execution                   |
| `TOS`       | Pseudo-instruction to reload top-of-stack  |

**Planned for future implementation:**

- `IFLT` – Branch if top of stack < 0  
- `IF_ICMPEQ` – Compare top two stack values, branch if equal

---

## 🕹 MIC-1 Controller Console

An interactive command-line controller is provided in:

```
MLDComputing.Emulators.MIC1.Controller
```

This acts as a REPL interface for inspecting memory, managing execution, and collecting performance stats.

### ✅ Controller Features

- Interactive command loop with history and auto-completion
- CPU control commands (`start`, `stop`, `reset`, `exit`)
- Introspection: register view, memory dump, stack dump
- Performance monitor: track instruction throughput and microinstruction cycles
- Error trapping and fault diagnostics
- Help system with `help [command]`

### 🖥️ Example Usage

```csharp
var sim = new MIC1Simulator();
var controller = new Mic1Controller(sim);
controller.Run();
```

Then at the prompt:

```
> start
> dumpstack hex sw
> stats d
> stop
> exit
```

### 🧰 Key Commands

| Command        | Description                                  |
|----------------|----------------------------------------------|
| `start` / `stop` | Begin or pause execution                  |
| `reset`        | Reset simulator to initial state             |
| `set`          | Toggle simulator flags (memory check, stats) |
| `reg`          | Show CPU register contents                   |
| `dumpmem`      | Dump memory with format and range options    |
| `dumpstack`    | Dump stack as raw bytes or 16-bit word pairs |
| `stats`        | Show IJVM statistics, optionally detailed    |
| `perfmon`      | Stream performance metrics live              |
| `showerrors`   | Display last processor faults                |
| `clearerrors`  | Clear error log                              |
| `help`         | List commands or get help on one             |
| `exit`         | Quit the controller                          |

---

## 📘 References

- **Andrew S. Tanenbaum**, *Structured Computer Organization* (MIC-1, IJVM chapters)
- [IJVM Overview on Wikipedia](https://en.wikipedia.org/wiki/IJVM)

---

## 📦 Project Structure

```
/Core
  └── MIC1Simulator.cs       // Main execution engine
/Controller
  └── Mic1Controller.cs      // Interactive CLI for the simulator
/MicroCode
  └── MicroInstructionCode.cs // Microcode routine definitions
/IJVM
  └── OpCode.cs              // IJVM instruction mapping
```

---

## 🔧 Build & Run

This project targets **.NET 7+** or newer. To build and run:

```bash
dotnet build
dotnet run --project YourConsoleAppEntry.csproj
```

---

## 🧪 License

This project is licensed under MIT, GPL, or your chosen license.

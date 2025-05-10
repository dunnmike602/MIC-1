namespace MLDComputing.Emulators.MIC1.Controller;

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Components;
using Core;
using Core.Bus;
using Core.Enums;
using Enums;
using Extensions;
using Core.Events;
using static Core.Bus.MemoryLayout;
using Core.Constants;
using Core.IJVM;

public class Mic1Controller
{
    private const string FormatParameterHelpText =
        "[f] is allowed as a parameter, the following values are allowed (D)ec=Decimal, (H)ex=Hexadecimal, (B)in=Binary. If omitted defaults to decimal.";
    
    public MIC1Simulator MIC1 { get; }

    private Dictionary<string, Action<string[]>>? _commands;

    private bool _runCommandLoop = true;
    
    private Dictionary<string, Action> _helpFiles = [];

    public List<TrapEventArgs> Errors { get; } = [];

    private bool _enablePerformanceCount = false;
    
    public Mic1Controller(MIC1Simulator mic1)
    {
        MIC1 = mic1;

        InitializeCommands();

        InitialiseAutoComplete();

        InitialiseEvents();
    }

    private void InitialiseEvents()
    {
        Errors.Clear();
        
        MIC1.TrapEvent += (_, args) =>
        {
            Errors.Add(args);
        };

        var firstTime = true;
        
        MIC1.PerfEvent += (_, args) =>
        {
            if (!_enablePerformanceCount)
            {
                firstTime = true;
                return;
            }

            if (firstTime)
            {
                firstTime = false;
                Task.Run(() =>
                {
                    while (true)
                    {
                        var key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            Console.WriteLine("Performance Counter Stopped.");
                            Console.Write("> ");
                            _enablePerformanceCount = false;
                            break;
                        }
                    }
                });
            }

            var processorTimeSeconds = (double)args.ProcessorTick / Stopwatch.Frequency;

            var effectiveSpeedHz = args.CycleCount / processorTimeSeconds;

            Console.WriteLine();

            Console.WriteLine("{0,-25} {1,-25} {2,-25}","DateTime",
                "CycleCount",
                "ProcessorSpeed (MHz)");
            
            Console.WriteLine("{0,-25:yyyy-MM-dd HH:mm:ss} {1,-25:N0} {2,-25:F6}", args.DateTime,
                args.CycleCount,
                effectiveSpeedHz / 1e6);
        };
    }

    private static void InitialiseAutoComplete()
    {
        var commands = typeof(CommandType)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => f.GetCustomAttribute<DescriptionAttribute>()?.Description)
            .Where(s => s != null)
            .ToArray();

        ReadLine.HistoryEnabled = true;
        ReadLine.AutoCompletionHandler = new SimpleAutoComplete(commands!);
    }

    public void Run()
    {
        ShowWelcomeMessage();

        _runCommandLoop = true;

        while (_runCommandLoop)
        {
            Console.ForegroundColor = ConsoleColor.White;
            
            var line = ReadLine.Read("> ");
            
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var (command, arguments) = ParseInput(line);
            ExecuteCommand(command, arguments);
        }
    }

    private void InitializeCommands()
    {
        _helpFiles = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
        {
            [CommandType.Help.GetDescription()] = DisplayHelpText,
            [CommandType.PerfInterval.GetDescription()] = DisplayPerfText,
            [CommandType.Set.GetDescription()] = DisplaySetText,
            [CommandType.ShowErrors.GetDescription()] = DisplayErrorText,
            [CommandType.ClearErrors.GetDescription()] = DisplayClearErrorsText,
            [CommandType.Info.GetDescription()] = DisplayInfoText,
            [CommandType.HelpH.GetDescription()] = DisplayHelpText,
            [CommandType.Reg.GetDescription()] = DisplayRegText,
            [CommandType.Time.GetDescription()] = DisplayTimeText,
            [CommandType.DumpStack.GetDescription()] = DisplayDumpStackText,
            [CommandType.Exit.GetDescription()] = DisplayExitText,
            [CommandType.ExitX.GetDescription()] = DisplayExitText,
            [CommandType.Stop.GetDescription()] = DisplayStopText,
            [CommandType.Start.GetDescription()] = DisplayStartText,
            [CommandType.Cls.GetDescription()] = DisplayClsText,
            [CommandType.DumpMemory.GetDescription()] = DisplayMemDumpText,
            [CommandType.Reset.GetDescription()] = DisplayResetText,
            [CommandType.Statistics.GetDescription()] = DisplayStatsText,
        };
        
        _commands = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase)
        {
            [CommandType.Help.GetDescription()] = HelpCommand,
            [CommandType.Set.GetDescription()] = SetCommand,
            [CommandType.ShowErrors.GetDescription()] = ShowErrorsCommand,
            [CommandType.ClearErrors.GetDescription()] = ClearErrorsCommand,
            [CommandType.Info.GetDescription()] = InfoCommand,
            [CommandType.HelpH.GetDescription()] = HelpCommand,
            [CommandType.Reg.GetDescription()] = RegCommand,
            [CommandType.Time.GetDescription()] = TimeCommand,
            [CommandType.DumpStack.GetDescription()] = DumpStackCommand,
            [CommandType.DumpMemory.GetDescription()] = DumpMemoryCommand,
            [CommandType.Exit.GetDescription()] = ExitCommand,
            [CommandType.ExitX.GetDescription()] = ExitCommand,
            [CommandType.Stop.GetDescription()] = StopCommand,
            [CommandType.Start.GetDescription()] = StartCommand,
            [CommandType.Reset.GetDescription()] = ResetCommand,
            [CommandType.Cls.GetDescription()] = ClsCommand,
            [CommandType.Statistics.GetDescription()] = ShowStatsCommand,
            [CommandType.PerfInterval.GetDescription()] = SetPerfInterval,
            [CommandType.RunPerf.GetDescription()] = RunPerfMon,
        };
    }

    private void StopCommand(string[] args)
    {
        MIC1.HaltDetected = true;
    }
    private void ClsCommand(string[] args)
    {
        Console.Clear();
    }

    private void ResetCommand(string[] args)
    {
        MIC1.Init();
    }
    
    private void StartCommand(string[] args)
    {
        MIC1.Start();
    }

    private void ClearErrorsCommand(string[] args)
    {
        Errors.Clear();

        Console.WriteLine("Error data has been cleared.");
    }

    private void DisplayCommandText(string command)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(command);
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
    }
    private void DisplayResetText()
    {
        DisplayCommandText("reset");
        Console.WriteLine("Reset the simulator to its initial state.");

        Console.WriteLine();
        Console.WriteLine("EXAMPLES: reset");
    }
    
    private void DisplayMemDumpText()
    {
        DisplayCommandText("dumpmem [f] [< start >] [< length >]");
        Console.WriteLine("Dumps memory in the specified format starting at <start>, showing <length> bytes. Format is optional but will display all values as specified.");
        Console.WriteLine("If no numeric parameters as supplied 0 and 256 are used.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: dumpmem");
        Console.WriteLine("          dumpmem b");
        Console.WriteLine("          dumpmem b 1000 1024");
        Console.WriteLine("          dumpmem 1000 10");
    }

    private void DisplayStatsText()
    {
        DisplayCommandText("stats [d]");
        Console.WriteLine("Display statistics about the simulator. The [d] switch can be used to display detailed instructions about IJVM execution.");
        Console.WriteLine("NOTE: Detailed Stats are only available if the S option has been enabled with Set.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: stats");
        Console.WriteLine("          stats d");
    }

    private void DisplayInfoText()
    {
        DisplayCommandText("info [f]");
        Console.WriteLine("Show information about the MIC-1 Simulator.");
        Console.WriteLine(FormatParameterHelpText);
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: info");
        Console.WriteLine("          info h");
    }

    private void DisplayPerfText()
    {
        DisplayCommandText("perf n.");
        Console.WriteLine("Sets the interval for collecting performance stats. Must be between 1 and 1000.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: perf 1");
    }
    
    private void DisplayHelpText()
    {
        DisplayCommandText("(h)elp [command].");
        Console.WriteLine("Displays a list of commands available in the system.");
        Console.WriteLine("Optionally will display help of a specific command is supplied as a parameter.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: help");
        Console.WriteLine("          help stackdump");
    }


    private void DisplayExitText()
    {
        DisplayCommandText("e(x)it");
        Console.WriteLine("Cleanly exit the simulator. Use X for shortcut.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: exit");
        Console.WriteLine("          x");
    }

    private void DisplayClsText()
    {
        DisplayCommandText("cls");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Clears the console window.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: cls");
    }
    
    private void DisplayStartText()
    {
        DisplayCommandText("start");
        Console.WriteLine("Move out of idle mode and start executing code at the current PC (Program Counter)");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: start");
    }

    private void DisplayClearErrorsText()
    {
        DisplayCommandText("clearerrors");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Clears the processor error collection.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: clearerrors");
    }

    private void DisplaySetText()
    {
        DisplayCommandText("set <flag> on/off");
        Console.WriteLine("Toggles the specific flag: (E)vent, (T)hrottled, (M)emCheck, Detailed (S)tats");
        Console.WriteLine("Events are processor and performance events.");
        Console.WriteLine(
            "Throttled when true will pin the processor to its specified speed, false will run it as fast as possible.");
        Console.WriteLine(
            "Memcheck prevents execution of stack or data and ensures the program area cannot be written to.");
        Console.WriteLine("Stats when on will produce detailed stats of IJVM instruction execution.");
        Console.WriteLine("Performance interval is the gap to report processor stats. Default is 2 seconds and Events must be on for this to work.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: set m on");
        Console.WriteLine("          set s off");
        Console.WriteLine("          set p off");
    }

    private void DisplayErrorText()
    {
        DisplayCommandText("showerrors   [n]");
        Console.WriteLine("Show the last n errors generated by the simulator. Omit n for all errors.");
        Console.WriteLine("Note that if the (E)vent flag is disabled, errors will not be recorded.");
        Console.WriteLine("This flag can be set/unset with the set command. Type: help set.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: showerrors");
        Console.WriteLine("          showerrors 10");
    }
    
    private void DisplayStopText()
    {
        DisplayCommandText("stop");
        Console.WriteLine("Halt any code currently executing on the MIC-1 and return to idle. This actually stops the acculation of cycles and processor time.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: stop");
    }

    private void DisplayTimeText()
    {
        DisplayCommandText("time");
        Console.WriteLine("Show the current date/time and the current number of ticks");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: time");
    }
    
    private void DisplayDumpStackText()
    {
        DisplayCommandText("dumpstack [f]");
        Console.WriteLine("Displays a listing of the stack, from the top of the stack to the current stack pointer.");
        Console.WriteLine(FormatParameterHelpText);
        Console.WriteLine("Optional parameter sw will show the stack as 4 byte words on one line rather than byte by byte.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: dumpstack");
        Console.WriteLine("          dumpstack bin");
        Console.WriteLine("          dumpstack bin sw");
        Console.WriteLine("          dumpstack sw");
    }
    
    private void DisplayRegText()
    {
        DisplayCommandText("reg [f]");
        Console.WriteLine(FormatParameterHelpText);
        Console.WriteLine("Optional parameter sw will show the stack as 4 byte words on one line rather than byte by byte.");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES: reg");
        Console.WriteLine("          reg bin");
    }

    private void RunPerfMon(string[] args)
    {
        MIC1.EnableExecutionEvents = true;
        _enablePerformanceCount = true;
    }

    private void SetPerfInterval(string[] args)
    {
        if (args.Length < 1 || !args[0].IsInteger())
        {
            Console.WriteLine("You must specify the performance interval in seconds");
            return;
        }

        var interval = Convert.ToInt32(args[0]);
        if (interval < 1 || interval > 10)
        {
            Console.WriteLine("Valid values are 1-10 (time is seconds)");
            return;
        }
        
        MIC1.PerformanceCountInterval = interval;
    }

    private void ShowStatsCommand(string[] args)
    {
        var elapsedTime = MIC1.TotalElapsedTime;
        var elapsedIdleTime = MIC1.TotalElapsedIdleTime;
        var elapsedSeconds = (double)elapsedTime.ElapsedTicks / Stopwatch.Frequency;
        var elapsedIdleSeconds = (double)elapsedIdleTime.ElapsedTicks / Stopwatch.Frequency;

        Console.WriteLine("------------------------- Statistics Report -------------------------");
        Console.WriteLine("{0,-30} {1:N0}", "Running on Core:", MIC1.CurrentCore);
        Console.WriteLine("{0,-30} {1:N0}", "Total Microcode Cycles:", MIC1.CycleCount);
        Console.WriteLine("{0,-30} {1:N0}", "Total IJVM Cycles:", MIC1.IJVMCycleCount);
        Console.WriteLine("{0,-30} {1:F10} sec", "Total Processor Time:", elapsedSeconds);
        Console.WriteLine("{0,-30} {1:F10} sec", "Total Idle Time:", elapsedIdleSeconds);

        var effectiveHz = elapsedSeconds == 0
            ? 0
            : MIC1.CycleCount / elapsedSeconds;

        Console.WriteLine("{0,-30} {1:N0} ({2:F3} MHz)", "Effective μInstr/sec:", effectiveHz,
            effectiveHz / 1e6);
        Console.WriteLine("{0,-30} {1:N0} MHz", "Target speed MHZ:",
            SimulatorConstants.Machine.DefaultTargetFrequencyHz / 1e6);

        if (args.Length > 0 && args[0] == CommandParameters.Details.GetDescription())
        {
            Console.WriteLine();
            Console.WriteLine("{0,-30} {1}", "Instr", "Cycles");

            for (var i = 0; i <= (int)OpCode.HALT; i++) // Replace `Last` with your max enum value
            {
                var opcode = (OpCode)i;
                if (MIC1.PerOpcodeCycles[i] != 0)
                {
                    Console.WriteLine("{0,-30} {1}", opcode, MIC1.PerOpcodeCycles[i]);
                }
            }
        }

        Console.WriteLine("-------------------------------------------------------------------");
    }
    
    private void ShowErrorsCommand(string[] args)
    {
        if (Errors.Count == 0)
        {
            Console.WriteLine("No errors to show.");
            return;
        }
        
        int? errorCount = null;

        if (args is { Length: > 0 } && int.TryParse(args[0], out var parsedNumber))
        {
            errorCount = parsedNumber;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine("---------------------------------- Error Log ----------------------------------");
        Console.WriteLine();

        Console.WriteLine("{0,-20} | {1,-25} | {2}", "Date", "TrapCode", "Message");

        var selectedErrors = errorCount.HasValue
            ? Errors.AsEnumerable().Reverse().Take(errorCount.Value)
            : Errors.AsEnumerable().Reverse();
        
        foreach (var error in selectedErrors)
        {
            Console.WriteLine("{0,-20:yyyy-MM-dd HH:mm:ss} | {1,-25} | {2}", error.DateTime, error.TrapCode, error.Message);

            Console.WriteLine();
            Console.WriteLine("  Register Snapshot:");
            Console.WriteLine("    " + error.RegisterSnapshot.Replace("\n", "\n    "));
            Console.WriteLine();
            
            if (!string.IsNullOrWhiteSpace(error.Information))
            {
                Console.WriteLine();
                Console.WriteLine("  Information:");
                Console.WriteLine("    " + error.Information.Replace("\n", "\n    "));
                Console.WriteLine();
            }

            Console.WriteLine(new string('-', 80));
        }
    }

    private void SetCommand(string[] args)
    {
        if (args[0].ToLower()[0] == 'e')
        {
            MIC1.EnableExecutionEvents = args[1].ToLower() == "on";
        }

        if (args[0].ToLower()[0] == 'm')
        {
            MIC1.MemoryChecking = args[1].ToLower() == "on";
        }

        if (args[0].ToLower()[0] == 's')
        {
            MIC1.ShowDetailedStats = args[1].ToLower() == "on";
        }

        if (args[0].ToLower()[0] == 't')
        {
            MIC1.UnThrottled = args[1].ToLower() == "off";
        }
    }

    private (string Command, string[] Arguments) ParseInput(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0];
        var arguments = parts.Length > 1 ? parts[1..] : [];
        return (command, arguments);
    }

    private void ExecuteCommand(string command, string[] args)
    {
        if (_commands!.TryGetValue(command.ToLower(), out var action))
        {
            try
            {
                action(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing '{command}': {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Unknown command: '{command}'. Type 'help' for a list of commands.");
        }
    }

    private void ShowWelcomeMessage()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Type 'help' to see available commands.");
    }

    private void HelpCommand(string[] args)
    {
        var command = ParseArgumentAsString(args, 0).ToLower();
        
        if (!string.IsNullOrEmpty(command) && _helpFiles.ContainsKey(command))
        {
            _helpFiles[command]();
            return;
        }
        
        Console.WriteLine("Available commands. Note command in [] means optional:");
        Console.WriteLine();
        WriteLabelValue("cls                                      - ", "Clears the console window.");
        WriteLabelValue("reg          [f]                         - ", "Show CPU register values in specified format.");
        WriteLabelValue("perf         n                           - ", "Sets the interval for collecting performance stats.");
        WriteLabelValue("stop                                     - ", "Halt any code currently executing on the MIC-1 and return to idle.");
        WriteLabelValue("start                                    - ", "Move out of idle mode and start executing code at the current PC (Program Counter).");
        WriteLabelValue("time                                     - ", "Show the current date/time.");
        WriteLabelValue("set          <flag> on/off               - ", "Toggles the specific flag that effects behaviour of the simulator. Use help set for more information.");
        WriteLabelValue("dumpmem      [f] [<start>] [<length>]    - ", "Dumps memory in the specified format starting at <start>, showing <length> bytes.");
        WriteLabelValue("info         [f]                         - ", "Show information about the MIC-1 Simulator.");
        WriteLabelValue("dumpstack    [f]                         - ", "Dump the stack in specified format: decimal, hex, binary, or octal.");
        WriteLabelValue("stats        [f]                         - ", "Show Detailed Processor Stats in specified format.");
        WriteLabelValue("perf         [f]                         - ", "Trace Processor performance in specified format.");
        WriteLabelValue("showerrors   [n]                         - ", "Show error the last n errors generated by the simulator. Omit n for all errors.");
        WriteLabelValue("clearerrors                              - ", "Clears the processor error collection.");
        WriteLabelValue("(h)elp       [command]                   - ", "Show help, optionally for a specific [command].");
        WriteLabelValue("stats        [d]                         - ", "Display information about the simulator. [d] for more detailed stats.");
        WriteLabelValue("reset                                    - ", "Reset the simulator to its initial state.");
        WriteLabelValue("e(x)it                                   - ", "Cleanly exit the simulator.");
    }

    private static string ParseArgumentAsString(string?[] args, int argIndex)
    {
        var returnValue = args.Length > argIndex ? args[argIndex] ?? string.Empty : string.Empty;
        return returnValue;
    }

    private static int ParseArgumentAsInt(string?[] args, int argIndex)
    {
        if (args.Length <= argIndex || string.IsNullOrWhiteSpace(args[argIndex]))
        {
            return 0;
        }

        var input = args[argIndex]!.Trim();

        // Handle hexadecimal values: starts with "0x" or "0X"
        if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            var hexPart = input[2..]; // Remove "0x"
            var hex = int.TryParse(hexPart, NumberStyles.HexNumber, null, out var hexValue) && hexValue >= 0
                ? hexValue
                : 0;

            return hex;
        }

        // Handle decimal values
        if (int.TryParse(input, out var decimalValue) && decimalValue >= 0)
        {
            return decimalValue;
        }

        return 0;
    }

    private static NumberFormat ParseNumberFormat(string[] args)
    {
        if (args.Length <= 0)
        {
            return NumberFormat.Decimal;
        }

        var key = args[0].ToLowerInvariant();
        return key switch
        {
            "d" or "dec" or "decimal" => NumberFormat.Decimal,
            "h" or "hex" => NumberFormat.Hex,
            "b" or "bin" or "binary" => NumberFormat.Binary,
            "o" or "oct" or "octal" => NumberFormat.Octal,
            _ => NumberFormat.Decimal
        };

    }

    private int ComputeWidth(IEnumerable<string> rows)
    {
        var max = rows.Max(r => r.Length);

        max = Math.Max(max, 7);
        
        return max + 4;
    }

    private void InfoCommand(string[] args)
    {
        var format = ParseNumberFormat(args);
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("---- Information ----");

        WriteLabelValue("Memory Size (bytes):\t\t", MIC1.MemorySize.ToString());
        WriteLabelValue("Processor Speed (MHz):\t\t", MIC1.UnThrottled ? "Max Possible"  : (MIC1.TargetProcessorSpeed / 1e6).ToString(CultureInfo.InvariantCulture));
        WriteLabelValue("Memory Region Checking:\t\t", MIC1.MemoryChecking ? "Enabled" : "Disabled");
        WriteLabelValue("Execution Events:\t\t", MIC1.EnableExecutionEvents ? "Enabled" : "Disabled");
        WriteLabelValue("Detailed Stats:\t\t\t", MIC1.ShowDetailedStats ? "Enabled" : "Disabled");
        WriteLabelValue("Processor Status:\t\t", MIC1.HaltDetected ? "Idle" : "Running");
        WriteLabelValue("Throttled:\t\t\t", MIC1.UnThrottled ? "No" : "Yes");
        
        if (MIC1.MemoryChecking)
        {
            Console.WriteLine();
            Console.WriteLine("---- Memory Regions ----");
            
            WriteLabelValue("Code Start:\t\t", CodeSegment.Start.FormatValue(format));
            WriteLabelValue("Code End:\t\t", CodeSegment.End.FormatValue(format));
            Console.WriteLine();
            WriteLabelValue("Data Start:\t\t", DataSegment.Start.FormatValue(format));
            WriteLabelValue("Data End:\t\t", DataSegment.End.FormatValue(format));
            Console.WriteLine();
            WriteLabelValue("Stack Top:\t\t", StackSegment.Top.FormatValue(format));
            WriteLabelValue("Stack Bottom:\t\t", StackSegment.Bottom.FormatValue(format));
        }
    }

    private void DumpMemoryCommand(string[] args)
    {
        const int defaultLength = 256;
        
        var format = ParseNumberFormat(args);

        var numIntegerArgs = args.Length > 0 && args[0].IsInteger() ? args.Length : args.Length - 1;
        int intArgsStart = args.Length > 0 && args[0].IsInteger() ? 0 : 1;
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("---- Memory Dump ----");
        Console.WriteLine();

        var startMem = 0;
        var length = defaultLength;

        if (numIntegerArgs > 1)
        {
            startMem = ParseArgumentAsInt(args, intArgsStart);
            var parsed = ParseArgumentAsInt(args, 1 + intArgsStart);
            length = parsed != 0 ? parsed : defaultLength;

        }
        else if(numIntegerArgs > 0)
        {
            startMem = ParseArgumentAsInt(args, intArgsStart);
        }
       
        if (startMem > Memory.Size)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Memory location {startMem} is out of range");
        }

        var endMem = Math.Min(startMem + length, Memory.Size);

        var tabs = "\t";

        if (format == NumberFormat.Binary)
        {
            tabs = "\t\t\t";
        }
        WriteLabel($"Address\t{tabs}");
        WriteLabel($"Offset 0{tabs}");
        WriteLabel($"Offset 1{tabs}");
        WriteLabel($"Offset 2{tabs}");
        WriteLabel($"Offset 3{tabs}");
        
        Console.WriteLine();
        
        for (var address = startMem; address < endMem; address+=4)
        {
            WriteLabel($"{address.FormatValue(format)}\t\t");

            var chars = string.Empty;
            
            var value = Memory.ReadByte(address);
            var valueText = value.FormatValue(format);
            var missingText = $"{" ".PadRight(valueText.Length - 1)}\t\t";

            WriteValue($"{valueText}\t\t");

            chars += value.ToPrintableChar();
            
            if (address + 1 < endMem)
            {
                var value1 = Memory.ReadByte(address + 1);
                WriteValue($"{value1.FormatValue(format)}\t\t");
                chars += value1.ToPrintableChar();
            }
            else
            {
                WriteValue(missingText); 
            }

            if (address + 2 < endMem)
            {
                var value2 = Memory.ReadByte(address + 2);
                WriteValue($"{value2.FormatValue(format)}\t\t");
                chars += value2.ToPrintableChar();
            }
            else
            {
                WriteValue(missingText);
            }
            
            if (address + 3 < endMem)
            {
                var value3 = Memory.ReadByte(address + 3);
                WriteValue($"{value3.FormatValue(format)}\t\t");
                chars += value3.ToPrintableChar();
            }
            else
            {
                WriteValue(missingText);
            }

            WriteValue($"\t{chars}");

            Console.WriteLine();
        }
    }

    private void DumpStackCommand(string[] args)
    {
        var format = ParseNumberFormat(args);

        var showWords = ParseArgumentAsString(args, 1) == CommandParameters.ShowWords.GetDescription() ||
                        ParseArgumentAsString(args, 0) == CommandParameters.ShowWords.GetDescription();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("---- Stack Dump ----");
        Console.WriteLine();
        
        if (StackSegment.Top == Registers.SP)
        {
            Console.WriteLine("Stack is Empty");
            return;
        }
        
        var rows = new List<(int Address, string Label, string Value)>();

        if (showWords)
        {
            BuildStackAsWords(rows, format);
        }
        else
        {
            BuildStackAsBytes(rows, format);
        }
        

        var padWidth = ComputeWidth(rows.Select(r => r.Label));

        WriteLabelValue("Address".PadRight(padWidth), "Value");

        rows.ForEach(d =>
        {
            if (d.Address == Registers.SP)
            {
                WriteLabelValue(d.Label.PadRight(padWidth), d.Value + " -> SP");
            }
            else
            {
                WriteLabelValue(d.Label.PadRight(padWidth), d.Value);
            }
        });
    }

    private static void BuildStackAsWords(List<(int Address, string Label, string Value)> rows, NumberFormat format)
    {
        // Remember stack order is reversed compared to main memory
        for (var address = StackSegment.Top - 1; address >= Registers.SP; address -= 4)
        {
            var b0 = Memory.ReadByte(address -3);     // LSB 
            var b1 = Memory.ReadByte(address - 2);
            var b2 = Memory.ReadByte(address - 1);
            var b3 = Memory.ReadByte(address);                // MSB

            var intValue = (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;

            rows.Add((address - 3, (address - 3).FormatValue(format), intValue.FormatValue(format)));
        }
    }
    
    private static void BuildStackAsBytes(List<(int Address, string Label, string Value)> rows, NumberFormat format)
    {
        for (var address = StackSegment.Top - 1; address >= Registers.SP; address--)
        {
            var value = Memory.ReadByte(address);

            rows.Add((address, address.FormatValue(format), value.FormatValue(format)));
        }
    }

    private void RegCommand(string[] args)
    {
        var format = ParseNumberFormat(args);

        // Header
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("---- CPU Registers ----");
        Console.ResetColor();

        // Each register: label in white, value in green
        WriteLabelValue("PC  : ", Registers.PC.FormatValue(format));
        WriteLabelValue("SP  : ", Registers.SP.FormatValue(format));
        WriteLabelValue("H   : ", Registers.H.FormatValue(format));
        WriteLabelValue("MPC : ", Registers.MPC.FormatValue(format));
        WriteLabelValue("MAR : ", Registers.MAR.FormatValue(format));
        WriteLabelValue("MDR : ", Registers.MDR.FormatValue(format));
        WriteLabelValue("MBR : ", ((int)Registers.MBR).FormatValue(format));
        WriteLabelValue("LV  : ", Registers.LV.FormatValue(format));
        WriteLabelValue("CPP : ", Registers.CPP.FormatValue(format));
        WriteLabelValue("TOS : ", Registers.TOS.FormatValue(format));
        WriteLabelValue("OPC : ", Registers.OPC.FormatValue(format));

        // Footer
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("------------------------");
        Console.ResetColor();
    }

    // And here’s the helper again, in case you need it:
    private void WriteLabelValue(string label, string value)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(label);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(value);
        Console.ResetColor();
    }

    private void WriteLabel(string label)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(label);
        Console.ResetColor();
    }

    private void WriteValue(string value)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(value);
        Console.ResetColor();
    }

    private void TimeCommand(string[] args)
    {
        Console.WriteLine($"Current time: {DateTime.Now:T}\tTicks: {DateTime.Now.Ticks}");
    }

    private void ExitCommand(string[] args)
    {
        Console.WriteLine("Exiting...");
        _runCommandLoop = false;
    }
}
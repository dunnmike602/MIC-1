namespace MLDComputing.Emulators.MIC1.Controller.Enums;

using System.ComponentModel;

public enum CommandType
{
    [Description("help")] Help,

    [Description("h")] HelpH,

    [Description("info")] Info,
    
    [Description("reg")] Reg,

    [Description("time")] Time,

    [Description("showerrors")] ShowErrors,

    [Description("clearerrors")] ClearErrors,

    [Description("dumpstack")] DumpStack,

    [Description("dumpmem")] DumpMemory,

    [Description("set")] Set,
    
    [Description("exit")] Exit,

    [Description("x")] ExitX,

    [Description("start")] Start,

    [Description("stop")] Stop,

    [Description("cls")] Cls,

    [Description("reset")] Reset,

    [Description("stats")] Statistics,

    [Description("perf")] PerfInterval,

    [Description("perfmon")] RunPerf
}
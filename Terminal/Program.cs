using System.IO.Pipes;

using var pipeServer = new NamedPipeServerStream("LogPipe", PipeDirection.In);
using var reader = new StreamReader(pipeServer);

Console.WriteLine("Log viewer ready. Waiting for messages...");

pipeServer.WaitForConnection();

while (!reader.EndOfStream)
{
    var line = reader.ReadLine();
    Console.WriteLine(line);
}
namespace MLDComputing.Emulators.MIC1.Ipc;

using System.IO.Pipes;
using System.Text;

public class LogPipeClient
{
    private NamedPipeClientStream? _pipeClient;
    
    private StreamWriter? _writer;

    public bool Connect(string pipeName = "LogPipe")
    {
        try
        {
            _pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            _pipeClient.Connect(1000); // wait 1 second max
            _writer = new StreamWriter(_pipeClient, Encoding.UTF8) { AutoFlush = true };
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public void SendLog(string message)
    {
        if (_pipeClient?.IsConnected == true)
        {
            _writer!.WriteLine(message);
        }
    }

    public void Disconnect()
    {
        _writer?.Dispose();
        _pipeClient?.Dispose();
    }
}
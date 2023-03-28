namespace BD.WTTS.Services.Implementation;

public abstract class IPCServiceImpl<TPacket> : IPCService<TPacket>, IDisposable where TPacket : class
{
    bool disposedValue;
    NamedPipeClientStream? pipeClient;

    public async ValueTask<int> RunAsync(string? pipeName)
    {
        if (pipeClient != null)
            return (int)IPCExitCode.Ok;

        if (string.IsNullOrWhiteSpace(pipeName))
            return (int)IPCExitCode.EmptyPipeName;

        pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

        try
        {
            // 尝试在 45 秒内连接主进程
            pipeClient.Connect(TimeSpan.FromSeconds(45d));
        }
        catch (TimeoutException)
        {
            return (int)IPCExitCode.ConnectServerTimeout;
        }

        while (!disposedValue)
        {
            var packet = ReadBytes(pipeClient);
            if (packet != default)
            {
                var exitCode = await HandleCommand(packet);
                if (exitCode.HasValue)
                    return exitCode.Value;
            }
        }
        return (int)IPCExitCode.Ok;
    }

    public void Send(TPacket packet)
    {
        if (disposedValue || pipeClient == null)
            return;

        Write(pipeClient, packet);
    }

    protected virtual ValueTask<int?> HandleCommand(TPacket packet)
    {
        return default;
    }

    protected abstract TPacket? ReadBytes(Stream stream);

    protected abstract void Write(Stream stream, TPacket packet);

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                pipeClient?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            pipeClient = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

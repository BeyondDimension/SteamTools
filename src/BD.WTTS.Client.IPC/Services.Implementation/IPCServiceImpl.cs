namespace BD.WTTS.Services.Implementation;

public abstract class IPCServiceImpl<TPacket> : IPCService<TPacket>, IDisposable where TPacket : class
{
    bool disposedValue;
    NamedPipeClientStream? pipeClient;

    public async ValueTask<int> RunAsync(string? pipeName)
    {
        if (pipeClient != null) // 此函数应仅调用一次
            return (int)IPCExitCode.Ok;

        if (string.IsNullOrWhiteSpace(pipeName))
            return (int)IPCExitCode.EmptyPipeName;

        // 命名管道实现 IPC
        pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

        try
        {
            // https://blog.lindexi.com/post/dotnet-%E4%BD%BF%E7%94%A8-NamedPipeClientStream-%E8%BF%9E%E6%8E%A5%E4%B8%80%E4%B8%AA%E4%B8%8D%E5%AD%98%E5%9C%A8%E7%AE%A1%E9%81%93%E6%9C%8D%E5%8A%A1%E5%90%8D%E5%B0%86%E4%B8%8D%E6%96%AD%E7%A9%BA%E8%B7%91-CPU-%E8%B5%84%E6%BA%90.html
            // 尝试在 15 秒内连接主进程
            const int timeout_connect = 15000;
            await pipeClient.ConnectAsync(timeout_connect);
        }
        catch (TimeoutException)
        {
            return (int)IPCExitCode.ConnectServerTimeout;
        }

        while (!disposedValue)
        {
            var packet = Read(pipeClient);
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

    /// <summary>
    /// 处理数据包
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    protected virtual ValueTask<int?> HandleCommand(TPacket packet)
    {
        return default;
    }

    /// <summary>
    /// 从流中读取一个数据包
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    protected virtual TPacket? Read(Stream stream)
    {
        Span<byte> inBuffer = stackalloc byte[sizeof(int)];
        stream.Read(inBuffer);
        var len = BitConverter.ToInt32(inBuffer);
        if (len <= 0) return default;
        inBuffer = stackalloc byte[len];
        stream.Read(inBuffer);
        var obj = MemoryPackSerializer.Deserialize<TPacket>(inBuffer);
        return obj;
    }

    /// <summary>
    /// 将一个数据包写入流中
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="packet"></param>
    protected virtual void Write(Stream stream, TPacket packet)
    {
        byte[] outBuffer = MemoryPackSerializer.Serialize(packet);
        stream.Write(BitConverter.GetBytes(outBuffer.Length));
        stream.Write(outBuffer);
        stream.Flush();
    }

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

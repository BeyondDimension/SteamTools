if (!args.Any()) return (int)ExitCode.EmptyArrayArgs;
var pipeName = args[0];
if (string.IsNullOrWhiteSpace(pipeName)) return (int)ExitCode.EmptyPipeName;

using var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

try
{
    // 尝试在 45 秒内连接主进程
    pipeClient.Connect(TimeSpan.FromSeconds(45d));
}
catch (TimeoutException)
{
    return (int)ExitCode.ConnectServerTimeout;
}

while (true)
{
    var packet = IReverseProxyPacket.ReadBytes(pipeClient);
    if (packet != default)
    {
        if (packet is ReverseProxyPacket packet1)
        {
            switch (packet1.Command)
            {
                case ReverseProxyCommand.Exit:
                    goto exit;
                case ReverseProxyCommand.GetFlowStatistics:
                    FlowStatistics flowStatistics = null!;
                    IReverseProxyPacket.Write(pipeClient, flowStatistics);
                    break;
                case ReverseProxyCommand.Start:
                    break;
                case ReverseProxyCommand.Stop:
                    break;
            }
        }
    }
}

exit: return (int)ExitCode.Ok;

enum ExitCode
{
    Ok = 0,
    EmptyArrayArgs = 4001,
    EmptyPipeName,
    ConnectServerTimeout,
}
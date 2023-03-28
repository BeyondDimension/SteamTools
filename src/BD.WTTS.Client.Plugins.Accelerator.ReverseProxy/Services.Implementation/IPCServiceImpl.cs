namespace BD.WTTS.Services.Implementation;

sealed class IPCServiceImpl : IPCServiceImpl<IReverseProxyPacket>, IPCService
{
    protected override ValueTask<int?> HandleCommand(IReverseProxyPacket packet)
    {
        if (packet is ReverseProxyPacket packet1)
        {
            switch (packet1.Command)
            {
                case ReverseProxyCommand.Exit:
                    return new ValueTask<int?>((int)IPCExitCode.Ok);
                case ReverseProxyCommand.GetFlowStatistics:
                    FlowStatistics flowStatistics = null!;
                    Send(flowStatistics);
                    break;
                case ReverseProxyCommand.Start:
                    break;
                case ReverseProxyCommand.Stop:
                    break;
            }
        }

        return default;
    }

    public void NotifyDNSError(Exception ex)
    {

    }

    protected override IReverseProxyPacket? ReadBytes(Stream stream)
    {
        return IReverseProxyPacket.ReadBytes(stream);
    }

    protected override void Write(Stream stream, IReverseProxyPacket packet)
    {
        IReverseProxyPacket.Write(stream, packet);
    }
}

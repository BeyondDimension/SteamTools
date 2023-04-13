// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class IPCServiceImpl : IPCServiceImpl<IReverseProxyPacket>, IPCService
{
    public IPCServiceImpl()
    {
        // 保持无依赖注入的空构造函数
    }

    protected override async ValueTask<int?> HandleCommand(IReverseProxyPacket packet)
    {
        if (packet is ReverseProxyPacket packet1)
        {
            switch (packet1.Command)
            {
                case ReverseProxyCommand.Exit:
                    return (int)IPCExitCode.Ok;
                case ReverseProxyCommand.GetFlowStatistics:
                    FlowStatistics flowStatistics = null!;
                    Send(flowStatistics);
                    break;
                case ReverseProxyCommand.Start:
                    var startResult = await IReverseProxyService.Instance.StartProxyAsync();
                    Send(ReverseProxyCommand.StartResult, startResult ? null : "Startup failed.");
                    break;
                case ReverseProxyCommand.Stop:
                    await IReverseProxyService.Instance.StopProxyAsync();
                    break;
                case ReverseProxyCommand.HotReloadConfig:
                    break;
            }
        }

        return default;
    }

    public void Send(ReverseProxyCommand command, string? data = null)
    {
        ReverseProxyPacket packet = new(command, data);
        Send(packet);
    }
}

namespace BD.WTTS.Services;

public interface IPCService : IPCService<IReverseProxyPacket>
{
    static IPCService Instance => Ioc.Get<IPCService>();

    /// <summary>
    /// 给主进程发送指令以及可选的附加消息
    /// </summary>
    /// <param name="command"></param>
    /// <param name="data"></param>
    void Send(ReverseProxyCommand command, string? data = null);
}

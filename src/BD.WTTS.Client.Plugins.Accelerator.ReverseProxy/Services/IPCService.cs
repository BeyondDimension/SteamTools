namespace BD.WTTS.Services;

public interface IPCService : IPCService<IReverseProxyPacket>
{
    static IPCService Instance => Ioc.Get<IPCService>();

    void NotifyDNSError(Exception ex);
}

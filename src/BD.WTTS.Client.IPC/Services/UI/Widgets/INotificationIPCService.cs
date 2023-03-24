// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public partial interface INotificationIPCService
{
    static INotificationIPCService Instance => Ioc.Get<INotificationIPCService>();

    void NotifyDNSErrorNotify(Exception ex);
}

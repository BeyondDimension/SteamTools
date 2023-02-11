// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 由平台实现的服务
/// </summary>
public partial interface IPlatformService
{
    static IPlatformService Instance => Ioc.Get<IPlatformService>();

    protected const string TAG = "PlatformS";
}

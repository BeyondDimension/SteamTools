using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services.Implementation;

sealed class YarpReverseProxyServiceImpl : ReverseProxyServiceImpl
{
    public YarpReverseProxyServiceImpl(IPlatformService platformService) : base(platformService)
    {

    }

    protected override void DisposeCore()
    {
        base.DisposeCore();
    }
}
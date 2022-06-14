using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services.Implementation;

sealed class TitaniumReverseProxyServiceImpl : ReverseProxyServiceImpl
{
    public TitaniumReverseProxyServiceImpl(IPlatformService platformService) : base(platformService)
    {

    }

    protected override void DisposeCore()
    {
        base.DisposeCore();
    }
}

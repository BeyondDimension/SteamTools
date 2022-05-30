namespace System.Application.Services.Implementation;

sealed class ConnectivityPlatformServiceImpl : IConnectivityPlatformService
{
    NetworkAccess IConnectivityPlatformService.NetworkAccess
        => Connectivity.NetworkAccess.Convert();
}

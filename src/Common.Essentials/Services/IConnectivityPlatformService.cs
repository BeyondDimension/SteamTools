namespace System.Application.Services;

public interface IConnectivityPlatformService
{
    static IConnectivityPlatformService? Interface => DI.Get_Nullable<IConnectivityPlatformService>();

    NetworkAccess NetworkAccess { get; }
}

using System;

namespace ArchiSteamFarm
{
    public interface IArchiSteamFarmHelperService
    {
        static IArchiSteamFarmHelperService Instance => DI.Get<IArchiSteamFarmHelperService>();

        int IPCPortValue { get; }
    }
}
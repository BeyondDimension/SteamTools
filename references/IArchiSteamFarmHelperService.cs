using System;
using System.Threading.Tasks;

namespace ArchiSteamFarm
{
    public interface IArchiSteamFarmHelperService
    {
        static IArchiSteamFarmHelperService Instance => DI.Get<IArchiSteamFarmHelperService>();

        int IPCPortValue { get; }

        Task Restart();
    }
}
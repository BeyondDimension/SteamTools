#if !EXCLUDE_ASF
using ArchiSteamFarm;
using System.Application.Settings;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ArchiSteamFarm.Library;

namespace System.Application.Services;

partial interface IArchiSteamFarmService : IArchiSteamFarmHelperService
{
    int CurrentIPCPortValue { get; protected set; }

    int IArchiSteamFarmHelperService.IPCPortValue
    {
        get
        {
            CurrentIPCPortValue = ASFSettings.IPCPortId.Value;
            if (CurrentIPCPortValue == default) CurrentIPCPortValue = ASFSettings.DefaultIPCPortIdValue;
            if (ASFSettings.IPCPortOccupiedRandom.Value)
            {
                if (SocketHelper.IsUsePort(CurrentIPCPortValue))
                {
                    CurrentIPCPortValue = SocketHelper.GetRandomUnusedPort(IPAddress.Loopback);
                    return CurrentIPCPortValue;
                }
            }
            return CurrentIPCPortValue;
        }
    }
}
#endif
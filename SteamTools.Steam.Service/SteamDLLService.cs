using Steam4NET;
using SteamTool.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SteamTool.Steam.Service
{
    public class SteamDLLService
    {
        private ISteam006 _steam006;
        private IClientUser _clientUser;
        private ISteamApps001 _steamApps001;
        private ISteamApps003 _steamApps003;
        private IClientEngine _clientEngine;
        private ISteamUser016 _steamUser016;
        private IClientFriends _clientFriends;
        private ISteamClient012 _steamClient012;
        private ISteamFriends002 _steamFriends002;

        private int _user;
        private int _pipe;

        public OperationResult ConnectToSteam()
        {
            var operationResult = new OperationResult(OperationResultType.Error, "unknown error.");
            TSteamError steamError = new TSteamError();
            if (!Steamworks.Load(true))
            {
                operationResult.Message = $"Steamworks failed to load.";
                Logger.Info(operationResult.Message);
                return operationResult;
            }

            if (Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory) == Steamworks.GetInstallPath())
            {
                operationResult.Message = $"You are not allowed to run this application from the Steam directory folder.";
                Logger.Info(operationResult.Message);
                return operationResult;
            }

            _steam006 = Steamworks.CreateSteamInterface<ISteam006>();
            if (_steam006.Startup(0, ref steamError) == 0)
            {
                operationResult.Message = $"ISteam006 failed to start. it returned 0.";
                Logger.Info(operationResult.Message);
                return operationResult;
            }

            _steamClient012 = Steamworks.CreateInterface<ISteamClient012>();
            _clientEngine = Steamworks.CreateInterface<IClientEngine>();

            if (_steamClient012 == null)
            {
                operationResult.Message = $"ISteamClient012 is null. Unable to create interface.";
                Logger.Info(operationResult.Message);
                return operationResult;
            }

            if (_clientEngine == null)
            {
                operationResult.Message = $"IClientEngine is null. Unable to create interface.";
                Logger.Info(operationResult.Message);
                return operationResult;
            }

            _pipe = _steamClient012.CreateSteamPipe();
            if (_pipe == 0)
            {
                operationResult.Message = $"ISteamClient012 failed to create pipe connection to Steam.";
                Logger.Info(operationResult.Message);
                return operationResult;
            }

            _user = _steamClient012.ConnectToGlobalUser(_pipe);
            if (_user == 0 || _user == -1)
            {
                operationResult.Message = $"ISteamClient012 failed to connect to global user. Value: {_user}";
                Logger.Info(operationResult.Message);
                return operationResult;
            }

            _steamUser016 = _steamClient012.GetISteamUser<ISteamUser016>(_user, _pipe);
            _clientUser = _clientEngine.GetIClientUser<IClientUser>(_user, _pipe);
            _clientFriends = _clientEngine.GetIClientFriends<IClientFriends>(_user, _pipe);
            _steamApps001 = _steamClient012.GetISteamApps<ISteamApps001>(_user, _pipe);
            _steamApps003 = _steamClient012.GetISteamApps<ISteamApps003>(_user, _pipe);
            _steamFriends002 = _steamClient012.GetISteamFriends<ISteamFriends002>(_user, _pipe);


            return operationResult;
        }


    }
}

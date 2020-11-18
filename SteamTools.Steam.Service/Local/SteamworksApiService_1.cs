using SteamTool.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SteamTool.Steam.Service.Local
{
    public class SteamworksApiService_1
    {
        //private ISteam006 _steam006;
        //private IClientUser _clientUser;
        //private ISteamApps001 _steamApps001;
        //private ISteamApps003 _steamApps003;
        //private IClientEngine _clientEngine;
        //private ISteamUser016 _steamUser016;
        //private IClientFriends _clientFriends;
        //private ISteamClient012 _steamClient012;
        //private ISteamFriends002 _steamFriends002;
        //private int _user;
        //private int _pipe;
        //public ISteam006 Steam006 { get => _steam006; set => _steam006 = value; }
        //public IClientUser ClientUser { get => _clientUser; set => _clientUser = value; }
        //public ISteamApps001 SteamApps001 { get => _steamApps001; set => _steamApps001 = value; }
        //public ISteamApps003 SteamApps003 { get => _steamApps003; set => _steamApps003 = value; }
        //public IClientEngine ClientEngine { get => _clientEngine; set => _clientEngine = value; }
        //public ISteamUser016 SteamUser016 { get => _steamUser016; set => _steamUser016 = value; }
        //public IClientFriends ClientFriends { get => _clientFriends; set => _clientFriends = value; }
        //public ISteamClient012 SteamClient012 { get => _steamClient012; set => _steamClient012 = value; }
        //public ISteamFriends002 SteamFriends002 { get => _steamFriends002; set => _steamFriends002 = value; }
        //public int User { get => _user; set => _user = value; }
        //public int Pipe { get => _pipe; set => _pipe = value; }

        //public OperationResult ConnectToSteam()
        //{
        //    var operationResult = new OperationResult(OperationResultType.Error, "未知的错误");
        //    TSteamError steamError = new TSteamError();
        //    if (!Steamworks.Load(true))
        //    {
        //        operationResult.Message = $"Steamworks加载失败";
        //        Logger.Warn(operationResult.Message);
        //        return operationResult;
        //    }

        //    if (Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory) == Steamworks.GetInstallPath())
        //    {
        //        operationResult.Message = $"您不允许从Steam目录文件夹中运行此应用程序";
        //        Logger.Warn(operationResult.Message);
        //        return operationResult;
        //    }

        //    Steam006 = Steamworks.CreateSteamInterface<ISteam006>();
        //    if (Steam006.Startup(0, ref steamError) == 0)
        //    {
        //        operationResult.Message = $"ISteam006无法启动，它返回0。";
        //        Logger.Info(operationResult.Message);
        //        return operationResult;
        //    }

        //    SteamClient012 = Steamworks.CreateInterface<ISteamClient012>();
        //    ClientEngine = Steamworks.CreateInterface<IClientEngine>();

        //    if (SteamClient012 == null)
        //    {
        //        operationResult.Message = $"ISteamClient012为空。无法创建接口。";
        //        Logger.Warn(operationResult.Message);
        //        return operationResult;
        //    }

        //    if (ClientEngine == null)
        //    {
        //        operationResult.Message = $"IClientEngine为空。无法创建接口。";
        //        Logger.Warn(operationResult.Message);
        //        return operationResult;
        //    }

        //    Pipe = SteamClient012.CreateSteamPipe();
        //    if (Pipe == 0)
        //    {
        //        operationResult.Message = $"ISteamClient012未能建立与Steam的管道连接。";
        //        Logger.Warn(operationResult.Message);
        //        return operationResult;
        //    }

        //    User = SteamClient012.ConnectToGlobalUser(Pipe);
        //    if (User == 0 || User == -1)
        //    {
        //        operationResult.Message = $"ISteamClient012未能连接到全局用户。Value: {User}";
        //        Logger.Warn(operationResult.Message);
        //        return operationResult;
        //    }

        //    SteamUser016 = SteamClient012.GetISteamUser<ISteamUser016>(User, Pipe);
        //    ClientUser = ClientEngine.GetIClientUser<IClientUser>(User, Pipe);
        //    ClientFriends = ClientEngine.GetIClientFriends<IClientFriends>(User, Pipe);
        //    SteamApps001 = SteamClient012.GetISteamApps<ISteamApps001>(User, Pipe);
        //    SteamApps003 = SteamClient012.GetISteamApps<ISteamApps003>(User, Pipe);
        //    SteamFriends002 = SteamClient012.GetISteamFriends<ISteamFriends002>(User, Pipe);
        //    operationResult.ResultType = OperationResultType.Success;
        //    operationResult.Message = "OK";
        //    return operationResult;
        //}


    }
}

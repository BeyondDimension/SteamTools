using Steam4NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteamTool.Steam4NET.Test
{
    public class SteamToNET
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
        public ISteam006 Steam006 { get => _steam006; set => _steam006 = value; }
        public IClientUser ClientUser { get => _clientUser; set => _clientUser = value; }
        public ISteamApps001 SteamApps001 { get => _steamApps001; set => _steamApps001 = value; }
        public ISteamApps003 SteamApps003 { get => _steamApps003; set => _steamApps003 = value; }
        public IClientEngine ClientEngine { get => _clientEngine; set => _clientEngine = value; }
        public ISteamUser016 SteamUser016 { get => _steamUser016; set => _steamUser016 = value; }
        public IClientFriends ClientFriends { get => _clientFriends; set => _clientFriends = value; }
        public ISteamClient012 SteamClient012 { get => _steamClient012; set => _steamClient012 = value; }
        public ISteamFriends002 SteamFriends002 { get => _steamFriends002; set => _steamFriends002 = value; }
        public int User { get => _user; set => _user = value; }
        public int Pipe { get => _pipe; set => _pipe = value; }

        public void ConnectToSteam()
        {
            TSteamError steamError = new TSteamError();
            if (!Steamworks.Load(true))
            {
                Console.WriteLine("Steamworks加载失败");
                return;
            }

            if (Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory) == Steamworks.GetInstallPath())
            {
                Console.WriteLine("您不允许从Steam目录文件夹中运行此应用程序");
                return;
            }

            Steam006 = Steamworks.CreateSteamInterface<ISteam006>();
            if (Steam006.Startup(0, ref steamError) == 0)
            {
                Console.WriteLine("ISteam006无法启动，它返回0。");
                return;
            }

            SteamClient012 = Steamworks.CreateInterface<ISteamClient012>();
            ClientEngine = Steamworks.CreateInterface<IClientEngine>();

            if (SteamClient012 == null)
            {
                Console.WriteLine("ISteamClient012为空。无法创建接口。");
                return;
            }

            if (ClientEngine == null)
            {
                Console.WriteLine("IClientEngine为空。无法创建接口。");
                return;
            }

            Pipe = SteamClient012.CreateSteamPipe();
            if (Pipe == 0)
            {
                Console.WriteLine("ISteamClient012未能建立与Steam的管道连接。");
                return;
            }

            User = SteamClient012.ConnectToGlobalUser(Pipe);
            if (User == 0 || User == -1)
            {
                Console.WriteLine($"ISteamClient012未能连接到全局用户。Value: {User}");
                return;
            }

            SteamUser016 = SteamClient012.GetISteamUser<ISteamUser016>(User, Pipe);
            ClientUser = ClientEngine.GetIClientUser<IClientUser>(User, Pipe);
            ClientFriends = ClientEngine.GetIClientFriends<IClientFriends>(User, Pipe);
            SteamApps001 = SteamClient012.GetISteamApps<ISteamApps001>(User, Pipe);
            SteamApps003 = SteamClient012.GetISteamApps<ISteamApps003>(User, Pipe);
            SteamFriends002 = SteamClient012.GetISteamFriends<ISteamFriends002>(User, Pipe);
        }
    }
}

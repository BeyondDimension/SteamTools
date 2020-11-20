using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAM.API;
using SteamTool.Core.Common;
using SteamTool.Model;

namespace SteamTool.Steam.Service.Local
{
    public class SteamworksApiService
    {
        public Client SteamClient = new Client();

        public bool Initialize()
        {
            try
            {
                SteamClient.Initialize(0);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
                return false;
            }
            return true;
        }
        public bool Initialize(int appid)
        {
            try
            {
                SteamClient.Initialize(appid);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
                return false;
            }
            return true;
        }

        public long GetSteamId64()
        {
            if (SteamClient.SteamUser == null)
                return 0;
            return (long)SteamClient.SteamUser.GetSteamId();
        }

        public bool OwnsApps(uint appid)
        {
            if (SteamClient.SteamApps008 == null)
                return false;
            return SteamClient.SteamApps008.IsSubscribedApp(appid);
        }

        public List<SteamApp> OwnsApps(List<SteamApp> apps)
        {
            if (SteamClient.SteamApps008 == null || SteamClient.SteamApps001 == null)
                return null;
            return apps.FindAll(f => SteamClient.SteamApps008.IsSubscribedApp(f.AppId))
                .OrderBy(s => s.Name).Select((s, i) => new SteamApp
                {
                    Index = i + 1,
                    AppId = s.AppId,
                    Name = s.Name,
                    Type = Enum.TryParse<SteamAppTypeEnum>(SteamClient.SteamApps001.GetAppData(s.AppId, "type"), true, out var result) ? result : SteamAppTypeEnum.Unknown,
                    //Logo = SteamClient.SteamApps001.GetAppData(s.AppId, "logo"),
                }).ToList();
        }

        public bool IsSteamChinaLauncher() 
        {
            return SteamClient.SteamUtils.IsSteamChinaLauncher();
        }
        public bool IsAppInstalled(uint appid)
        {
            return SteamClient.SteamApps008.IsAppInstalled(appid);
        }
        public string GetIPCountry()
        {
            return SteamClient.SteamUtils.GetIPCountry();
        }
    }
}

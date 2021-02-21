using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SteamTool.Model
{
    public class SteamApp : IComparable<SteamApp>
    {
        public SteamApp()
        {

        }

        public int Index { get; set; }

        public uint AppId { get; set; }

        public bool IsInstalled { get; set; }

        public string InstalledDir { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }

        public string Icon { get; set; }

        public SteamAppTypeEnum Type { get; set; }

        public string LogoUrl => string.IsNullOrEmpty(Logo) ? null : string.Format(Const.STEAMAPP_LOGO_URL, AppId, Logo);

        public string HeaderLogoUrl => string.Format(Const.STEAMAPP_CAPSULE_URL, AppId);

        public string IconUrl => string.IsNullOrEmpty(Icon) ? null : string.Format(Const.STEAMAPP_LOGO_URL, AppId, Icon);

        public Process Process { get; set; }

        public TradeCard Card { get; set; }

        public SteamAppInfo Common { get; set; }

        public int CompareTo(SteamApp other)
        {
            return Name.CompareTo(other.Name);
        }

        public string GetIdAndName()
        {
            return $"{AppId} | {Name}";
        }
    }

    public class TradeCard
    {
        public double MinutesPlayed { get; set; }

        public double Price { get; set; }

        public int CardSremaining { get; set; }
    }

    public class Applist
    {
        public List<SteamApp> Apps { get; set; }
    }

    public class SteamApps
    {
        public Applist AppList { get; set; }
    }
}

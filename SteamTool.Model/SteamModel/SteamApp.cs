using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SteamTool.Model
{
    public class SteamApp: IComparable<SteamApp>
    {
        public SteamApp() 
        {

        }

        public int Index { get; set; }

        public uint AppId { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }

        public SteamAppTypeEnum Type { get; set; }

        public string LogoUrl  => $@"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{AppId}/{Logo}.jpg";

        public string HeaderLogoUrl => $@"https://steamcdn-a.akamaihd.net/steam/apps/{AppId}/header.jpg";

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

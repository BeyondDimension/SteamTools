using System.Diagnostics;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    public class SteamApp : IComparable<SteamApp>
    {
        public int Index { get; set; }

        public uint AppId { get; set; }

        public bool IsInstalled { get; set; }

        public string? InstalledDir { get; set; }

        public string? Name { get; set; }

        public string? Logo { get; set; }

        public string? Icon { get; set; }

        public SteamAppType Type { get; set; }

        public string? LogoUrl => string.IsNullOrEmpty(Logo) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Logo);

        public string HeaderLogoUrl => string.Format(STEAMAPP_CAPSULE_URL, AppId);

        public string? IconUrl => string.IsNullOrEmpty(Icon) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Icon);

        public Process? Process { get; set; }

        public TradeCard? Card { get; set; }

        public SteamAppInfo? Common { get; set; }

        public int CompareTo(SteamApp? other) => string.Compare(Name, other?.Name);

        public string GetIdAndName()
        {
            return $"{AppId} | {Name}";
        }
    }
}
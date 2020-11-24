using System;

namespace SteamTool.Model
{
    public class Const
    {
        public const string APP_LIST_FILE = "apps.json";
        public const string SETTINGS_FILE = "settings.json";
        public const string SCRIPT_DIR = "scripts";
        public const string HostTag = "#S302";

        public const string MY_PROFILE_URL = "https://steamcommunity.com/profiles/76561198289531723/";
        public const string MY_MINIPROFILE_URL = "https://steamcommunity.com/miniprofile/329265995";

        public const string STEAMAPP_LIST_URL = "https://api.steampowered.com/ISteamApps/GetAppList/v2";
        public const string STORE_APP_URL = "https://store.steampowered.com/app/{0}";
        public const string STEAMAPP_LOGO_URL = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
        public const string STEAMAPP_ICON_URL = "http://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}";
        public const string STEAMAPP_CAPSULE_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/capsule_184x69.jpg";
        public const string STEAMAPP_HEADIMAGE_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg";
        public const string STEAM_MEDIA_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/movie_max.webm";
        public const string STEAM_VIDEO_URL = "https://store.steampowered.com/video/watch/{0}/";

        public const string STEAM_INSTALL_URL = "steam://install/{0}";
        public const string STEAM_RUNGAME_URL = "steam://rungameid/{0}";

        public const string STEAMDB_USERINFO_URL = "https://steamdb.ml/api/v1/users/{0}";
        public const string STEAMDB_APPINFO_URL = "https://steamdb.ml/api/v1/apps/{0}";
    }
}

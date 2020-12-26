using System;

namespace SteamTool.Model
{
    public class Const
    {
        public const string APP_LIST_FILE = "apps.json";
        public const string SETTINGS_FILE = "settings.json";
        public const string SCRIPT_DIR = "scripts";
        public const string HOST_TAG = "#S302";

        public const string REWARDMELIST_URL = "https://gitee.com/rmbgame/steam-tools_-data/raw/master/RewardRecord.json";
        //public const string REWARDMELIST_URL = "https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/RewardRecord.json";

        public const string GITHUB_LATEST_RELEASEAPI_URL = "https://api.github.com/repos/rmbadmin/SteamTools/releases/latest";
        public const string GITHUB_RELEASEAPI_URL = "https://api.github.com/repos/rmbadmin/SteamTools/releases";

        public const string GITHUB_URL = "https://github.com/rmbadmin/SteamTools";
        public const string GITHUB_RELEASES_URL = "https://github.com/rmbadmin/SteamTools/releases";
        public const string MY_PROFILE_URL = "https://steamcommunity.com/profiles/76561198289531723/";
        public const string MY_WISHLIST_URL = "https://store.steampowered.com/wishlist/profiles/76561198289531723/";
        public const string MY_MINIPROFILE_URL = "https://steamcommunity.com/miniprofile/329265995";

        public const string STEAM_BADGES_URL = "https://steamcommunity.com/profiles/{0}/badges/";
        public const string STEAMAPP_LIST_URL = "https://api.steampowered.com/ISteamApps/GetAppList/v2";
        public const string STORE_APP_URL = "https://store.steampowered.com/app/{0}";
        public const string STEAMAPP_LOGO_URL = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
        public const string STEAMAPP_ICON_URL = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}";
        public const string STEAMAPP_CAPSULE_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/capsule_184x69.jpg";
        public const string STEAMAPP_HEADIMAGE_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg";
        public const string STEAM_MEDIA_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/movie_max.webm";
        public const string STEAM_VIDEO_URL = "https://store.steampowered.com/video/watch/{0}/";
        public const string STEAM_REGISTERKEY_URL = "https://store.steampowered.com/account/registerkey?key={0}";

        public const string STEAM_INSTALL_URL = "steam://install/{0}";
        public const string STEAM_RUNGAME_URL = "steam://rungameid/{0}";
        public const string STEAM_OPENURL = "steam://openurl/{0}";
        public const string STEAM_RUN_KEYURL = "steam://openurl/https://store.steampowered.com/account/registerkey?key={0}";

        public const string STEAMDB_USERINFO_URL = "https://api.steamdb.ml/v1/users/{0}";
        public const string STEAMDB_APPINFO_URL = "https://api.steamdb.ml/v1/apps/{0}";
    }
}

namespace System.Application
{
    public static class SteamApiUrls
    {
        public const string MY_PROFILE_URL = "https://steamcommunity.com/profiles/76561198289531723/";
        public const string MY_WISHLIST_URL = "https://store.steampowered.com/wishlist/profiles/76561198289531723/";
        public const string MY_MINIPROFILE_URL = "https://steamcommunity.com/miniprofile/329265995";

        public const string STEAM_PROFILES_URL = "https://steamcommunity.com/profiles/{0}";

        public const string STEAM_LOGIN_URL = "https://steamcommunity.com/login/home/?goto=my/profile";
        public const string STEAM_BADGES_URL = "https://steamcommunity.com/profiles/{0}/badges/";
        public const string STEAMAPP_LIST_URL = "https://api.steampowered.com/ISteamApps/GetAppList/v2";
        public const string STEAMAPP_LOGO_URL = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
        public const string STEAMAPP_LIBRARY_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/library_600x900.jpg";
        public const string STEAMAPP_LIBRARYHERO_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/library_hero.jpg";
        public const string STEAMAPP_LIBRARYLOGO_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo.png";

        /// <summary>
        /// 多语言版本logo，{1}传入steam多语言文本
        /// </summary>
        public const string STEAMAPP_LIBRARYLOGO_LOCALIZED_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo_{1}.png";

        public const string STEAMAPP_ICON_URL = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}";
        public const string STEAMAPP_CAPSULE_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/capsule_184x69.jpg";
        public const string STEAMAPP_HEADIMAGE_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg";
        public const string STEAM_MEDIA_URL = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/movie_max.webm";
        public const string STEAM_VIDEO_URL = "https://store.steampowered.com/video/watch/{0}/";
        public const string STEAM_REGISTERKEY_URL = "https://store.steampowered.com/account/registerkey?key={0}";
        public const string STEAM_FRIENDMESSAGESLOG_URL = "https://help.steampowered.com/zh-cn/accountdata/GetFriendMessagesLog";

        public const string STEAM_INSTALL_URL = "steam://install/{0}";
        public const string STEAM_RUNGAME_URL = "steam://rungameid/{0}";
        public const string STEAM_OPENURL = "steam://openurl/{0}";
        public const string STEAM_RUN_KEYURL = "steam://openurl/https://store.steampowered.com/account/registerkey?key={0}";

        public const string STEAMDB_USERINFO_URL = "https://api.steamdb.ml/v1/users/{0}";
        public const string STEAMDB_APPINFO_URL = "https://api.steamdb.ml/v1/apps/{0}";
        public const string STORE_APP_URL = "https://store.steampowered.com/app/{0}";
        public const string STEAMDBINFO_URL = "https://steamdb.info/app/{0}";

        /// <summary>
        /// 这里需要steamid3而不是id64
        /// </summary>
        public const string STEAM_MINIPROFILE_URL = "https://steam-chat.com/miniprofile/{0}/json";
        public const string STEAM_USERINFO_XML_URL = "https://steamcommunity.com/profiles/{0}?xml=1";


        public const string STEAMCN_USERINFO_XML_URL = "https://my.steamchina.com/profiles/76561198289531723?xml=1";
    }
}
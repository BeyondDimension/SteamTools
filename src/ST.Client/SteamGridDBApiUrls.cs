namespace System.Application
{
    public static class SteamGridDBApiUrls
    {
        public const string SteamGridDB_APP_URL = "https://www.steamgriddb.com/game/{0}";
        
        public const string SteamGridDB_Author_URL = "https://www.steamgriddb.com/profile/{0}";

        public const string API_BaseUrl = "https://www.steamgriddb.com/api/v2";

        public const string SteamGridDBUrl = "https://www.steamgriddb.com";

        //Retrieve game by game ID.
        public const string RetrieveGameById_Url = API_BaseUrl + "/games/id/{0}";

        //Retrieve game by Steam App ID.
        public const string RetrieveGameBySteamAppId_Url = API_BaseUrl + "/games/steam/{0}";


        //Retrieve grids by game ID.
        public const string RetrieveGrids_Url = API_BaseUrl + "/grids/game/{0}";

        //Retrieve grids by platform ID.
        //{0} "steam" "origin" "egs" "bnet" "uplay" "flashpoint" "eshop"
        //{1} Game Id
        public const string RetrieveGridsByPlatformId_Url = API_BaseUrl + "/grids/{0}/{1}";


        //Retrieve heroes by game ID.
        public const string RetrieveHeros_Url = API_BaseUrl + "/heroes/game/{0}";

        //Retrieve heroes by platform ID.
        //{0} "steam" "origin" "egs" "bnet" "uplay" "flashpoint" "eshop"
        //{1} Game Id
        public const string RetrieveHerosByPlatformId_Url = API_BaseUrl + "/heroes/{0}/{1}";


        //Retrieve logos by game ID.
        public const string RetrieveLogos_Url = API_BaseUrl + "/logos/game/{0}";

        //Retrieve logos by platform ID.
        //{0} "steam" "origin" "egs" "bnet" "uplay" "flashpoint" "eshop"
        //{1} Game Id
        public const string RetrieveLogosByPlatformId_Url = API_BaseUrl + "/logos/{0}/{1}";


        //Retrieve icons by game ID.
        public const string RetrieveIcons_Url = API_BaseUrl + "/icons/game/{0}";

        //Retrieve icons by platform ID.
        //{0} "steam" "origin" "egs" "bnet" "uplay" "flashpoint" "eshop"
        //{1} Game Id
        public const string RetrieveIconsByPlatformId_Url = API_BaseUrl + "/icons/{0}/{1}";


        //Search for a game by name.
        public const string SearchGameByName_Url = API_BaseUrl + "/search/autocomplete/{0}";



        public const string SteamGridDBUrl_Grid = SteamGridDBUrl + "/grid/{0}";

        public const string SteamGridDBUrl_Hero = SteamGridDBUrl + "/hero/{0}";

        public const string SteamGridDBUrl_Icon = SteamGridDBUrl + "/icon/{0}";
        
        public const string SteamGridDBUrl_Logo = SteamGridDBUrl + "/logo/{0}";
    }
}
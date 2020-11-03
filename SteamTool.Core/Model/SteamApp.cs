using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace SteamTool.Core.Model
{
    public class SteamApp
    {
        public long AppId { get; set; }

        public SteamAppInfo Common { get; set; }

    }

    public class SteamAppInfo
    {

        public string ClientIcon { get; set; }

        public string ClientTga { get; set; }

        public string Name { get; set; }

        public Dictionary<string,short> Languages { get; set; }


        public string Logo { get; set; }
        public string Logo_Small { get; set; }
        public string Icon { get; set; }
        public string OsList { get; set; }
        public string Type { get; set; }


        public string Metacritic_Name { get; set; }


        public Dictionary<string, string> Small_Capsule { get; set; }
        public Dictionary<string, string> Header_Image { get; set; }

    }

}

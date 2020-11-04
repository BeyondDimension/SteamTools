using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Model
{
    public class SteamApp
    {
        public long AppId { get; set; }

        public SteamAppInfo Common { get; set; }
    }
}

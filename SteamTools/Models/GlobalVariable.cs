using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.Models
{
    public class GlobalVariable
    {
        public static readonly GlobalVariable Instance = new GlobalVariable();

        public string SteamPath { get; set; }

        public string SteamExePath { get; set; }

        public SteamUser CurrentSteamUser { get; set; }

        public List<SteamUser> LocalSteamUser { get; set; }

        public List<SteamApp> CurrentSteamUserApp { get; set; }


    }
}

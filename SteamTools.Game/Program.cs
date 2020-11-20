using SteamTool.Steam.Service;
using SteamTool.Steam.Service.Local;
using SteamTools.Game.Services;
using System;
using System.Diagnostics;

namespace SteamTools.Game
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Process.Start("SAM.Picker.exe");
                return;
            }

            if (int.TryParse(args[0], out int appId) == false)
            {
                return;
            }

            if (SAM.API.Steam.GetInstallPath() == AppDomain.CurrentDomain.BaseDirectory)
            {
                return;
            }

            SteamConnectService.Current.Initialize(appId);

            Console.WriteLine("status:" + SteamConnectService.Current.IsConnectToSteam.ToString());

            Console.ReadKey();
        }
    }
}

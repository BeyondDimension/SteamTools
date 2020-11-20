using System;
using Steamworks;
using Steamworks.Data;

namespace SteamTool.Steamworks.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // Init Client
            //
            SteamClient.Init(0);


            foreach (var a in SteamUserStats.Achievements)
            {
                Console.WriteLine($"{a.Identifier}");
                Console.WriteLine($"	a.State: {a.State}");
                Console.WriteLine($"	a.UnlockTime: {a.UnlockTime}");
                Console.WriteLine($"	a.Name: {a.Name}");
                Console.WriteLine($"	a.Description: {a.Description}");
                Console.WriteLine($"	a.GlobalUnlocked:	{a.GlobalUnlocked}");
                a.GetIconAsync().Wait();
                var icon = a;

                Console.WriteLine($"	a.Icon:	{icon}");
            }

            Console.WriteLine($"User.SteamID: {SteamClient.SteamId.Value}");

            var deaths = new Stat("deaths");
            Console.WriteLine($"{deaths.Name} {deaths.GetInt()} times");
            Console.WriteLine($"{deaths.Name} {deaths.GetFloat()} times");

            Console.WriteLine($"User.SteamLevel: {SteamUser.SteamLevel}");

            Console.ReadKey();
        }
    }
}

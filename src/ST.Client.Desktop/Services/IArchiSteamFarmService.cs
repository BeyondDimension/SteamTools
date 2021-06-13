using ReactiveUI;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IArchiSteamFarmService
    {
        public static IArchiSteamFarmService Instance => DI.Get<IArchiSteamFarmService>();

        public string? ArchiSteamFarmExePath { get; }
        public string? ArchiSteamFarmConfigPath { get; }
        public string? ArchiSteamFarmOutText { get; }

        public Process? Process { get; }

        public bool IsArchiSteamFarmExists { get; }

        public bool IsArchiSteamFarmRuning { get; }



        void SetArchiSteamFarmExePath(string path);
        string GetArchiSteamFarmIPCUrl();

        bool RunArchiSteamFarm();
        void StopArchiSteamFarm();

        void SetArchiSteamFarmConfig();

        void AddBot();
    }
}
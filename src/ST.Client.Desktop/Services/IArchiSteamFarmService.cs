using ReactiveUI;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    public interface IArchiSteamFarmService
    {
        public static IArchiSteamFarmService Instance => DI.Get<IArchiSteamFarmService>();

        public string? ArchiSteamFarmExePath { get; }
        public string? ArchiSteamFarmConfigPath { get; }
        public StringBuilder ArchiSteamFarmOutText { get; }

        public Process? Process { get; }

        public bool IsArchiSteamFarmExists { get; }

        public bool IsArchiSteamFarmRuning { get; }

        void SetArchiSteamFarmExePath(string path);
        string GetArchiSteamFarmIPCUrl();

        bool RunArchiSteamFarm();

        void SetArchiSteamFarmConfig();

        void AddBot();
    }
}
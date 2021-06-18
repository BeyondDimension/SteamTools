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
        public string? ArchiSteamFarmConfigDirPath { get; }
        public string? ArchiSteamFarmOutText { get; }

        public string? IPCUrl { get; }
        public Process? Process { get; }

        public bool IsArchiSteamFarmExists { get; }

        public bool IsArchiSteamFarmRuning { get; }


        void SetArchiSteamFarmExePath(string path);

        bool RunArchiSteamFarm();
        void StopArchiSteamFarm();

        void WirteLineCommand(string command, bool useipc = false);
        void SetArchiSteamFarmConfig();

        void AddBot();
    }
}
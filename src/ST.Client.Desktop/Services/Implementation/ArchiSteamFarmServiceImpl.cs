using ReactiveUI;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed class ArchiSteamFarmServiceImpl : ReactiveObject, IArchiSteamFarmService
    {
        readonly IDesktopPlatformService platformService;
        public string? ArchiSteamFarmExePath { get; private set; }
        public string? ArchiSteamFarmConfigPath => string.IsNullOrEmpty(ArchiSteamFarmExePath) ? Path.GetDirectoryName(ArchiSteamFarmExePath) : null;

        public StringBuilder ArchiSteamFarmOutText { get; private set; } = new StringBuilder();

        public Process? Process { get; private set; }

        public bool IsArchiSteamFarmExists => File.Exists(ArchiSteamFarmExePath);

        public bool IsArchiSteamFarmRuning => !Process?.HasExited ?? false;

        public ArchiSteamFarmServiceImpl(IDesktopPlatformService platform)
        {
            platformService = platform;

            var temp = Path.Combine(IOPath.BaseDirectory, "ASF", "ArchiSteamFarm.exe");
            SetArchiSteamFarmExePath(temp);
        }

        public void SetArchiSteamFarmExePath(string path)
        {
            if (File.Exists(path) && Path.GetExtension(path) == "exe")
            {
                ArchiSteamFarmExePath = path;
            }
        }

        public bool RunArchiSteamFarm()
        {
            try
            {
                if (IsArchiSteamFarmExists)
                {
                    Task.Run(() =>
                    {
                        Process = new Process();
                        Process.StartInfo.FileName = ArchiSteamFarmExePath;//要执行的程序名称 
                        Process.StartInfo.UseShellExecute = true;
                        Process.StartInfo.StandardOutputEncoding = Encoding.Default;
                        Process.StartInfo.RedirectStandardInput = true;
                        Process.StartInfo.RedirectStandardOutput = true;
                        Process.StartInfo.RedirectStandardError = true;
                        Process.StartInfo.CreateNoWindow = true;//不显示程序窗口 
                        Process.Start();//启动程序 
                        Process.StandardInput.AutoFlush = true;
                        //Process.StartInfo.Verb = "runas";

                        //Process.StandardInput.WriteLine();
                        StreamReader standardOutput = Process.StandardOutput;

                        while (!standardOutput.EndOfStream)
                        {
                            string? text = standardOutput.ReadLine();
                            ArchiSteamFarmOutText.Append(text + Environment.NewLine);
                        }

                        Process.Close();
                        standardOutput.Close();
                    }).Forget();
                }
            }
            catch (Exception ex)
            {
                Log.Error(nameof(IArchiSteamFarmService), ex, nameof(RunArchiSteamFarm));
            }
            return IsArchiSteamFarmRuning;
        }

        public string GetArchiSteamFarmIPCUrl()
        {
            return "http://0.0.0.0:1242";
        }

        public void SetArchiSteamFarmConfig()
        {

        }

        public void AddBot()
        {

        }

        public void GetAllBots()
        {

        }

        public void SaveBotConfig()
        {

        }
    }
}
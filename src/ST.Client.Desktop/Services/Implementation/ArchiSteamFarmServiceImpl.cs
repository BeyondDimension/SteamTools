using ReactiveUI;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed class ArchiSteamFarmServiceImpl : ReactiveObject, IArchiSteamFarmService
    {
        readonly IDesktopPlatformService platformService;

        string? _ArchiSteamFarmExePath;
        public string? ArchiSteamFarmExePath
        {
            get => _ArchiSteamFarmExePath;
            private set => this.RaiseAndSetIfChanged(ref _ArchiSteamFarmExePath, value);
        }

        public string? ArchiSteamFarmConfigPath => !string.IsNullOrEmpty(ArchiSteamFarmExePath) ? Path.GetDirectoryName(ArchiSteamFarmExePath) : null;

        public string? ArchiSteamFarmVersion => !string.IsNullOrEmpty(ArchiSteamFarmExePath) ? FileVersionInfo.GetVersionInfo(ArchiSteamFarmExePath).ProductVersion : null;

        string? _ArchiSteamFarmOutText;
        public string? ArchiSteamFarmOutText
        {
            get => _ArchiSteamFarmOutText;
            private set => this.RaiseAndSetIfChanged(ref _ArchiSteamFarmOutText, value);
        }

        Process? _Process;
        public Process? Process
        {
            get => _Process;
            private set => this.RaiseAndSetIfChanged(ref _Process, value);
        }

        public bool IsArchiSteamFarmExists => !string.IsNullOrEmpty(ArchiSteamFarmExePath) ? File.Exists(ArchiSteamFarmExePath) : false;

        public bool IsArchiSteamFarmRuning => !Process?.HasExited ?? false;

        public ArchiSteamFarmServiceImpl(IDesktopPlatformService platform)
        {
            platformService = platform;

            ArchiSteamFarmExePath = ASFSettings.ArchiSteamFarmExePath.Value;
            if (string.IsNullOrEmpty(ArchiSteamFarmExePath))
            {
                var temp = Path.Combine(IOPath.BaseDirectory, "ASF", "ArchiSteamFarm.exe");
                SetArchiSteamFarmExePath(temp);
            }
        }

        public void SetArchiSteamFarmExePath(string path)
        {
            if (File.Exists(path) && Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                ArchiSteamFarmExePath = path;
                ASFSettings.ArchiSteamFarmExePath.Value = path;
            }
        }

        public bool RunArchiSteamFarm()
        {
            try
            {
                if (!IsArchiSteamFarmRuning)
                {
                    if (IsArchiSteamFarmExists)
                    {
                        Task.Run(() =>
                        {
                            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                            Process = new Process();
                            Process.StartInfo.FileName = ArchiSteamFarmExePath;//要执行的程序名称 
                            Process.StartInfo.UseShellExecute = false;
                            Process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(R.Culture.TextInfo.ANSICodePage);
                            Process.StartInfo.StandardInputEncoding = Process.StartInfo.StandardOutputEncoding;
                            Process.StartInfo.StandardErrorEncoding = Process.StartInfo.StandardOutputEncoding;
                            Process.StartInfo.RedirectStandardInput = true;
                            Process.StartInfo.RedirectStandardOutput = true;
                            Process.StartInfo.RedirectStandardError = true;
                            Process.StartInfo.CreateNoWindow = true;//不显示程序窗口 
                            Process.Start();//启动程序 
                            Process.StandardInput.AutoFlush = true;
                            //Process.StartInfo.Verb = "runas";
                            this.RaisePropertyChanged(nameof(IsArchiSteamFarmRuning));
                            using StreamReader standardOutput = Process.StandardOutput;

                            //Process.StandardInput.WriteLine();
                            while (!standardOutput.EndOfStream)
                            {
                                string? text = standardOutput.ReadLine();
#if DEBUG
                                Debug.WriteLine(text);
#endif
                                ArchiSteamFarmOutText += (text + Environment.NewLine);
                            }

                            Process.Close();
                            standardOutput.Close();
                        }).Forget();
                    }
                    else
                    {
                        Toast.Show(AppResources.ASF_ExeNoExists);
                    }
                }
                else
                {
                    Toast.Show(AppResources.ASF_RuningTip);
                }
            }
            catch (Exception ex)
            {
                Log.Error(nameof(IArchiSteamFarmService), ex, nameof(RunArchiSteamFarm));
            }
            return IsArchiSteamFarmRuning;
        }

        public void WirteLineCommand(string command) 
        {
            if (IsArchiSteamFarmRuning && !string.IsNullOrEmpty(command)) 
            {
                Process!.StandardInput.WriteLine(command);
                Process!.StandardInput.Flush();
            }
        }

        public void StopArchiSteamFarm()
        {
            if (IsArchiSteamFarmRuning)
            {
                Process?.Kill();
            }
        }

        public string GetArchiSteamFarmIPCUrl()
        {
            return "http://127.0.0.1:1242";
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

        public void UpdateOrDownloadASF()
        {


        }
    }
}
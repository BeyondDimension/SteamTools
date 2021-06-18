using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed class ArchiSteamFarmServiceImpl : ReactiveObject, IArchiSteamFarmService
    {
        readonly IDesktopPlatformService platformService;
        readonly IHttpService httpService;
        readonly ICloudServiceClient clientService;

        const string GITHUB_LATEST_RELEASEAPI_URL = "https://api.github.com/repos/JustArchiNET/ArchiSteamFarm/releases/latest";

        static string Variant => DI.Platform switch
        {
            Platform.Windows => "win-x64",
            Platform.Linux => "linux-x64",
            Platform.Apple => "osx-x64",
            _ => "win-x64",
        };



        string? _ArchiSteamFarmExePath;
        public string? ArchiSteamFarmExePath
        {
            get => _ArchiSteamFarmExePath;
            private set => this.RaiseAndSetIfChanged(ref _ArchiSteamFarmExePath, value);
        }

        public string? ArchiSteamFarmConfigDirPath => !string.IsNullOrEmpty(ArchiSteamFarmExePath) ? Path.GetDirectoryName(ArchiSteamFarmExePath) : null;

        public Version? ArchiSteamFarmVersion => !string.IsNullOrEmpty(ArchiSteamFarmExePath) ? new Version(FileVersionInfo.GetVersionInfo(ArchiSteamFarmExePath).ProductVersion) : null;

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

        bool _IsNewVerison;
        public bool IsNewVerison
        {
            get => _IsNewVerison;
            private set => this.RaiseAndSetIfChanged(ref _IsNewVerison, value);
        }

        GithubReleaseModel? _NewVerison;
        public GithubReleaseModel? NewVerison
        {
            get => _NewVerison;
            private set => this.RaiseAndSetIfChanged(ref _NewVerison, value);
        }

        string? _IPCUrl;
        public string? IPCUrl
        {
            get => _IPCUrl;
            private set => this.RaiseAndSetIfChanged(ref _IPCUrl, value);
        }

        public ArchiSteamFarmServiceImpl(IDesktopPlatformService platform, IHttpService http, ICloudServiceClient client)
        {
            clientService = client;
            platformService = platform;
            httpService = http;

            ArchiSteamFarmExePath = ASFSettings.ArchiSteamFarmExePath.Value;
            if (string.IsNullOrEmpty(ArchiSteamFarmExePath))
            {
                var temp = Path.Combine(IOPath.BaseDirectory, "ASF", "ArchiSteamFarm.exe");
                SetArchiSteamFarmExePath(temp);
            }

            if (IsArchiSteamFarmExists)
            {
                IPCUrl = "http://127.0.0.1:1242";
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
                //Process!.StandardInput.Flush();
            }
        }

        public void StopArchiSteamFarm()
        {
            if (IsArchiSteamFarmRuning)
            {
                Process?.Kill();
            }
        }

        public void SetArchiSteamFarmConfig()
        {

        }

        public void AddBot()
        {

        }

        public void GetAllBots()
        {
            if (IsArchiSteamFarmExists && IsArchiSteamFarmRuning)
            {
                var botNames = Directory.EnumerateFiles(ArchiSteamFarmConfigDirPath!, "*" + ".json").Select(Path.GetFileNameWithoutExtension).Where(botName => !string.IsNullOrEmpty(botName));


            }
        }

        public void SaveBotConfig()
        {

        }

        protected void OnReportDownloading(float value) => OnReport(value, AppResources.Downloading.Format(MathF.Round(value, 2)));
        protected void OnReport(float value, string str)
        {
            ProgressValue = value;
            ProgressString = str;
        }

        float _ProgressValue;
        public float ProgressValue
        {
            get => _ProgressValue;
            private set => this.RaiseAndSetIfChanged(ref _ProgressValue, value);
        }

        string _ProgressString = string.Empty;
        public string ProgressString
        {
            get => _ProgressString;
            private set => this.RaiseAndSetIfChanged(ref _ProgressString, value);
        }

        public async void UpdateOrDownloadASF()
        {
            string targetFile = "ASF-" + Variant + ".zip";
            GithubReleaseModel.Assets? binaryAsset = null;
            if (IsArchiSteamFarmExists)
            {
                var releaseModel = await CheckUpdate();
                if (releaseModel != null && IsNewVerison)
                {
                    binaryAsset = releaseModel.assets.FirstOrDefault(asset => !string.IsNullOrEmpty(asset.name) && asset.name!.Equals(targetFile, StringComparison.OrdinalIgnoreCase));
                }
            }
            else
            {
                var result = await httpService.GetAsync<GithubReleaseModel>(GITHUB_LATEST_RELEASEAPI_URL);
                binaryAsset = result.assets.FirstOrDefault(asset => !string.IsNullOrEmpty(asset.name) && asset.name!.Equals(targetFile, StringComparison.OrdinalIgnoreCase));
            }

            if (binaryAsset?.browser_download_url == null)
            {
                Toast.Show("未获取到下载链接");
                return;
            }
            var downloadFilePath = IOPath.CacheDirectory + Path.DirectorySeparatorChar + targetFile;
            var download = await clientService.Download(true, binaryAsset?.browser_download_url, downloadFilePath, new Progress<float>(OnReportDownloading));

            if (download.IsSuccess)
            {
                var dirPath = Path.GetDirectoryName(ArchiSteamFarmExePath);
                if (string.IsNullOrEmpty(dirPath))
                {
                    dirPath = Path.Combine(IOPath.BaseDirectory, "ASF");
                }
                TarGZipHelper.Unpack(downloadFilePath, dirPath);
                Toast.Show("升级完成");
            }
            else // 下载失败，进度条填满，可能服务器崩了
            {
                Toast.Show("下载失败:" + binaryAsset?.browser_download_url);
            }
        }

        public async Task<GithubReleaseModel?> CheckUpdate()
        {
            NewVerison = await httpService.GetAsync<GithubReleaseModel>(GITHUB_LATEST_RELEASEAPI_URL);

            if (NewVerison == null)
            {
                return null;
            }

            IsNewVerison = (ArchiSteamFarmVersion >= NewVerison.version);

            if (IsNewVerison)
            {
                Toast.Show("ASF已是最新版本");
            }
            else
            {
                Toast.Show("检测到新版本" + NewVerison.version);
            }

            return NewVerison;
        }
    }
}
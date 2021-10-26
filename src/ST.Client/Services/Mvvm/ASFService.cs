using ArchiSteamFarm;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Storage;
using DynamicData;
using ReactiveUI;
using System;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    public sealed class ASFService : ReactiveObject
    {
        static ASFService? mCurrent;
        public static ASFService Current => mCurrent ?? new();

        readonly IArchiSteamFarmService archiSteamFarmService = IArchiSteamFarmService.Instance;

        string? _IPCUrl;
        public string? IPCUrl
        {
            get => _IPCUrl;
            set => this.RaiseAndSetIfChanged(ref _IPCUrl, value);
        }

        string? _ConsoleLogText;
        public string? ConsoleLogText
        {
            get => _ConsoleLogText;
            set => this.RaiseAndSetIfChanged(ref _ConsoleLogText, value);
        }

        public IConsoleBuilder ConsoleLogBuilder { get; } = new ConsoleBuilder()
        {
            MaxLine = ASFSettings.ConsoleMaxLine,
        };

        public SourceCache<Bot, string> SteamBotsSourceList;

        public bool IsASFRuning => archiSteamFarmService.StartTime != null;

        GlobalConfig? _GlobalConfig;
        public GlobalConfig? GlobalConfig
        {
            get => _GlobalConfig;
            set => this.RaiseAndSetIfChanged(ref _GlobalConfig, value);
        }

        public ASFService()
        {
            mCurrent = this;

            SteamBotsSourceList = new SourceCache<Bot, string>(t => t.BotName);

            archiSteamFarmService.OnConsoleWirteLine += OnConsoleWirteLine;

            //InitASF();
        }

        void OnConsoleWirteLine(string message)
        {
            ConsoleLogBuilder.AppendLine(message);
            ConsoleLogText = ConsoleLogBuilder.ToString();
        }

        public async void InitASF()
        {
            await archiSteamFarmService.Start();

            RefreshBots();

            IPCUrl = archiSteamFarmService.GetIPCUrl();

            this.RaisePropertyChanged(nameof(IsASFRuning));
        }

        public async void StopASF()
        {
            await archiSteamFarmService.Stop();

            this.RaisePropertyChanged(nameof(IsASFRuning));
        }

        public void RefreshBots()
        {
            SteamBotsSourceList.Clear();

            var bots = archiSteamFarmService.GetReadOnlyAllBots();
            if (bots.Any_Nullable())
                SteamBotsSourceList.AddOrUpdate(bots!.Values);
        }

        public void RefreshConfig()
        {
            GlobalConfig = archiSteamFarmService.GetGlobalConfig();
        }

        public async void ImportBotFiles(IEnumerable<string> files)
        {
            var num = 0;
            foreach (var filename in files)
            {
                var file = new FileInfo(filename);
                if (file.Exists)
                {
                    var bot = await BotConfig.Load(file.FullName).ConfigureAwait(false);
                    if (bot.BotConfig != null)
                    {
                        try
                        {
                            file.CopyTo(Path.Combine(SharedInfo.ConfigDirectory, file.Name), true);
                            num++;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(nameof(ASFService), ex, nameof(ImportBotFiles));
                        }
                    }
                }
            }
            Toast.Show(string.Format(AppResources.LocalAuth_ImportSuccessTip, num));
        }
    }
}

using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Storage;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
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

            //InitASF();
        }

        public async void InitASF()
        {
            IArchiSteamFarmService.Instance.GetConsoleWirteFunc = (message) =>
            {
                if (!string.IsNullOrEmpty(ConsoleLogText))
                {
                    var lines = ConsoleLogText.Split(Environment.NewLine, StringSplitOptions.None).ToList();
                    if (lines.Count >= 200)
                    {
                        lines.RemoveAt(0);
                        lines.Add(message);
                        ConsoleLogText = string.Join(Environment.NewLine, lines);
                        return;
                    }
                }
                ConsoleLogText += Environment.NewLine + message;
            };

            await IArchiSteamFarmService.Instance.Start();

            RefreshBots();

            IPCUrl = archiSteamFarmService.GetIPCUrl();

            this.RaisePropertyChanged(nameof(IsASFRuning));
        }

        public async void StopASF()
        {
            await IArchiSteamFarmService.Instance.Stop();

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

        public void OpenWebUIConsole()
        {
            Browser2.Open(archiSteamFarmService.GetIPCUrl());
        }
    }
}

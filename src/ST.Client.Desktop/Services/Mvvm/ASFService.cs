using ArchiSteamFarm.Steam;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public class ASFService : ReactiveObject
    {
        #region static members
        public static ASFService Current { get; } = new();
        #endregion

        private readonly IArchiSteamFarmService archiSteamFarmService = IArchiSteamFarmService.Instance;

        bool _IPCUrl;
        public bool IPCUrl
        {
            get => _IPCUrl;
            set => this.RaiseAndSetIfChanged(ref _IPCUrl, value);
        }

        string? _CommandText;
        public string? CommandText
        {
            get => _CommandText;
            set => this.RaiseAndSetIfChanged(ref _CommandText, value);
        }

        public SourceCache<Bot, string> SteamBotsSourceList;


        public ASFService()
        {
            SteamBotsSourceList = new SourceCache<Bot, string>(t => t.BotName);

            InitASF();
        }

        public async void InitASF()
        {
            await IArchiSteamFarmService.Instance.Start();

            var bots = archiSteamFarmService.GetReadOnlyAllBots();
            if (bots.Any_Nullable())
                SteamBotsSourceList.AddOrUpdate(bots!.Values);
        }


        public async void StopASF()
        {
            await IArchiSteamFarmService.Instance.Stop();
        }
    }
}

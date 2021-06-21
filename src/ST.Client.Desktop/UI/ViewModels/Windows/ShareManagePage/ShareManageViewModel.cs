using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class ShareManageViewModel : WindowViewModel
    {
        readonly ISteamService steamService = DI.Get<ISteamService>();
        readonly IHttpService httpService = DI.Get<IHttpService>();
        readonly ISteamworksWebApiService webApiService = DI.Get<ISteamworksWebApiService>();
        public ShareManageViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " q| " + AppResources.GameList_HideGameManger;
            this.WhenAnyValue(x => x.AuthorizedList)
                .Subscribe(s => this.RaisePropertyChanged(nameof(IsAuthorizedListEmpty)));

            _AuthorizedSourceList = new SourceCache<AuthorizedDevice, long>(t => t.SteamId3_Int);
            _AuthorizedSourceList
             .Connect()
             .ObserveOn(RxApp.MainThreadScheduler)
             .Sort(SortExpressionComparer<AuthorizedDevice>.Descending(x => x.Timeused))
             .Bind(out _AuthorizedList)
             .Subscribe();

            Refresh_Click();
        }
        public bool _IsAuthorizedListEmpty;
        public bool IsAuthorizedListEmpty
        {
            get => _IsAuthorizedListEmpty;
            set => this.RaiseAndSetIfChanged(ref _IsAuthorizedListEmpty, value);
        }

        private ReadOnlyObservableCollection<AuthorizedDevice>? _AuthorizedList;
        public ReadOnlyObservableCollection<AuthorizedDevice>? AuthorizedList => _AuthorizedList;

        public SourceCache< AuthorizedDevice, long> _AuthorizedSourceList;
        public SourceCache<AuthorizedDevice, long> AuthorizedSourceList
        {
            get => _AuthorizedSourceList;
            set => this.RaiseAndSetIfChanged(ref _AuthorizedSourceList, value);
        } 
        //private string? _SearchText;
        //public string? SearchText
        //{
        //    get => _SearchText;
        //    set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        //}

        public void Refresh_Click()
        {
            var list = new List<AuthorizedDevice>();

            var userlist = steamService.GetRememberUserList();
            var id = string.Empty;
            foreach (var item in steamService.GetAuthorizedDeviceList())
            { 
                var user = userlist.FirstOrDefault(x => x.SteamId3_Int == item.SteamId3_Int);
                item.SteamNickName = user?.SteamNickName;
                item.ShowName = item.SteamNickName + $"({item.SteamId64_Int})";
                item.AccountName = user?.AccountName;
                list.Add( item);
            }
            if (list.Count == 0)
                IsAuthorizedListEmpty = true;
            else
                IsAuthorizedListEmpty = false;
            _AuthorizedSourceList.AddOrUpdate(list); 
            _AuthorizedSourceList.Refresh();
            Refresh_Cash().ConfigureAwait(false);
        }
        public async Task Refresh_Cash() {

            var accountRemarks = Serializable.Clone<IReadOnlyDictionary<long, string?>?>(SteamAccountSettings.AccountRemarks.Value);
            foreach (var user in _AuthorizedSourceList.Items)
            {
                string? remark = null;
                var temp = await webApiService.GetUserInfo(user.SteamId64_Int);
                accountRemarks?.TryGetValue(user.SteamId64_Int, out remark);
                user.Remark = remark;
                user.SteamID = temp.SteamID;
                user.OnlineState = temp.OnlineState;
                user.SteamNickName = temp.SteamNickName;
                user.AvatarIcon = temp.AvatarIcon;
                user.AvatarMedium = temp.AvatarMedium;
                user.AvatarStream = httpService.GetImageAsync(temp.AvatarFull, ImageChannelType.SteamAvatars);
            }
            _AuthorizedSourceList.Refresh();
        }
        public void SteamId_Click(AuthorizedDevice user)
        {

        }
        public void DeleteButton_Click(AuthorizedDevice user)
        {

        }

        
        public void SetActivity_Click(AuthorizedDevice item)
        {
            steamService.UpdateAuthorizedDeviceList(item);
            Refresh_Click(); 
        }
    }
}
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
using System.Windows;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public class ShareManageViewModel : WindowViewModel
    {
        readonly ISteamService steamService = DI.Get<ISteamService>();
        readonly IHttpService httpService = DI.Get<IHttpService>();
        readonly ISteamworksWebApiService webApiService = DI.Get<ISteamworksWebApiService>();
        public ShareManageViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.AccountChange_Title;
            this.WhenAnyValue(x => x.AuthorizedList)
                .Subscribe(s => this.RaisePropertyChanged(nameof(IsAuthorizedListEmpty)));

            _AuthorizedSourceList = new SourceCache<AuthorizedDevice, long>(t => t.SteamId3_Int);
            _AuthorizedSourceList
             .Connect()
             .ObserveOn(RxApp.MainThreadScheduler)
             .Sort(SortExpressionComparer<AuthorizedDevice>.Ascending(x => x.Index))
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

        public SourceCache<AuthorizedDevice, long> _AuthorizedSourceList;
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
            var allList = steamService.GetAuthorizedDeviceList();
            int count = allList.Count - 1;
            foreach (var item in allList)
            {
                var temp = userlist.FirstOrDefault(x => x.SteamId3_Int == item.SteamId3_Int);
                item.SteamNickName = temp?.SteamNickName;
                item.ShowName = item.SteamNickName + $"({item.SteamId64_Int})";
                item.AccountName = temp?.AccountName;
                item.First = item.Index == 0;
                item.End = item.Index == count;
                list.Add(item);
            }
            if (list.Count == 0)
                IsAuthorizedListEmpty = true;
            else
                IsAuthorizedListEmpty = false;
            _AuthorizedSourceList.AddOrUpdate(list);
            _AuthorizedSourceList.Refresh();
            Refresh_Cash().ConfigureAwait(false);
        }
        public async Task Refresh_Cash()
        {

            var accountRemarks = Serializable.Clone<IReadOnlyDictionary<long, string?>?>(SteamAccountSettings.AccountRemarks.Value);

            foreach (var item in _AuthorizedSourceList.Items)
            {
                string? remark = null;
                var temp = await webApiService.GetUserInfo(item.SteamId64_Int);
                accountRemarks?.TryGetValue(item.SteamId64_Int, out remark);
                item.Remark = remark;
                item.SteamID = temp.SteamID;
                item.OnlineState = temp.OnlineState?? "offline";
                item.SteamNickName = temp.SteamNickName ?? item.AccountName ?? item.SteamId3_Int.ToString();
                item.AvatarIcon = temp.AvatarIcon;
                item.AvatarMedium = temp.AvatarMedium;
                item.AvatarStream = httpService.GetImageAsync(temp.AvatarFull, ImageChannelType.SteamAvatars);
            }

            _AuthorizedSourceList.AddOrUpdate(_AuthorizedSourceList.Items);
            _AuthorizedSourceList.Refresh();
            foreach (var item in _AuthorizedSourceList.Items)
            {
                item.MiniProfile = await webApiService.GetUserMiniProfile(item.SteamId3_Int);
                var miniProfile = item.MiniProfile;
                if (miniProfile != null)
                {
                    if (!string.IsNullOrEmpty(miniProfile.AnimatedAvatar))
                        item.AvatarStream = httpService.GetImageAsync(miniProfile.AnimatedAvatar, ImageChannelType.SteamAvatars);
                    //miniProfile.AvatarFrameStream = httpService.GetImageAsync(miniProfile.AvatarFrame, ImageChannelType.SteamAvatars);
                }
            }
            _AuthorizedSourceList.AddOrUpdate(_AuthorizedSourceList.Items);
            _AuthorizedSourceList.Refresh();
        }
        public void OpenUserProfileUrl(AuthorizedDevice user)
        {
            BrowserOpen(user.ProfileUrl);
        }
          public void About_Click()
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.AccountChange_ShareManageAboutTips, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OK).ContinueWith(async (s) =>
            { 
            }); 
        }
        public void SetFirstButton_Click(AuthorizedDevice item)
        {
            item.Index = 1;
            Sort(item, true);
        }
        public void UpButton_Click(AuthorizedDevice item)
        {
            Sort(item, true);
        }
        public void DowButton_Click(AuthorizedDevice item)
        {
            Sort(item, false);
        }
        public void Sort(AuthorizedDevice item, bool up)
        {
            var index = item.Index;
            int count = _AuthorizedSourceList.Count - 1;
            if (up ? item.Index != 0 : item.Index != count)
            {
                _AuthorizedSourceList.AddOrUpdate(_AuthorizedSourceList.Items.Select(x =>
                {
                    if (up ? x.Index == index - 1 : x.Index == index + 1)
                        x.Index = up ? x.Index + 1 : x.Index - 1;
                    if (x.SteamId3_Int == item.SteamId3_Int)
                        x.Index = up ? item.Index - 1 : item.Index + 1;

                    x.First = x.Index == 0;
                    x.End = x.Index == count;
                    return x;
                }).ToList());
            }
        }
        public void SetActivity_Click()
        {
            if (_AuthorizedList != null)
                steamService.UpdateAuthorizedDeviceList(_AuthorizedList);
            var result = MessageBoxCompat.ShowAsync(@AppResources.AccountChange_RestartSteam, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    steamService.TryKillSteamProcess();
                    steamService.StartSteam();
                }
            });
        }
    }
}
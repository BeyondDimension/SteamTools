using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class ShareManageViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.AccountChange_Title;

        readonly ISteamService steamService = DI.Get<ISteamService>();
        readonly IHttpService httpService = DI.Get<IHttpService>();
        readonly ISteamworksWebApiService webApiService = DI.Get<ISteamworksWebApiService>();

        public ShareManageViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);

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

        public bool IsAuthorizedListEmpty => !AuthorizedList.Any_Nullable();

        readonly ReadOnlyObservableCollection<AuthorizedDevice> _AuthorizedList;

        public ReadOnlyObservableCollection<AuthorizedDevice> AuthorizedList => _AuthorizedList;

        readonly SourceCache<AuthorizedDevice, long> _AuthorizedSourceList;

        public void Refresh_Click()
        {
            var list = new List<AuthorizedDevice>();

            var userlist = steamService.GetRememberUserList();
            var allList = steamService.GetAuthorizedDeviceList();
            allList.Add(SteamAccountSettings.DisableAuthorizedDevice.Value!.Select(x => new AuthorizedDevice
            {
                SteamId3_Int = x.SteamId3_Int,
                Timeused = x.Timeused,
                Description = x.Description,
                Disable = true,
                Tokenid = x.Tokenid,
            }));
            foreach (var item in allList)
            {
                var temp = userlist.FirstOrDefault(x => x.SteamId32 == item.SteamId3_Int);
                item.SteamNickName = temp?.SteamNickName;
                item.ShowName = $"{item.SteamNickName}({item.SteamId64_Int})";
                item.AccountName = temp?.AccountName;
                list.Add(item);
            }
            _AuthorizedSourceList.Clear();
            _AuthorizedSourceList.AddOrUpdate(list);
            //_AuthorizedSourceList.Refresh();

            Refresh_Cash().ConfigureAwait(false);
        }

        public async Task Refresh_Cash()
        {
            IReadOnlyDictionary<long, string?>? accountRemarks = SteamAccountSettings.AccountRemarks.Value;

            foreach (var item in _AuthorizedSourceList.Items)
            {
                var temp = await webApiService.GetUserInfo(item.SteamId64_Int);
                item.SteamID = temp.SteamID;
                item.SteamNickName = temp.SteamNickName ?? item.AccountName ?? item.SteamId3_Int.ToString();
                item.AvatarIcon = temp.AvatarIcon;
                item.AvatarMedium = temp.AvatarMedium;
                item.MiniProfile = temp.MiniProfile;
                if (item.MiniProfile != null && !string.IsNullOrEmpty(item.MiniProfile.AnimatedAvatar))
                {
                    item.AvatarStream = httpService.GetImageAsync(item.MiniProfile.AnimatedAvatar, ImageChannelType.SteamAvatars);
                }
                else
                {
                    item.AvatarStream = httpService.GetImageAsync(temp.AvatarFull, ImageChannelType.SteamAvatars);
                }

                if (accountRemarks?.TryGetValue(item.SteamId64_Int, out var remark) == true &&
                     !string.IsNullOrEmpty(remark))
                    item.Remark = remark;
            }

            _AuthorizedSourceList.Refresh();

            //foreach (var item in _AuthorizedSourceList.Items)
            //{
            //    item.MiniProfile = await webApiService.GetUserMiniProfile(item.SteamId3_Int);
            //    var miniProfile = item.MiniProfile;
            //    if (miniProfile != null)
            //    {
            //        if (!string.IsNullOrEmpty(miniProfile.AnimatedAvatar))
            //            item.AvatarStream = httpService.GetImageAsync(miniProfile.AnimatedAvatar, ImageChannelType.SteamAvatars);
            //        //miniProfile.AvatarFrameStream = httpService.GetImageAsync(miniProfile.AvatarFrame, ImageChannelType.SteamAvatars);
            //    }
            //}
            //_AuthorizedSourceList.Refresh();
        }

        public async void OpenUserProfileUrl(AuthorizedDevice user)
        {
            await Browser2.OpenAsync(user.ProfileUrl);
        }

        public async void About_Click()
        {
            await MessageBox.ShowAsync(AppResources.AccountChange_ShareManageAboutTips, button: MessageBox.Button.OK);
        }

        public void SetFirstButton_Click(AuthorizedDevice item)
        {
            if (item.Index != 0)
            {
                item.Index = -1;
                _AuthorizedSourceList.Refresh(item);
                for (var i = 0; i < AuthorizedList.Count; i++)
                {
                    _AuthorizedSourceList.Lookup(AuthorizedList[i].SteamId3_Int).Value.Index = i;
                }
            }
        }

        public async void RemoveButton_Click(AuthorizedDevice item)
        {
            var result = await MessageBox.ShowAsync(AppResources.Steam_Share_RemoveShare, button: MessageBox.Button.OKCancel);
            if (result.IsOK())
            {
                _AuthorizedSourceList.Remove(item);
            }
        }

        public void DisableOrEnableButton_Click(AuthorizedDevice item)
        {
            item.Disable = !item.Disable;
            _AuthorizedSourceList.AddOrUpdate(_AuthorizedList!);
        }

        public void UpButton_Click(AuthorizedDevice item)
        {
            Sort(item, true);
        }

        public void DownButton_Click(AuthorizedDevice item)
        {
            Sort(item, false);
        }

        private void Sort(AuthorizedDevice item, bool up)
        {
            var index = item.Index;
            if (up ? item.Index != 0 : item.Index != _AuthorizedSourceList.Count - 1)
            {
                var dest = AuthorizedList[up ? index - 1 : index + 1];

                item.Index = dest.Index;
                dest.Index = index;

                _AuthorizedSourceList.Refresh(item);
                _AuthorizedSourceList.Refresh(dest);
            }
        }

        public async void SetActivity_Click()
        {
            if (_AuthorizedList != null)
                steamService.UpdateAuthorizedDeviceList(_AuthorizedList.Where(x => !x.Disable));
            SteamAccountSettings.DisableAuthorizedDevice.Value = _AuthorizedList.Where(x => x.Disable).Select(x => new DisableAuthorizedDevice
            {
                Description = x.Description,
                SteamId3_Int = x.SteamId3_Int,
                Timeused = x.Timeused,
                Tokenid = x.Tokenid,
            }).ToArray();
            var result = await MessageBox.ShowAsync(AppResources.AccountChange_RestartSteam, button: MessageBox.Button.OKCancel);
            if (result.IsOK())
            {
                steamService.TryKillSteamProcess();
                steamService.StartSteamWithParameter();
            }
        }
    }
}
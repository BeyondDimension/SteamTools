using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamFamilyShareManagePageViewModel : WindowViewModel
{
    public static string DisplayName => Strings.AccountChange_Title;

    readonly ISteamService steamService = Ioc.Get<ISteamService>();
    readonly ISteamworksWebApiService webApiService = Ioc.Get<ISteamworksWebApiService>();

    public SteamFamilyShareManagePageViewModel()
    {
        Title = DisplayName;

        //_AuthorizedSourceList = new SourceCache<AuthorizedDevice, long>(t => t.SteamId3_Int);
        //_AuthorizedSourceList
        // .Connect()
        // .ObserveOn(RxApp.MainThreadScheduler)
        // .Sort(SortExpressionComparer<AuthorizedDevice>.Ascending(x => x.Index))
        // .Bind(out _AuthorizedList)
        // .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAuthorizedListEmpty)));

        AuthorizedList = new ObservableCollection<AuthorizedDevice>();

        this.WhenValueChanged(x => x.AuthorizedList)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAuthorizedListEmpty)));

        OpenUserProfileUrl_Click = ReactiveCommand.Create<AuthorizedDevice>(OpenUserProfileUrl);

        RemoveButton_Click = ReactiveCommand.Create<AuthorizedDevice>(RemoveButton);

        //SetFirstButton_Click = ReactiveCommand.Create<AuthorizedDevice>(SetFirstButton);

        //UpButton_Click = ReactiveCommand.Create<AuthorizedDevice>(UpButton);

        //DownButton_Click = ReactiveCommand.Create<AuthorizedDevice>(DownButton);

        Refresh_Click();
    }

    public bool IsAuthorizedListEmpty => !AuthorizedList.Any_Nullable();

    //readonly ReadOnlyObservableCollection<AuthorizedDevice> _AuthorizedList;
    [Reactive]
    public ObservableCollection<AuthorizedDevice> AuthorizedList { get; set; }

    //readonly SourceCache<AuthorizedDevice, long> _AuthorizedSourceList;

    public void Refresh_Click()
    {
        var list = new List<AuthorizedDevice>();

        var userlist = steamService.GetRememberUserList();
        var allList = steamService.GetAuthorizedDeviceList();
        allList.Add(GameAccountSettings.DisableAuthorizedDevice.Value!.Select(x => new AuthorizedDevice
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
            item.AvatarMedium = temp?.AvatarMedium;
            list.Add(item);
        }
        AuthorizedList.Clear();
        AuthorizedList.Add(list.OrderBy(x => x.Index));

        Refresh_Cash();
    }

    public async void Refresh_Cash()
    {
        var accountRemarks = GameAccountSettings.AccountRemarks.Value;

        foreach (var item in AuthorizedList)
        {
            var temp = await webApiService.GetUserInfo(item.SteamId64_Int);
            item.SteamID = temp.SteamID;
            //item.SteamNickName = temp.SteamNickName ?? item.AccountName ?? item.SteamId3_Int.ToString();
            item.AvatarIcon = temp.AvatarIcon;
            item.AvatarMedium = temp.AvatarMedium;
            //item.MiniProfile = temp.MiniProfile;

            //if (item.MiniProfile != null && !string.IsNullOrEmpty(item.MiniProfile.AnimatedAvatar))
            //{
            //    item.AvatarMedium = item.MiniProfile.AnimatedAvatar;
            //}

            if (accountRemarks?.TryGetValue("Steam-" + item.SteamId64_Int, out var remark) == true &&
                 !string.IsNullOrEmpty(remark))
                item.Remark = remark;
        }

        //AuthorizedList.Refresh();
    }

    public async void OpenUserProfileUrl(AuthorizedDevice user)
    {
        await Browser2.OpenAsync(user.ProfileUrl);
    }

    public async void About_Click()
    {
        await MessageBox.ShowAsync(Strings.AccountChange_ShareManageAboutTips, button: MessageBox.Button.OK);
    }

    //public void SetFirstButton(AuthorizedDevice item)
    //{
    //    if (item.Index != 0)
    //    {
    //        item.Index = -1;
    //        _AuthorizedSourceList.Refresh(item);
    //        for (var i = 0; i < AuthorizedList.Count; i++)
    //        {
    //            _AuthorizedSourceList.Lookup(AuthorizedList[i].SteamId3_Int).Value.Index = i;
    //        }
    //    }
    //}

    public async void RemoveButton(AuthorizedDevice item)
    {
        var result = await MessageBox.ShowAsync(Strings.Steam_Share_RemoveShare, button: MessageBox.Button.OKCancel);
        if (result.IsOK())
        {
            AuthorizedList.Remove(item);
        }
    }

    //public void UpButton(AuthorizedDevice item)
    //{
    //    Sort(item, true);
    //}

    //public void DownButton(AuthorizedDevice item)
    //{
    //    Sort(item, false);
    //}

    //private void Sort(AuthorizedDevice item, bool up)
    //{
    //    var index = item.Index;
    //    if (up ? item.Index != 0 : item.Index != _AuthorizedSourceList.Count - 1)
    //    {
    //        var dest = AuthorizedList[up ? index - 1 : index + 1];

    //        item.Index = dest.Index;
    //        dest.Index = index;

    //        _AuthorizedSourceList.Refresh(item);
    //        _AuthorizedSourceList.Refresh(dest);
    //    }
    //}

    public async void SetActivity_Click()
    {
        if (AuthorizedList != null)
        {
            for (var i = 0; i < AuthorizedList.Count; i++)
            {
                AuthorizedList[i].Index = i;
            }
            steamService.UpdateAuthorizedDeviceList(AuthorizedList.Where(x => !x.Disable));

            GameAccountSettings.DisableAuthorizedDevice.Value = AuthorizedList!.Where(x => x.Disable).Select(x => new DisableAuthorizedDevice
            {
                Description = x.Description,
                SteamId3_Int = x.SteamId3_Int,
                Timeused = x.Timeused,
                Tokenid = x.Tokenid,
            }).ToArray();

            var result = await MessageBox.ShowAsync(Strings.AccountChange_RestartSteam, button: MessageBox.Button.OKCancel);
            if (result.IsOK())
            {
                await steamService.TryKillSteamProcess();
                steamService.StartSteamWithParameter();
            }
        }
    }
}

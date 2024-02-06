using System.Collections.Specialized;

namespace BD.WTTS.UI.ViewModels;

public partial class BotPageViewModel : ViewModelBase
{
    private readonly IArchiSteamFarmService asfService = IArchiSteamFarmService.Instance;

    public BotPageViewModel()
    {
        AddBot = ReactiveCommand.Create(() => ASFService.Current.OpenBrowser(ActionItem.WebAddBot.ToString()));

        RefreshBots = ReactiveCommand.Create(ASFService.Current.RefreshBots);

        SelectBotFiles = ReactiveCommand.CreateFromTask(SelectBotFiles_Click);

        PauseOrResumeBotFarming = ReactiveCommand.Create<Bot>(PauseOrResumeBotFarming_Click);

        EnableOrDisableBot = ReactiveCommand.Create<Bot>(EnableOrDisableBot_Click);

        GoToBotSettings = ReactiveCommand.Create<Bot>(GoToBotSettings_Click);

        DeleteBot = ReactiveCommand.Create<Bot>(DeleteBot_Click);

        EditBotFile = ReactiveCommand.Create<Bot>(EditBotFile_Click);

        ASFService.Current.SteamBotsSourceList
          .Connect()
          .ObserveOn(RxApp.MainThreadScheduler)
          .Sort(SortExpressionComparer<BotViewModel>.Descending(x => x.Bot.BotName))
          .Bind(out _SteamBots)
          .Subscribe();
    }

    public async Task SelectBotFiles_Click()
    {
        FilePickerFileType? fileTypes;
        if (IApplication.IsDesktop())
        {
            fileTypes = new ValueTuple<string, string[]>[]
            {
                ("Json Files", new[] { FileEx.JSON, }),
                //("All Files", new[] { "*", }),
            };
        }
        else if (OperatingSystem2.IsAndroid())
        {
            fileTypes = new[] { MediaTypeNames.JSON };
        }
        else
        {
            fileTypes = null;
        }
        await FilePicker2.PickMultipleAsync(ASFService.Current.ImportBotFiles, fileTypes);
    }

    public async void PauseOrResumeBotFarming_Click(Bot bot)
    {
        IApiRsp apiRsp = null;
        if (bot.CardsFarmer.Paused)
        {
            apiRsp = await asfService.BotResumeAsync(bot.BotName);
        }
        else
        {
            var request = new BotPauseRequest() { Permanent = true };
            apiRsp = await asfService.BotPauseAsync(bot.BotName, request);
        }

        if (apiRsp.IsSuccess)
            Toast.Show(ToastIcon.Success, apiRsp.Message);
        else
            Toast.Show(ToastIcon.Error, string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, apiRsp.Message));

        ASFService.Current.RefreshBots();
    }

    public async void EnableOrDisableBot_Click(Bot bot)
    {
        IApiRsp apiRsp;
        if (bot.KeepRunning)
            apiRsp = await asfService.BotStopAsync(bot.BotName);
        else
            apiRsp = await asfService.BotStartAsync(bot.BotName);

        if (!apiRsp.IsSuccess)
            Toast.Show(ToastIcon.Error, string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, apiRsp.Message));
    }

    public async Task<(IReadOnlyDictionary<string, string>? UnusedKeys, IReadOnlyDictionary<string, string>? UsedKeys)> GetUsedAndUnusedKeys_Click(Bot bot)
    {
        var r = await asfService.BotGetUsedAndUnusedKeysAsync(bot.BotName);
        if (r.IsSuccess && r.Content.TryGetValue(bot.BotName, out var response))
            return (response.UnusedKeys, response.UsedKeys);
        return default;
    }

    public async void RedeemKeyBot_Click(Bot bot, IOrderedDictionary keys)
    {
        var request = new BotGamesToRedeemInBackgroundRequest() { GamesToRedeemInBackground = (OrderedDictionary)keys };
        await asfService.BotRedeemKeyAsync(bot.BotName, request);
    }

    public async Task<bool> ResetBotRedeemedKeysRecord_Click(Bot bot)
    {
        var r = await asfService.BotResetRedeemedKeysRecordAsync(bot.BotName);
        return r.IsSuccess;
    }

    public async void GoToBotSettings_Click(Bot bot)
    {
        await Browser2.OpenAsync(ASFService.Current.IPCUrl + "/bot/" + bot.BotName + "/config", BrowserLaunchMode.External);
    }

    public void EditBotFile_Click(Bot bot)
    {
        var filePath = Path.Combine(SharedInfo.HomeDirectory, SharedInfo.ConfigDirectory, $"{bot.BotName}{SharedInfo.JsonConfigExtension}");
        IPlatformService.Instance.OpenFileByTextReader(filePath);
    }

    public async void DeleteBot_Click(Bot bot)
    {
        var s = await MessageBox.ShowAsync(BDStrings.ASF_DeleteBotTip, button: MessageBox.Button.OKCancel);
        if (s == MessageBox.Result.OK)
        {
            var result = await asfService.BotDeleteAsync(bot.BotName);
            if (result.IsSuccess)
            {
                ASFService.Current.SteamBotsSourceList.Remove(bot);
                Toast.Show(ToastIcon.Success, BDStrings.ASF_DeleteBotSuccess);
            }
        }
    }

    public async void ShowAddBotWindow()
    {
        await IWindowManager.Instance.ShowAsync(AppEndPoint.ASF_AddBot, resizeMode: ResizeMode.CanResize);
    }
}

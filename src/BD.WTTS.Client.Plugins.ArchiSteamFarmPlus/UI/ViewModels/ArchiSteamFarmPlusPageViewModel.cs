#if !EXCLUDE_ASF
using ArchiSteamFarm.Steam;
using System.Collections.Specialized;
using Avalonia.Automation;
using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;
using BD.SteamClient.Services;
using BD.WTTS.UI.Views.Pages;
using System.Linq;
using ArchiSteamFarm.Core;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class ArchiSteamFarmPlusPageViewModel : ViewModelBase
{
    private readonly IArchiSteamFarmService asfService = IArchiSteamFarmService.Instance;

    public ArchiSteamFarmPlusPageViewModel()
    {
        SelectBotFiles = ReactiveCommand.CreateFromTask(SelectBotFiles_Click);

        SelectGlobalFiles = ReactiveCommand.CreateFromTask(SelectGlobalFiles_Click);

        SelectASFExePath = ReactiveCommand.CreateFromTask(SelectASFProgramLocation);

        OpenWebUIConsole = ReactiveCommand.Create(() =>
        {
            OpenBrowser(null);
        });

        OpenASFBrowser = ReactiveCommand.Create<string>(OpenBrowser);

        RunOrStop = ReactiveCommand.Create(RunOrStopASF);

        AddBot = ReactiveCommand.Create(ShowAddBotWindow);

        RefreshBots = ReactiveCommand.Create(ASFService.Current.RefreshBots);

        OpenASFFolder = ReactiveCommand.Create<string>(OpenFolder);

        SetEncryptionKey = ReactiveCommand.Create(SetEncryptionKey_Click);

        ASFService.Current.WhenValueChanged(x => x.IsASFRuning)
            .Subscribe(x =>
            {
                RunOrStopText = x ? BDStrings.ASF_Stop : BDStrings.ASF_Start;
            });

        ASFService.Current.SteamBotsSourceList
                  .Connect()
                  .ObserveOn(RxApp.MainThreadScheduler)
                  .Sort(SortExpressionComparer<Bot>.Descending(x => x.BotName))
                  .Bind(out _SteamBots)
                  .Subscribe();

        //if (IApplication.IsDesktopPlatform)
        //{
        //    ConsoleSelectFont = R.Fonts.FirstOrDefault(x => x.Value == ASFSettings.ConsoleFontName.Value);
        //    this.WhenValueChanged(x => x.ConsoleSelectFont, false)
        //          .Subscribe(x => ASFSettings.ConsoleFontName.Value = x.Value);
        //}
    }

    public override void Activation()
    {
        base.Activation();

        if (ASFSettings.AutoRunArchiSteamFarm.Value && !string.IsNullOrEmpty(ASFSettings.ArchiSteamFarmExePath.Value))
            StartOrStopASF_Click();
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

    public async Task SelectGlobalFiles_Click()
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
        await FilePicker2.PickAsync(ASFService.Current.ImportGlobalFiles, fileTypes);
    }

    public async Task SelectASFProgramLocation()
    {
        AvaloniaFilePickerFileTypeFilter fileTypes = new AvaloniaFilePickerFileTypeFilter.Item[] {
            new("ArchiSteamFarm") {
                Patterns = new[] { "ArchiSteamFarm.exe", },
                //MimeTypes =
                //AppleUniformTypeIdentifiers =
                },
        };
        await FilePicker2.PickAsync((path) =>
        {
            if (!string.IsNullOrEmpty(path))
                ASFSettings.ArchiSteamFarmExePath.Value = path;
        }, fileTypes);
    }

    public static void StartOrStopASF_Click(bool? startOrStop = null) => Task.Run(async () =>
    {
        var s = ASFService.Current;
        if (!s.IsASFRuning)
        {
            if (!startOrStop.HasValue || startOrStop.Value)
                await s.InitASFAsync();
        }
        else
        {
            if (!startOrStop.HasValue || !startOrStop.Value)
                await s.StopASFAsync();
        }
    });

    public void RunOrStopASF() => StartOrStopASF_Click();

    public async void ShowAddBotWindow()
    {
        await IWindowManager.Instance.ShowAsync(AppEndPoint.ASF_AddBot, resizeMode: ResizeMode.CanResize);
    }

    public async void PauseOrResumeBotFarming(Bot bot)
    {
        (bool success, string message) result;
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
        result = (apiRsp.IsSuccess, apiRsp.Message);

        ASFService.Current.SteamBotsSourceList.AddOrUpdate(bot);

        if (result.success)
            Toast.Show(ToastIcon.Success, result.message);
        else
            Toast.Show(ToastIcon.Error, string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, result.message));
    }

    public async void EnableOrDisableBot(Bot bot)
    {
        (bool success, string message) result;
        IApiRsp apiRsp = null;
        if (bot.KeepRunning)
            apiRsp = await asfService.BotStopAsync(bot.BotName);
        else
            apiRsp = await asfService.BotStartAsync(bot.BotName);
        result = (apiRsp.IsSuccess, apiRsp.Message);

        if (!result.success)
            Toast.Show(ToastIcon.Error, string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, result.message));

        ASFService.Current.SteamBotsSourceList.AddOrUpdate(bot);
    }

    public async Task<(IReadOnlyDictionary<string, string>? UnusedKeys, IReadOnlyDictionary<string, string>? UsedKeys)> GetUsedAndUnusedKeys(Bot bot)
    {
        var r = await asfService.BotGetUsedAndUnusedKeysAsync(bot.BotName);
        if (r.IsSuccess && r.Content.TryGetValue(bot.BotName, out var response))
            return (response.UnusedKeys, response.UsedKeys);
        return default;
    }

    public async void RedeemKeyBot(Bot bot, IOrderedDictionary keys)
    {
        var request = new BotGamesToRedeemInBackgroundRequest() { GamesToRedeemInBackground = (OrderedDictionary)keys };
        await asfService.BotRedeemKeyAsync(bot.BotName, request);
    }

    public async Task<bool> ResetBotRedeemedKeysRecord(Bot bot)
    {
        var r = await asfService.BotResetRedeemedKeysRecordAsync(bot.BotName);
        return r.IsSuccess;
    }

    public async void GoToBotSettings(Bot bot)
    {
        await Browser2.OpenAsync(IPCUrl + "/bot/" + bot.BotName + "/config", BrowserLaunchMode.External);
    }

    public void EditBotFile(Bot bot)
    {
        var filePath = Path.Combine(SharedInfo.HomeDirectory, SharedInfo.ConfigDirectory, $"{bot.BotName}{SharedInfo.JsonConfigExtension}");
        IPlatformService.Instance.OpenFileByTextReader(filePath);
    }

    public async void DeleteBot(Bot bot)
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

    public void OpenFolder(string tag)
    {
        if (!Enum.TryParse<EPathFolder>(tag, true, out var folderASFPath)) return;
        var folderASFPathValue = folderASFPath.GetFolderPath();
        IPlatformService.Instance.OpenFolder(folderASFPathValue);
    }

    async void OpenBrowserCore(ActionItem tag)
    {
        var url = tag switch
        {
            ActionItem.Repo => "https://github.com/JustArchiNET/ArchiSteamFarm",
            ActionItem.Wiki => "https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Home-zh-CN",
            ActionItem.ConfigGenerator => "https://justarchinet.github.io/ASF-WebConfigGenerator",
            ActionItem.WebConfig => IPCUrl + "/asf-config",
            ActionItem.WebAddBot => IPCUrl + "/bot/new",
            _ => IPCUrl,
        };

        if (url.StartsWith(IPCUrl))
        {
            string? ipc_error = null;
            if (!ASFService.Current.IsASFRuning)
            {
                ipc_error = BDStrings.ASF_RequirRunASF;
            }
            else if (!ASF.IsReady)
            {
                // IPC 未启动前无法获取正确的端口号，会导致拼接的 URL 值不正确
                ipc_error = BDStrings.ASF_IPCIsReadyFalse;
            }
            if (ipc_error != null)
            {
                Toast.Show(ToastIcon.Error, ipc_error);
                return;
            }
        }

        await Browser2.OpenAsync(url, BrowserLaunchMode.External);
    }

    public void OpenBrowser(string? tag)
    {
        var tag_ = Enum.TryParse<ActionItem>(tag, out var @enum) ? @enum : default;
        OpenBrowserCore(tag_);
    }

    public async void SetEncryptionKey_Click() => await asfService.SetEncryptionKeyAsync();

    public string IPCUrl => asfService.GetIPCUrl();
}
#endif
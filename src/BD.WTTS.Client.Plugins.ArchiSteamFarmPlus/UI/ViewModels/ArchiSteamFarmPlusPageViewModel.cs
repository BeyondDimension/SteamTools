using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class ArchiSteamFarmPlusPageViewModel : ViewModelBase
{
    private readonly IArchiSteamFarmService asfService = IArchiSteamFarmService.Instance;

    public ArchiSteamFarmPlusPageViewModel()
    {
        SelectASFExePath = ReactiveCommand.CreateFromTask(ASFService.Current.SelectASFProgramLocation);

        OpenWebUIConsole = ReactiveCommand.Create(() => ASFService.Current.OpenBrowser(null));

        OpenASFBrowser = ReactiveCommand.Create<string>(ASFService.Current.OpenBrowser);

        ShellMessageInput = ReactiveCommand.Create(ShellMessageInput_Click);

        RunOrStop = ReactiveCommand.Create(RunOrStopASF);
    }

    public override void Activation()
    {
        base.Activation();
        if (string.IsNullOrEmpty(ASFSettings.ArchiSteamFarmExePath.Value))
        {
            MainThread2.InvokeOnMainThreadAsync(async () =>
            {
                var model = new ArchiSteamFarmExePathSettingsPageViewModel();
                await IWindowManager.Instance.ShowTaskDialogAsync(model,
                    title: BDStrings.ASF_Settings,
                    subHeader: BDStrings.ASF_SetExePathFirst,
                    pageContent: new ArchiSteamFarmExePathSettingsPage(),
                    isCancelButton: true,
                    cancelCloseAction: () =>
                    {
                        var cancel = !File.Exists(ASFSettings.ArchiSteamFarmExePath.Value);
                        if (cancel)
                            Toast.Show(ToastIcon.Error, BDStrings.ASF_SelectASFExePath);
                        return cancel;
                    });
            });
        }

        if (ASFSettings.AutoRunArchiSteamFarm.Value && !string.IsNullOrEmpty(ASFSettings.ArchiSteamFarmExePath.Value))
            StartOrStopASF_Click();
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

    public void ShellMessageInput_Click() => ASFService.Current.ShellMessageInput();

    public string? IPCUrl => ASFService.Current.IPCUrl;
}
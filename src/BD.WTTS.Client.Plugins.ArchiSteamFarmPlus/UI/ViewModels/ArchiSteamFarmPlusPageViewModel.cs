using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class ArchiSteamFarmPlusPageViewModel
{
    public ArchiSteamFarmPlusPageViewModel()
    {
        SelectASFExePath = ReactiveCommand.CreateFromTask(ASFService.Current.SelectASFProgramLocationAsync);

        OpenWebUIConsole = ReactiveCommand.Create(() => ASFService.Current.OpenBrowser(null));

        OpenASFBrowser = ReactiveCommand.Create<string>(ASFService.Current.OpenBrowser);

        var runOrStop = ReactiveCommand.CreateFromTask(() => RunASFActionAsync(ASFService.Current.ToggleASFAsync));
        CompositeDisposable.Add(runOrStop);
        RunOrStop = runOrStop;
    }

    public override async void Activation()
    {
        base.Activation();
        if (string.IsNullOrEmpty(ASFSettings.ArchiSteamFarmExePath.Value))
        {
            await MainThread2.InvokeOnMainThreadAsync(async () =>
            {
                var model = new ArchiSteamFarmExePathSettingsPageViewModel();
                await IWindowManager.Instance.ShowTaskDialogAsync(model,
                    title: BDStrings.ASF_Settings,
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
            await RunASFActionAsync(ASFService.Current.InitASFAsync);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ASFService.Current.StopASFAsync().Wait();
        }
        base.Dispose(disposing);
    }

    static async Task RunASFActionAsync(Func<Task> action)
    {
        try
        {
            await Task.Run(action);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
    }

    public string? IPCUrl => ASFService.Current.IPCUrl;
}
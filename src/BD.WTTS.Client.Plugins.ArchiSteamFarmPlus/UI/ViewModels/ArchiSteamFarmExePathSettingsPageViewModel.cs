using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.ViewModels;

public partial class ArchiSteamFarmExePathSettingsPageViewModel : ViewModelBase
{
    public ICommand SelectProgramPath { get; }

    public ICommand DownloadASF { get; }

    public ArchiSteamFarmExePathSettingsPageViewModel()
    {
        SelectProgramPath = ReactiveCommand.Create(ASFService.Current.SelectASFProgramLocationAsync);
        DownloadASF = ReactiveCommand.Create(async () =>
        {
            if (INotificationService.Instance.IsSupportNotifyDownload)
            {
                var progress = INotificationService.Instance.NotifyDownload(() => BD.WTTS.Client.Resources.Strings.L10nAsf_Text099, NotificationType.NewVersion);
                ASFService.Current.DownloadASFAsync(progress: progress);
                Toast.Show(ToastIcon.Info, BD.WTTS.Client.Resources.Strings.L10nAsf_Text100, ToastLength.Short);
            }
            else
            {
                var (downloadDialog, progress) = CreateDownloadDialog();
                ASFService.Current.DownloadASFAsync(progress: progress);
                _ = await downloadDialog.ShowAsync(true);
            }
        });
    }

    private static (TaskDialog DownloadDialog, IProgress<float> Progress) CreateDownloadDialog()
    {
        var downloadDialog = new TaskDialog
        {
            Title = BD.WTTS.Client.Resources.Strings.L10nAsf_Text101,
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = BD.WTTS.Client.Resources.Strings.L10nAsf_Text102,
            Content = BD.WTTS.Client.Resources.Strings.L10nAsf_Text103,
            XamlRoot = AvaloniaWindowManagerImpl.GetWindowTopLevel(),
            // DownloadASFAsync() 无法取消后再次开启下载
            //Buttons = [new TaskDialogButton("取消", TaskDialogStandardResult.Cancel)],
        };
        downloadDialog.Opened += (_, _) => { downloadDialog.SetProgressBarState(0, TaskDialogProgressState.Normal); };

        var progress = new Progress<float>();
        progress.ProgressChanged += (_, value) =>
        {
            // Value here report 1-100
            downloadDialog.Content = string.Format(BD.WTTS.Client.Resources.Strings.L10nAsf_Text104, value);
            downloadDialog.SetProgressBarState(value, TaskDialogProgressState.Normal);

            if (value >= 100)
            {
                downloadDialog.Hide();
            }
        };

        return (downloadDialog, progress);
    }
}
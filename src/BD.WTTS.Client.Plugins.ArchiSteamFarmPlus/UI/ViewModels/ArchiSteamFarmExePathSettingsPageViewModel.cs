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
                var progress = INotificationService.Instance.NotifyDownload(() => "开始下载 ASF ", NotificationType.NewVersion);
                ASFService.Current.DownloadASFAsync(progress: progress);
                Toast.Show(ToastIcon.Info, "等待后台下载文件", ToastLength.Short);
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
            Title = "下载插件",
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = "开始下载 ASF",
            Content = "正在初始化，请稍候",
            XamlRoot = AvaloniaWindowManagerImpl.GetWindowTopLevel(),
            // DownloadASFAsync() 无法取消后再次开启下载
            //Buttons = [new TaskDialogButton("取消", TaskDialogStandardResult.Cancel)],
        };
        downloadDialog.Opened += (_, _) => { downloadDialog.SetProgressBarState(0, TaskDialogProgressState.Normal); };

        var progress = new Progress<float>();
        progress.ProgressChanged += (_, value) =>
        {
            // Value here report 1-100
            downloadDialog.Content = $"正在下载 {value}%";
            downloadDialog.SetProgressBarState(value, TaskDialogProgressState.Normal);

            if (value >= 100)
            {
                downloadDialog.Hide();
            }
        };

        return (downloadDialog, progress);
    }
}
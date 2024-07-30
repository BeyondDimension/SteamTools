namespace BD.WTTS.UI.ViewModels;

public partial class ArchiSteamFarmExePathSettingsPageViewModel : ViewModelBase
{
    public ICommand SelectProgramPath { get; }

    public ICommand DownloadASF { get; }

    public ArchiSteamFarmExePathSettingsPageViewModel()
    {
        SelectProgramPath = ReactiveCommand.Create(ASFService.Current.SelectASFProgramLocationAsync);
        DownloadASF = ReactiveCommand.Create(() =>
        {
            var progress = INotificationService.Instance.NotifyDownload(() => "开始下载 ASF ", NotificationType.NewVersion);
            ASFService.Current.DownloadASFAsync(progress: progress);
            Toast.Show(ToastIcon.Info, "等待后台下载文件", ToastLength.Short);
        });
    }
}

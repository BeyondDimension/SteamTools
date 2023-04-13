#if DEBUG

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public partial class DebugPageViewModel
{
    public DebugPageViewModel() { }

    string _DebugString = string.Empty;

    public string DebugString
    {
        get => _DebugString;
        set => this.RaiseAndSetIfChanged(ref _DebugString, value);
    }

    public async void Debug(string? cmd)
    {
        if (string.IsNullOrEmpty(cmd))
            return;

        var cmds = cmd.ToLowerInvariant().Split(' ');
        switch (cmds[0])
        {
            case "dialog":
                INotificationService.Instance.Notify("测试Test🎆🎇→→", NotificationType.Announcement);
                break;
            case "webview":
                break;
            case "demo":
                break;
            case "window":
                ContentWindowViewModel vm = new() { };
                await IWindowManager.Instance.ShowAsync(AppEndPoint.Content, vm);
                break;
            default:
                DebugString += "未知命令" + Environment.NewLine;
                break;
        }
    }
}
#endif
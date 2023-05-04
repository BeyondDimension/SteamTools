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

        var title = "测试Test🎆🎇→→";

        var cmds = cmd.ToLowerInvariant().Split(' ');
        switch (cmds[0])
        {
            case "dialog":
                var isDialog = cmds.Length > 1 && cmds[1].Contains('1');
                var textModel = new MessageBoxWindowViewModel() { Content = "   <TextBox Classes=\"Content readonlybox h7\"\r\n               DockPanel.Dock=\"Top\"\r\n               Padding=\"15\"\r\n               IsReadOnly=\"True\"\r\n               Text=\"{Binding Content}\"\r\n               AcceptsReturn=\"True\"\r\n               MaxHeight=\"500\"\r\n               MaxWidth=\"{ReflectionBinding #u.MaxWidth}\"\r\n               ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"\r\n               ScrollViewer.VerticalScrollBarVisibility=\"Auto\"\r\n               TextAlignment=\"Left\"\r\n               HorizontalAlignment=\"Stretch\"\r\n               HorizontalContentAlignment=\"Stretch\"\r\n               VerticalContentAlignment=\"Stretch\"\r\n               VerticalAlignment=\"Stretch\"\r\n               TextWrapping=\"Wrap\"/>" };
                var result = await IWindowManager.Instance.ShowTaskDialogAsync(textModel, "Window Title", subHeader: title, isDialog: isDialog, isCancelButton: true);
                DebugString += "ShowTaskDialogAsync Result: " + result + Environment.NewLine;
                //+ "Text Result: " + textModel.Value + Environment.NewLine;
                break;
            case "notify":
                INotificationService.Instance.Notify(title, NotificationType.Announcement);
                break;
            case "webview":
                break;
            case "demo":
                break;
            case "window":
                ContentWindowViewModel vm = new() { PageViewModel = new SettingsPageViewModel { } };
                await IWindowManager.Instance.ShowAsync(AppEndPoint.Content, vm);
                break;
            default:
                DebugString += "未知命令" + Environment.NewLine;
                break;
        }
    }
}
#endif
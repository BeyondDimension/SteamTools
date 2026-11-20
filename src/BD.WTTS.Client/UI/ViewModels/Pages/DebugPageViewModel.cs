using BD.WTTS.Logging;

// ReSharper disable once CheckNamespace

namespace BD.WTTS.UI.ViewModels;

[MP2Obj]
public sealed partial class DebugPageViewModel : TabItemViewModel
{
    protected override bool IsSingleInstance => true;

    public override string Name => "Debug";

    public override string IconKey => "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/bug.ico";

    public DebugPageViewModel() { }

    [MP2Ignore]
    public LogTextBuffer ConsoleLogs { get; } = new();

    sealed class D : Repository<Common.Entities.KeyValuePair, string>
    {

    }

    public async Task Debug(string? cmd)
    {
        if (string.IsNullOrEmpty(cmd))
            return;

        var title = "测试Test🎆🎇→→";

        var cmds = cmd.ToLowerInvariant().Split(' ');
        switch (cmds[0])
        {
            case "dialog":
                var isDialog = cmds.Length > 1 && cmds[1].Contains('1');
                var textModel = new TextBoxWindowViewModel();
                var result = await IWindowManager.Instance.ShowTaskDialogAsync(textModel, "Window Title", subHeader: title, isDialog: isDialog, isCancelButton: true);
                ConsoleLogs.Append($"ShowTaskDialogAsync Result: {result}{Environment.NewLine}Text Result: {textModel.Value}{Environment.NewLine}");
                break;
            case "toast":
                Toast.Show(title);
                break;
            case "messagebox":
                MessageBox.Show(title);
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
                await IWindowManager.Instance.ShowAsync(AppEndPoint.Content, vm, isParent: false);
                break;
            case "asm":
                ConsoleLogs.Replace(string.Join(Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName).OrderBy(x => x)));
                break;
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            case "plu":
                ConsoleLogs.Replace(Startup.Instance.TryGetPlugins(out var loadedPlugins)
                    ? string.Join(Environment.NewLine, loadedPlugins.Select(x => x.GetType().Assembly.FullName).OrderBy(x => x))
                    : null);
                break;
#endif
            case "acc":
                try
                {
                    var accelerator = Startup.Instance.TryGetPlugins(out var plugins)
                        ? plugins.FirstOrDefault(x => x.UniqueEnglishName == AssemblyInfo.Accelerator)
                        : null;
                    if (accelerator == null)
                        throw new InvalidOperationException("Accelerator plugin is not enabled.");
                    await accelerator.OnCommandRun("start");
                }
                catch (Exception ex)
                {
                    ConsoleLogs.Append(ex + Environment.NewLine);
                }
                break;
            case "updatetray":

                break;
            case "gc":
                GC.Collect();
                break;
            case "refreshnews":
#if DEBUG
                NoticeService.Current.ClrearLastLookNoticeDateTime();
#endif
                await NoticeService.Current.GetNewsAsync();
                break;
            case "login":
                if (cmds.Length > 1)
                {
                    // var ss = Ioc.Get_Nullable<ISecurityService>();
                    // var a = await ss.EB(Encoding.UTF8.GetBytes("aaaa"));
                    // var b = await ss.DB(a);
                    // var c = Encoding.UTF8.GetString(b!);
                    // var d = new D();
                    // var key = Hashs.String.SHA256("KEY_CURRENT_LOGIN_USER");
                    // var item = await d.FirstOrDefaultAsync(x => x.Id == key);
                    // var f = await ss.DB(item?.Value);
                    // try
                    // {
                    //     var user = Serializable.DMP<CurrentUser?>(f!);
                    // }
                    // catch (Exception ex)
                    // {
                    // }
                    string phonenumber = cmds[0];
                    if (phonenumber == null) phonenumber = "180" + Random2.GenerateRandomNum(8);
                    if (cmds[1] == "sms")
                    {
                        SendSmsRequest sendSmsRequest = new SendSmsRequest() { PhoneNumber = phonenumber, Type = SmsCodeType.LoginOrRegister };
                        var response = await IMicroServiceClient.Instance.AuthMessage.SendSms(sendSmsRequest);
                    }
                    else
                    {
                        var request = new LoginOrRegisterRequest
                        {
                            PhoneNumber = phonenumber,
                            SmsCode = cmds[1],
                        };
                        //request.Channel = LoginChannel.Client;
                        var response = await IMicroServiceClient.Instance.Account.LoginOrRegister(request);
                    }
                }
                break;
            default:
                ConsoleLogs.Append("未知命令" + Environment.NewLine);
                break;
        }
    }
}
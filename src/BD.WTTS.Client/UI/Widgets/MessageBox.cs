// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI;

/// <summary>
/// 显示消息框
/// </summary>
public static partial class MessageBox
{
    /// <summary>
    /// 指定显示在消息框上的按钮。 用作 <see cref="MessageBox"/>.Show... 方法的参数
    /// </summary>
    public enum Button
    {
        OK = 0,
        OKCancel = 1,

        [Obsolete("non-standard api")]
        YesNo = 4,
        [Obsolete("non-standard api")]
        YesNoCancel = 3,
        [Obsolete("non-standard api")]
        OkAbort = 1000,
        [Obsolete("non-standard api")]
        YesNoAbort = 1001,
    }

    /// <summary>
    /// 指定消息框所显示的图标
    /// </summary>
    public enum Image
    {
        Asterisk = 64,
        Error = 16,
        Exclamation = 48,
        Hand = 16,
        Information = 64,
        None = 0,
        [Obsolete("The question mark message icon is no longer recommended because it does not clearly represent a specific type of message and because the phrasing of a message as a question could apply to any message type. In addition, users can confuse the question mark symbol with a help information symbol. Therefore, do not use this question mark symbol in your message boxes. The system continues to support its inclusion only for backward compatibility.")]
        Question = 32,
        Stop = 16,
        Warning = 48,

        [Obsolete("non-standard api")]
        Battery = 1000,
        [Obsolete("non-standard api")]
        Database,
        [Obsolete("non-standard api")]
        Folder,
        [Obsolete("non-standard api")]
        Forbidden,
        [Obsolete("non-standard api")]
        Plus,
        [Obsolete("non-standard api")]
        Setting,
        [Obsolete("non-standard api")]
        SpeakerLess,
        [Obsolete("non-standard api")]
        SpeakerMore,
        [Obsolete("non-standard api")]
        Stop2,
        [Obsolete("non-standard api")]
        Stopwatch,
        [Obsolete("non-standard api")]
        Wifi,
    }

    /// <summary>
    /// 指定弹框是否可选不再显示。 用作 <see cref="MessageBox"/>.Show... 方法的参数。
    /// </summary>
    public enum DontPromptType
    {
        Undefined = 0,

        /// <summary>
        /// 解锁成就提示
        /// </summary>
        UnLockAchievement = 1,

        /// <summary>
        /// 捐助提示
        /// </summary>
        Donate = 2,

        ResetHostsFile,

        SaveEditAppInfo,

        AndroidCertificateTrustTip,
    }

    /// <summary>
    /// 指定用户单击的消息框按钮。 由 <see cref="MessageBox"/>.Show... 方法返回。
    /// </summary>
    public enum Result
    {
        Cancel = 2,
        None = 0,
        OK = 1,

        [Obsolete("non-standard api")]
        Yes = 6,
        [Obsolete("non-standard api")]
        No = 7,
        [Obsolete("non-standard api")]
        Abort = 1000,
    }

    const string default_caption = AssemblyInfo.Trademark;
    const Button default_button = Button.OK;
    static readonly IMessageBoxService? mbcs = IMessageBoxService.Instance;
    public const Button OKCancel = Button.OKCancel;

    /// <inheritdoc cref="IMessageBoxService.ShowAsync(string, string, Button, Image)"/>
    public static async Task<Result> ShowAsync(
        string messageBoxText, string caption = default_caption, Button button = default_button, Image icon = default, DontPromptType rememberChooseKey = default)
    {
        if (mbcs != null)
        {
            return await mbcs.ShowAsync(messageBoxText, caption, button, icon);
        }

        var isDoNotShow = rememberChooseKey != DontPromptType.Undefined;

        if (isDoNotShow &&
            UISettings.MessageBoxDontPrompts.Contains(rememberChooseKey))
        {
            return Result.OK;
        }

        var viewModel = new MessageBoxWindowViewModel
        {
            Content = messageBoxText,
            IsCancelcBtn = button == OKCancel,
            IsShowRememberChoose = isDoNotShow,
        };

        var r = await IWindowManager.Instance.ShowTaskDialogAsync(
            viewModel, caption, isDialog: false, isCancelButton: viewModel.IsCancelcBtn);

        if (r && viewModel.RememberChoose && isDoNotShow)
        {
            UISettings.MessageBoxDontPrompts.Add(rememberChooseKey);
        }

        return r ? Result.OK : Result.Cancel;
    }

    /// <inheritdoc cref="IMessageBoxService.ShowAsync(string, string, Button, Image)"/>
    public static async void Show(string messageBoxText, string caption = default_caption, Button button = default_button, Image icon = default, DontPromptType rememberChooseKey = default)
    {
        await ShowAsync(messageBoxText, caption, button, icon, rememberChooseKey);
    }

    ///// <inheritdoc cref="IMessageBoxService.ShowAsync(string, string, Button, Image)"/>
    //public static Task<Result> ShowAsync(Exception exception, string caption = default_caption, Button button = default_button, Image icon = default)
    //{
    //    var messageBoxText = exception.GetAllMessage();
    //    return ShowAsync(messageBoxText, caption, button, icon);
    //}

    /// <inheritdoc cref="IMessageBoxService.ShowAsync(string, string, Button, Image)"/>
    public static void Show(Exception exception, string caption = default_caption, Button button = default_button, Image icon = default)
    {
        var messageBoxText = exception.GetAllMessage();
        Show(messageBoxText, caption, button, icon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOK(this Result r) => r == Result.OK;
}
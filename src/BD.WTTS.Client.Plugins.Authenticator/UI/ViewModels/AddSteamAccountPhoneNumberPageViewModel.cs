using WinAuth;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

/// <summary>
/// 添加 Steam 账号手机号页面 VM
/// </summary>
public partial class AddSteamAccountPhoneNumberPageViewModel : WindowViewModel
{
    #region 步骤 Index 定义

    const int TAB_INDEX_BINDING = 0;

    const int TAB_INDEX_CONFIRMATION_Email = 1;

    const int TAB_INDEX_CONFIRMATION_SMS = 2;

    #endregion 步骤 Index 定义

    /// <summary>
    /// 手机号文本
    /// </summary>
    [Reactive]
    public partial string? PhoneNumberText { get; set; }

    /// <summary>
    /// 验证码文本
    /// </summary>
    [Reactive]
    public partial string? CodeText { get; set; }

    /// <summary>
    /// 步骤 Index
    /// </summary>
    [Reactive]
    public partial int SelectIndex { get; set; }

    /// <summary>
    /// 是否显示加载中
    /// </summary>
    [Reactive]
    public partial bool IsLoading { get; set; }

    public ICommand StartBindingCommand { get; }
    public ICommand ConfirmEmailCommand { get; }
    public ICommand VerifyCodeCommand { get; }

    private readonly SteamAuthenticator.EnrollState _enrollState;
    private readonly SteamAuthenticator _steamAuthenticator;

    public AddSteamAccountPhoneNumberPageViewModel(
        SteamAuthenticator.EnrollState enrollState,
        SteamAuthenticator steamAuthenticator
        )
    {
        SelectIndex = TAB_INDEX_BINDING;

        _steamAuthenticator = steamAuthenticator;
        _enrollState = enrollState;

        StartBindingCommand = CreateCommand(StartBindingAsync);
        ConfirmEmailCommand = CreateCommand(ConfirmBindingEmailAsync);
        VerifyCodeCommand = CreateCommand(VerifyAccountPhoneWithCodeAsync);
    }

    ICommand CreateCommand(Func<Task> action)
    {
        var command = ReactiveCommand.CreateFromTask(action);
        CompositeDisposable.Add(command);
        CompositeDisposable.Add(command.ThrownExceptions.Subscribe(static ex => ex.LogAndShowT()));
        return command;
    }

    /// <summary>
    /// 开始绑定手机号
    /// </summary>
    /// <returns></returns>
    public async Task StartBindingAsync()
    {
        if (IsLoading)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_DoNotOperateFrequently);
            return;
        }

        if (string.IsNullOrEmpty(PhoneNumberText))
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseEnterTel);
            return;
        }

        IsLoading = true;

        try
        {
            string? msg = await _steamAuthenticator.AddPhoneNumberAsync(_enrollState, PhoneNumberText);

            Toast.Show(ToastIcon.Info, msg ?? string.Empty);

            if (_enrollState.RequiresEmailConfirmPhone)
            {
                SelectIndex = TAB_INDEX_CONFIRMATION_Email;
            }
            else
            {
                SelectIndex = TAB_INDEX_CONFIRMATION_SMS;
            }
        }
        finally
        {
            IsLoading = false;
        }

    }

    /// <summary>
    /// 确认邮箱链接
    /// </summary>
    /// <returns></returns>
    public async Task ConfirmBindingEmailAsync()
    {
        if (IsLoading)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_DoNotOperateFrequently);
            return;
        }

        if (string.IsNullOrEmpty(PhoneNumberText))
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseEnterTel);
            return;
        }

        IsLoading = true;

        try
        {
            var reslut = await _steamAuthenticator.AddPhoneNumberAsync(_enrollState, PhoneNumberText!);

            if (_enrollState.RequiresEmailConfirmPhone == false && reslut == null)
            {
                SelectIndex = TAB_INDEX_CONFIRMATION_SMS;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 验收短信验证码
    /// </summary>
    /// <returns></returns>
    public async Task VerifyAccountPhoneWithCodeAsync()
    {
        if (IsLoading) return;

        if (string.IsNullOrEmpty(PhoneNumberText))
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseEnterTel);
            return;
        }

        if (string.IsNullOrEmpty(CodeText))
        {
            Toast.Show(ToastIcon.Warning, AppResources.Error_PleaseEnterTelCode);
            return;
        }

        IsLoading = true;
        try
        {
            var verified = await _steamAuthenticator.VerifyPhoneNumberAsync(PhoneNumberText, CodeText, _enrollState.AccessToken!);
            if (verified)
                Close?.Invoke(true);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public static class SendSmsUIHelper
{
    public interface IViewModel
    {
        CancellationTokenSource? CTS { get; set; }

        int TimeLimit { get; set; }

        string BtnSendSmsCodeText { set; }

        bool Disposed { get; }

        bool SendSmsCodeSuccess { get; set; }

        bool IsUnTimeLimit { get; }

        Action? TbPhoneNumberFocus { get; set; }

        Action? TbSmsCodeFocus { get; set; }
    }

    public static async Task SendSmsAsync(this IViewModel i, Func<SendSmsRequest> request)
    {
        if (i.IsUnTimeLimit) return;

        var request_ = request();
        var validator = Ioc.Get<IModelValidator>();
        var isStartSendSmsTimer = validator.Validate(request_);
        if (isStartSendSmsTimer) i.StartSendSmsTimer();

        await i.SendSmsAsync(request_, isStartSendSmsTimer);
    }

    static async void StartSendSmsTimer(this IViewModel i)
    {
        bool SetBtnSendSmsCodeText(int timeLimit)
        {
            if (timeLimit <= 0)
            {
                i.TimeLimit = ApiConstants.SMSInterval;
                i.BtnSendSmsCodeText = AppResources.User_GetSMSCode;
                return false;
            }
            else
            {
                i.TimeLimit = timeLimit;
                i.BtnSendSmsCodeText = AppResources.User_LoginCodeTimeLimitTip_.Format(i.TimeLimit);
                return true;
            }
        }

        i.CTS?.Cancel();
        i.CTS = new();
        var token = i.CTS.Token.Register(() =>
        {
            if (!i.Disposed)
            {
                SetBtnSendSmsCodeText(0);
            }
        }).Token;
        bool b;
        do
        {
            if (token.IsCancellationRequested) break;

            var timeLimit = i.TimeLimit - 1;
            b = SetBtnSendSmsCodeText(timeLimit);
#if DEBUG
            //Toast.Show($"TimeLimit: {i.TimeLimit}");
#endif
            try
            {
                if (b) await Task.Delay(1000, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        while (b);
    }

    static async ValueTask<IApiRsp> SendSmsAsync(this IViewModel i, SendSmsRequest request, bool isStartSendSmsTimer)
    {
        i.TbSmsCodeFocus?.Invoke();

        var client = IMicroServiceClient.Instance;

        var response = await client.AuthMessage.SendSms(request);

        if (!response.IsSuccess)
        {
            if (isStartSendSmsTimer) i.CTS?.Cancel();
            if (response.Code == ApiRspCode.BadRequest ||
                response.Code == ApiRspCode.RequestModelValidateFail ||
                response.Code == ApiRspCode.Fail)
            {
                i.TbPhoneNumberFocus?.Invoke();
            }
        }

        if (!i.SendSmsCodeSuccess && response.IsSuccess) i.SendSmsCodeSuccess = true;

        return response;
    }

    public static bool CanSubmit(this IViewModel i)
    {
        if (!i.SendSmsCodeSuccess)
        {
            Toast.Show(ToastIcon.Error, AppResources.User_SMSCode_Error);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 移除相关委托，在 View 层销毁时调用，因平台 View 层实现与关联 ViewModel 可能不一致，所以不能在 ViewModel 释放时调用
    /// </summary>
    /// <param name="i"></param>
    public static void RemoveAllDelegate(this IViewModel i)
    {
        i.TbPhoneNumberFocus = null;
        i.TbSmsCodeFocus = null;
    }
}
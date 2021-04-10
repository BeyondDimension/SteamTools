using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Threading;
using System.Threading.Tasks;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
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

            Action? TbPhoneNumberFocus { get; }

            Action? TbSmsCodeFocus { get; }
        }

        public static bool TimeStart(this IViewModel i)
        {
            if (i.IsUnTimeLimit) return false;

            i.CTS?.Cancel();
            i.CTS = new();
            Task.Run(async () =>
            {
                bool b;
                do
                {
                    bool SetBtnSendSmsCodeText()
                    {
                        if (i.TimeLimit <= 0)
                        {
                            i.TimeLimit = SMSInterval;
                            i.BtnSendSmsCodeText = AppResources.User_GetSMSCode;
                            return false;
                        }
                        else
                        {
                            i.BtnSendSmsCodeText = string.Format(AppResources.User_LoginCodeTimeLimitTip, i.TimeLimit);
                            return true;
                        }
                    }

                    i.TimeLimit--;
                    b = SetBtnSendSmsCodeText();
#if DEBUG
                    //Toast.Show($"TimeLimit: {i.TimeLimit}");
#endif
                    try
                    {
                        if (b) await Task.Delay(1000, i.CTS.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        if (!i.Disposed)
                        {
                            i.TimeLimit = 0;
                            SetBtnSendSmsCodeText();
                        }
                        break;
                    }
                } while (b);
            });

            return true;
        }

        public static async ValueTask<IApiResponse> SendSms(this IViewModel i, SendSmsRequest request)
        {
            i.TbSmsCodeFocus?.Invoke();

            var client = ICloudServiceClient.Instance;

            var response = await client.AuthMessage.SendSms(request);

            if (!response.IsSuccess)
            {
                i.CTS?.Cancel();
                if (response.Code == ApiResponseCode.BadRequest || response.Code == ApiResponseCode.RequestModelValidateFail)
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
                Toast.Show(AppResources.User_SMSCode_Error);
                return false;
            }

            return true;
        }
    }
}
using BD.WTTS.Client.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAuth;
#if !ANDROID
using ToastLength = BD.Common.Enums.ToastLength;
#endif

namespace BD.WTTS.Models;
public sealed partial class SteamImportAuthenticator
{
    SteamAuthenticator steamAuthenticator = new();

    SteamAuthenticator.EnrollState enrollState = new();

    /// <summary>
    /// 重置界面状态
    /// </summary>
    public void Reset()
    {
        enrollState = new();
        LoginTabIsVisible = true;
        FinalTabIsVisible = false;
        EmailCodeTabIsVisible = false;
        PhoneCodeTabIsVisible = false;
        CaptchaImageTabIsVisible = false;
        UserNameText = null;
        PasswordText = null;
        CaptchaImageText = null;
        CaptchaCodeText = null;
        EmailAuthText = null;
        EmailDomainText = null;
        PhoneCodeText = null;
        RevocationCodeText = null;
    }

    private bool _IsLogining;

    public async Task<bool> LoginSteamFirstAsync()
    {
        if (string.IsNullOrWhiteSpace(UserNameText) || string.IsNullOrWhiteSpace(PasswordText))
        {
            Toast.Show("请输入用户名及密码。");
            return false;
        }
        enrollState.Username = UserNameText;
        enrollState.Password = PasswordText;
        enrollState.Language = ResourceService.GetCurrentCultureSteamLanguageName();
        _IsLogining = true;

        var result = await steamAuthenticator.EnrollAsync(enrollState);

        return result;
    }

    public async Task<bool> LoginSteamWithCaptchaCodeAsync()
    {
        if (string.IsNullOrEmpty(CaptchaCodeText))
        {
            Toast.Show("请输入验证码。");
            return false;
        }
        else
            enrollState.CaptchaText = CaptchaCodeText;

        var result = await steamAuthenticator.EnrollAsync(enrollState);

        CaptchaCodeText = null;
        return result;
    }

    public async Task<bool> LoginSteamWithEmailCodeAsync()
    {
        if (string.IsNullOrEmpty(EmailAuthText))
        {
            Toast.Show("请输入邮箱验证码。");
            return false;
        }
        else
            enrollState.EmailAuthText = EmailAuthText;

        var result = await steamAuthenticator.EnrollAsync(enrollState);
        EmailAuthText = null;
        EmailDomainText = null;
        return result;
    }

    public async Task<bool> LoginSteamWithPhoneCodeAsync()
    {
        if (string.IsNullOrEmpty(PhoneCodeText))
        {
            Toast.Show("请输入手机验证码");
            return false;
        }
        else
            enrollState.ActivationCode = PhoneCodeText;

        var result = await steamAuthenticator.EnrollAsync(enrollState);
        PhoneCodeText = null;
        return result;
    }

    public async void CheckResult(bool issuccess)
    {
        if (!issuccess)
        {
            if (string.IsNullOrEmpty(enrollState.Error) == false)
            {
                if (enrollState.Error.Length > 50)
                    await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel { Content = enrollState.Error, IsCancelcBtn = true });
                else
                    Toast.Show(enrollState.Error, ToastLength.Long);
            }

            //已有令牌，无法导入
            if (enrollState.Requires2FA == true)
            {
                Toast.Show(Strings.LocalAuth_SteamUser_Requires2FA, ToastLength.Long);
                Reset();
                return;
            }
            //需要文字验证码
            else if (enrollState.RequiresCaptcha == true)
            {
                CaptchaImageTabIsVisible = true;
                CaptchaImageText = enrollState.CaptchaUrl;
                LoginTabIsVisible = false;
                return;
            }
            //需要邮箱验证码
            else if (enrollState.RequiresEmailAuth == true)
            {
                LoginTabIsVisible = false;
                CaptchaImageTabIsVisible = false;
                EmailCodeTabIsVisible = true;
                EmailDomainText = string.IsNullOrEmpty(enrollState.EmailDomain) == false ? $"******@{enrollState.EmailDomain}" : string.Empty;
                return;
            }
            //导入最后一步，需要账号绑定的手机验证码确认
            else if (enrollState.RequiresActivation == true)
            {
                enrollState.Error = null;
                LoginTabIsVisible = false;
                EmailCodeTabIsVisible = false;
                PhoneCodeTabIsVisible = true;
                RevocationCodeText = enrollState.RevocationCode;
                return;
            }
            else
            {
                string error = string.IsNullOrEmpty(enrollState.Error) ? Strings.LocalAuth_SteamUser_Error : enrollState.Error;
                Toast.Show(error, ToastLength.Long);
                Reset();
            }
        }
        else
        {
            var iADTO = new AuthenticatorDTO() { Name = $"Steam({UserNameText})", Value = steamAuthenticator };
            AuthenticatorService.AddOrUpdateSaveAuthenticators(iADTO, false, null);
            await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel { Content = Strings.LocalAuth_SteamUserImportSuccess, IsCancelcBtn = true });
            Reset();
        }
    }

    public async void LoginSteamImport()
    {
        if (_IsLogining)
        {
            return;
        }
        _IsLogining = true;

        try
        {
            var isSupportedToastService = ToastService.IsSupported;
            if (isSupportedToastService)
            {
                ToastService.Current.Set(Strings.Logining);
            }
            bool result = false;
            if (enrollState.RequiresCaptcha)
            {
                result = await LoginSteamWithCaptchaCodeAsync();
            }
            else if (enrollState.RequiresEmailAuth)
            {
                result = await LoginSteamWithEmailCodeAsync();
            }
            else if (enrollState.RequiresActivation)
            {
                result = await LoginSteamWithPhoneCodeAsync();
            }
            else
            {
                result = await LoginSteamFirstAsync();
            }
            if (isSupportedToastService)
            {
                ToastService.Current.Set();
            }
            CheckResult(result);

        }
        catch (Exception ex)
        {
            if (ex is WinAuthSteamToManyRequestException)
            {
                Toast.Show(ex.Message);
            }
            else
            {
                Log.Error(nameof(AuthenticatorPageViewModel), ex, nameof(LoginSteamImport));
                await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel { Content = ex + "\rError " + nameof(LoginSteamImport), IsCancelcBtn = true });
            }
        }
        finally
        {
            _IsLogining = false;
            if (ToastService.IsSupported)
            {
                ToastService.Current.Set();
            }
        }
    }

    public async void ShowCaptchaUrl()
    {
        if (string.IsNullOrEmpty(enrollState.CaptchaUrl))
        {
            Toast.Show("CaptchaUrl is null or white space.");
        }
        else
        {
            if (!await Browser2.OpenAsync(enrollState.CaptchaUrl))
            {
                await Clipboard2.SetTextAsync(enrollState.CaptchaUrl);
                Toast.Show(Strings.CopyToClipboard);
            }
        }
    }
}

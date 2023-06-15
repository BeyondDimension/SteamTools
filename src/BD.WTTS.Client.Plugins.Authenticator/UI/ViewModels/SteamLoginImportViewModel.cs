using BD.WTTS.Client.Resources;
using BD.WTTS.Exceptions;
using BD.WTTS.UI.Views.Controls;
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

public sealed partial class SteamLoginImportViewModel
{
    public SteamLoginImportViewModel()
    {
        
    }

    // public void StepperOnSkiping(Stepper sender, CancelEventArgs args)
    // {
    //     Toast.Show("SkipTest");
    // }
    //
    // public void StepperOnBacking(Stepper sender, CancelEventArgs args)
    // {
    //     Toast.Show("BackTest");
    // }
    //
    // public void StepperOnNexting(Stepper sender, CancelEventArgs args)
    // {
    //     args.Cancel = LoginSteamImport().ConfigureAwait(false).GetAwaiter().GetResult();
    //
    // }

    SteamAuthenticator steamAuthenticator = new();

    SteamAuthenticator.EnrollState enrollState = new();

    /// <summary>
    /// 重置状态
    /// </summary>
    public void Reset()
    {
        enrollState = new();
        // LoginTabIsVisible = true;
        // FinalTabIsVisible = false;
        // EmailCodeTabIsVisible = false;
        // PhoneCodeTabIsVisible = false;
        // CaptchaImageTabIsVisible = false;
        UserNameText = null;
        PasswordText = null;
        CaptchaImageText = null;
        CaptchaCodeText = null;
        EmailAuthText = null;
        EmailDomainText = null;
        PhoneCodeText = null;
        RevocationCodeText = null;
        SelectIndex = 0;
    }

    private bool _IsLogining;

    public async Task<bool> LoginSteamFirstAsync()
    {
        if (string.IsNullOrWhiteSpace(UserNameText) || string.IsNullOrWhiteSpace(PasswordText))
        {
            throw new SteamLoginNullCatpchaCodeException("请输入用户名及密码。");
        }
        enrollState.Username = UserNameText;
        enrollState.Password = PasswordText;
        enrollState.Language = ResourceService.GetCurrentCultureSteamLanguageName();
        _IsLogining = true;

        var result = await steamAuthenticator.EnrollAsync(enrollState);

        // if (enrollState.RequiresCaptcha || enrollState.RequiresEmailAuth || enrollState.RequiresActivation) result = true;
        // if (enrollState.RequiresEmailAuth) CaptchaImageTabCanSkip = true;
        // if (enrollState.RequiresActivation) EmailCodeTabCanSkip = true;
        return result;
    }

    public async Task<bool> LoginSteamWithCaptchaCodeAsync()
    {
        if (string.IsNullOrEmpty(CaptchaCodeText))
        {
            throw new SteamLoginNullCatpchaCodeException("请输入验证码。");
        }
        else
            enrollState.CaptchaText = CaptchaCodeText;

        var result = await steamAuthenticator.EnrollAsync(enrollState);

        // if (enrollState.RequiresEmailAuth || enrollState.RequiresActivation) result = true;
        // if (enrollState.RequiresActivation) EmailCodeTabCanSkip = true;

        CaptchaCodeText = null;
        return result;
    }

    public async Task<bool> LoginSteamWithEmailCodeAsync()
    {
        if (string.IsNullOrEmpty(EmailAuthText))
        {
            throw new SteamLoginNullCatpchaCodeException("请输入邮箱验证码。");
        }
        else
            enrollState.EmailAuthText = EmailAuthText;

        var result = await steamAuthenticator.EnrollAsync(enrollState);

        //if (enrollState.RequiresActivation) result = true;

        EmailAuthText = null;
        EmailDomainText = null;
        return result;
    }

    public async Task<bool> LoginSteamWithPhoneCodeAsync()
    {
        if (string.IsNullOrEmpty(PhoneCodeText))
        {
            throw new SteamLoginNullCatpchaCodeException("请输入手机验证码。");
        }
        else
            enrollState.ActivationCode = PhoneCodeText;

        var result = await steamAuthenticator.EnrollAsync(enrollState);
        PhoneCodeText = null;
        
        return result;
    }

    public async void CheckResult(bool result)
    {
        if (!result)
        {
            if (string.IsNullOrEmpty(enrollState.Error) == false)
            {
                if (enrollState.Error.Length > 50)
                    await IWindowManager.Instance.ShowTaskDialogAsync(
                        new MessageBoxWindowViewModel { Content = enrollState.Error, IsCancelcBtn = true });
                else
                    Toast.Show(enrollState.Error, ToastLength.Long);
            }

            //已有令牌，无法导入
            if (enrollState.Requires2FA == true)
            {
                Toast.Show(Strings.LocalAuth_SteamUser_Requires2FA, ToastLength.Long);
                Reset();
                //throw new SteamLoginRequires2FAException(Strings.LocalAuth_SteamUser_Requires2FA);
            }
            //需要文字验证码
            else if (enrollState.RequiresCaptcha == true)
            {
                CaptchaImageText = enrollState.CaptchaUrl;
                SelectIndex = 1;
                return;
            }
            //需要邮箱验证码
            else if (enrollState.RequiresEmailAuth == true)
            {
                EmailDomainText = string.IsNullOrEmpty(enrollState.EmailDomain) == false
                    ? $"******@{enrollState.EmailDomain}"
                    : string.Empty;
                SelectIndex = 2;
                return;
            }
            //导入最后一步，需要账号绑定的手机验证码确认
            else if (enrollState.RequiresActivation == true)
            {
                enrollState.Error = null;
                RevocationCodeText = enrollState.RevocationCode;
                SelectIndex = 3;
                return;
            }
            else
            {
                string error = string.IsNullOrEmpty(enrollState.Error)
                    ? Strings.LocalAuth_SteamUser_Error
                    : enrollState.Error;
                Toast.Show(error, ToastLength.Long);
                Reset();
                return;
            }
        }
        else
        {
            var iADTO = new AuthenticatorDTO() { Name = $"Steam({UserNameText})", Value = steamAuthenticator };
            AuthenticatorService.AddOrUpdateSaveAuthenticatorsAsync(iADTO, false, null);
            await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel { Content = Strings.LocalAuth_SteamUserImportSuccess, IsCancelcBtn = true });
            SelectIndex = 4;
        }
    }

    public async Task<bool> LoginSteamImport()
    {
        if (_IsLogining)
        {
            return false;
        }
        _IsLogining = true;

        try
        {
            var isSupportedToastService = ToastService.IsSupported;
            if (isSupportedToastService)
            {
                ToastService.Current.Set(Strings.Logining);
            }
            var result = true;
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
            return result;
        }
        catch (Exception ex)
        {
            if (ex is WinAuthSteamToManyRequestException or WinAuthInvalidEnrollResponseException)
            {
                Toast.Show(ex.Message, ToastLength.Long);
                return false;
            }
            else if (ex is SteamLoginRequires2FAException slr2ex)
            {
                Toast.Show(slr2ex.Message, ToastLength.Long);
                return false;
            }
            else if (ex is SteamLoginNullCatpchaCodeException slnex)
            {
                Toast.Show(slnex.Message, ToastLength.Long);
                return false;
            }
            else
            {
                Log.Error(nameof(AuthenticatorPageViewModel), ex, nameof(LoginSteamImport));
                await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel { Content = ex + "\rError " + nameof(LoginSteamImport), IsCancelcBtn = true });
            }

            return false;
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
            Toast.Show("验证码URL为空，请重新登录");
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

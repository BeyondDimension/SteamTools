using AppResources = BD.WTTS.Client.Resources.Strings;

using BD.SteamClient.Models;
using BD.SteamClient.Services;
using WinAuth;
#if !ANDROID
using ToastLength = BD.Common.Enums.ToastLength;
#endif

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamLoginImportPageViewModel : ViewModelBase
{
    public static string Name => AppResources.Auth_SteamLoginImport;

    readonly ISteamAccountService _steamAccountService = Ioc.Get<ISteamAccountService>();

    SteamAuthenticator steamAuthenticator = new();

    SteamLoginState _steamLoginState = new();

    SteamAuthenticator.EnrollState _enrollState = new SteamAuthenticator.EnrollState();

    /// <summary>
    /// 重置状态
    /// </summary>
    public void Reset()
    {
        _steamLoginState = new();
        UserNameText = null;
        PasswordText = null;
        CaptchaImageText = null;
        EmailAuthText = null;
        EmailDomainText = null;
        PhoneCodeText = null;
        RevocationCodeText = null;
        SelectIndex = 0;
    }

    private async Task LoginSteamFirstAsync()
    {
        if (string.IsNullOrWhiteSpace(UserNameText) || string.IsNullOrWhiteSpace(PasswordText))
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_PleaseEnterUsernamePassword);
            return;
        }
        _steamLoginState.Username = UserNameText;
        _steamLoginState.Password = PasswordText;
        _steamLoginState.Language = ResourceService.GetCurrentCultureSteamLanguageName();
        IsLoading = true;

        await _steamAccountService.DoLoginV2Async(_steamLoginState);
    }

    //private async Task LoginSteamWithCaptchaCodeAsync()
    //{
    //    if (string.IsNullOrEmpty(CaptchaCodeText))
    //    {
    //        Toast.Show(ToastIcon.Error, AppResources.Error_PleaseEnterUsernamePassword);
    //        return;
    //    }
    //    else
    //        _steamLoginState.CaptchaText = CaptchaCodeText;

    //    await _steamAccountService.DoLoginV2Async(_steamLoginState);

    //    CaptchaCodeText = null;
    //}

    private async Task LoginSteamWithEmailCodeAsync()
    {
        if (string.IsNullOrEmpty(EmailAuthText))
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_PleaseEnterEmailCode);
            return;
        }
        else
            _steamLoginState.EmailCode = EmailAuthText;

        await _steamAccountService.DoLoginV2Async(_steamLoginState);

        EmailAuthText = null;
        EmailDomainText = null;
    }

    public async Task<bool> FinalizeAddAuthenticatorAsync()
    {
        if (string.IsNullOrEmpty(PhoneCodeText))
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_PleaseEnterTelCode);
            return false;
        }
        else
            _enrollState.ActivationCode = PhoneCodeText;

        await steamAuthenticator.FinalizeAddAuthenticatorAsync(_enrollState);

        PhoneCodeText = null;
        return _enrollState.Success;
    }

    /// <summary>
    /// 检查添加令牌是否成功
    /// </summary>
    /// <returns></returns>
    async Task CheckAddAuthenticatorResult()
    {
        if (string.IsNullOrEmpty(_enrollState.Error) == false)
        {
            if (_enrollState.Error.Length > 50)
                await IWindowManager.Instance.ShowTaskDialogAsync(
                    new MessageBoxWindowViewModel { Content = _enrollState.Error });
            else
                Toast.Show(ToastIcon.Info, _enrollState.Error);
        }

        if (_enrollState.NoPhoneNumber == true)
        {
            _enrollState.Error = null;
            SelectIndex = 1;
        }

        //导入最后一步，需要账号绑定的手机验证码确认
        if (_enrollState.RequiresActivation == true)
        {
            _enrollState.Error = null;
            RevocationCodeText = _enrollState.RevocationCode;
            SelectIndex = 2;
        }
    }

    /// <summary>
    /// 检查登录是否成功
    /// </summary>
    /// <param name="result"></param>
    /// <returns>accessToken不为空返回true,否则返回false</returns>
    async Task<bool> CheckLoginResult()
    {
        //已有令牌，无法导入
        if (_steamLoginState.Requires2FA == true)
        {
            Toast.Show(ToastIcon.Error, Strings.LocalAuth_SteamUser_Requires2FA);
            Reset();
            return false;
        }
        //需要邮箱验证码
        //else if (_steamLoginState.RequiresEmailAuth == true)
        //{
        //    EmailDomainText = string.IsNullOrEmpty(_steamLoginState.EmailDomain) == false
        //        ? $"******@{_steamLoginState.EmailDomain}"
        //        : string.Empty;
        //    SelectIndex = -1;
        //    return false;
        //}
        else if (!string.IsNullOrEmpty(_steamLoginState.Message))
        {
            if (_steamLoginState.Message.Length > 50)
                await IWindowManager.Instance.ShowTaskDialogAsync(
                    new MessageBoxWindowViewModel { Content = _steamLoginState.Message, IsCancelcBtn = true });
            else
                Toast.Show(ToastIcon.Info, _steamLoginState.Message);
        }
        //else
        //{
        //    SelectIndex = 0;
        //}

        return !string.IsNullOrEmpty(_steamLoginState.AccessToken);
    }

    public async Task LoginSteamImport()
    {
        if (IsLoading)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_DoNotOperateFrequently);
            return;
        }
        IsLoading = true;

        try
        {
            if (SelectIndex == 0)
            {
                await LoginSteamFirstAsync();
            }
            //else if (_steamLoginState.RequiresCaptcha)
            //{
            //    await LoginSteamWithCaptchaCodeAsync();
            //}
            else if (_steamLoginState.RequiresEmailAuth)
            {
                await LoginSteamWithEmailCodeAsync();
            }

            //验证登录
            if (await CheckLoginResult())
            {
                _enrollState.AccessToken ??= _steamLoginState.AccessToken;
                _enrollState.RefreshToken ??= _steamLoginState.RefreshToken;
                _enrollState.SteamId ??= _steamLoginState.SteamId.ToString();

                //是否需激活令牌
                if (_enrollState.RequiresActivation)
                {
                    if (await FinalizeAddAuthenticatorAsync())
                    {
                        var iADTO = new AuthenticatorDTO()
                        {
                            Name = $"Steam({UserNameText})",
                            Value = steamAuthenticator,
                            Created = DateTimeOffset.Now,
                        };
                        await IAuthenticatorImport.SaveAuthenticator(iADTO);
                        SelectIndex = 3;
                        return;
                    }
                    else
                    {
                        var error = string.IsNullOrEmpty(_enrollState.Error)
                            ? Strings.LocalAuth_SteamUser_Error
                            : _enrollState.Error;
                        Toast.Show(ToastIcon.Error, error);
                    }
                }
                else
                {
                    if (_enrollState.NoPhoneNumber)
                    {
                        if (string.IsNullOrEmpty(PhoneNumberText))
                        {
                            Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseEnterTel);
                            return;
                        }

                        var reslut = await steamAuthenticator.AddPhoneNumberAsync(_enrollState, PhoneNumberText);

                        if (_enrollState.RequiresEmailConfirmPhone)
                        {
                            while (await MessageBox.ShowAsync(Strings.Steam_EmailConfirmation.Format(_enrollState.PhoneNumber, _enrollState.EmailDomain), button: MessageBox.Button.OKCancel) == MessageBox.Result.OK)
                            {
                                reslut = await steamAuthenticator.AddPhoneNumberAsync(_enrollState, PhoneNumberText);
                                if (_enrollState.RequiresEmailConfirmPhone == false && reslut == null)
                                    break;
                            }
                        }

                        if (reslut != null)
                        {
                            Toast.Show(ToastIcon.Info, reslut);
                            return;
                        }
                    }

                    await steamAuthenticator.AddAuthenticatorAsync(_enrollState);

                    await CheckAddAuthenticatorResult();
                }
            }
        }
        catch (WinAuthSteamToManyRequestException ex)
        {
            Toast.Show(ToastIcon.Error, ex.Message);
        }
        catch (WinAuthInvalidEnrollResponseException ex)
        {
            Toast.Show(ToastIcon.Error, ex.Message);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            //Log.Error(nameof(AuthenticatorHomePageViewModel), ex, nameof(LoginSteamImport));
            //await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel
            //{
            //    Content = ex.ToString()
            //});
        }
        finally
        {
            IsLoading = false;
        }
    }
}

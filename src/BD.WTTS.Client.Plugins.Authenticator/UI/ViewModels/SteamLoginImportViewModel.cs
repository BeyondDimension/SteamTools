using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Exceptions;
using WinAuth;
#if !ANDROID
using ToastLength = BD.Common.Enums.ToastLength;
#endif

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamLoginImportViewModel
{
    bool _isFirstLogin = true;

    bool _isConfirmMailUrl;
    
    readonly ISteamAccountService _steamAccountService;

    Func<IAuthenticatorDTO, Task> _saveAuthFun;

    public SteamLoginImportViewModel()
    {
        _steamAccountService = Ioc.Get<ISteamAccountService>();
        _saveAuthFun = (authenticatorDto) => Task.CompletedTask;
    }
    
    public SteamLoginImportViewModel(Func<IAuthenticatorDTO, Task> saveAuthFunc)
    {
        _steamAccountService = Ioc.Get<ISteamAccountService>();
        _saveAuthFun = saveAuthFunc;
    }

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
        CaptchaCodeText = null;
        EmailAuthText = null;
        EmailDomainText = null;
        PhoneCodeText = null;
        RevocationCodeText = null;
        _isFirstLogin = true;
        SelectIndex = 0;
    }

    private bool _IsLogining;

    public async Task LoginSteamFirstAsync()
    {
        if (string.IsNullOrWhiteSpace(UserNameText) || string.IsNullOrWhiteSpace(PasswordText))
        {
            throw new SteamLoginNullCatpchaCodeException("请输入用户名及密码。");
        }
        _steamLoginState.Username = UserNameText;
        _steamLoginState.Password = PasswordText;
        _steamLoginState.Language = ResourceService.GetCurrentCultureSteamLanguageName();
        _IsLogining = true;
        
        await _steamAccountService.DoLoginV2Async(_steamLoginState);
        _isFirstLogin = false;
    }

    public async Task LoginSteamWithCaptchaCodeAsync()
    {
        if (string.IsNullOrEmpty(CaptchaCodeText))
        {
            throw new SteamLoginNullCatpchaCodeException("请输入验证码。");
        }
        else
            _steamLoginState.CaptchaText = CaptchaCodeText;
        
        await _steamAccountService.DoLoginV2Async(_steamLoginState);

        CaptchaCodeText = null;
    }

    public async Task LoginSteamWithEmailCodeAsync()
    {
        if (string.IsNullOrEmpty(EmailAuthText))
        {
            throw new SteamLoginNullCatpchaCodeException("请输入邮箱验证码。");
        }
        else
            _steamLoginState.EmailCode = EmailAuthText;

        await _steamAccountService.DoLoginV2Async(_steamLoginState);
        
        EmailAuthText = null;
        EmailDomainText = null;
    }

    public async Task<bool> AddAuthenticatorAsync()
    {
        return await steamAuthenticator.AddAuthenticatorAsync(_enrollState);
    }
    
    public async Task<bool> FinalizeAddAuthenticatorAsync()
    {
        if (string.IsNullOrEmpty(PhoneCodeText))
        {
            throw new SteamLoginNullCatpchaCodeException("请输入手机验证码。");
        }
        else
            _enrollState.ActivationCode = PhoneCodeText;
        
        await steamAuthenticator.FinalizeAddAuthenticatorAsync(_enrollState);
        PhoneCodeText = null;
        
        return _steamLoginState.Success;
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
                    new MessageBoxWindowViewModel { Content = _enrollState.Error, IsCancelcBtn = true });
            else
                Toast.Show(ToastIcon.Info, _enrollState.Error, ToastLength.Long);
        }

        if (_enrollState.NoPhoneNumber == true)
        {
            _enrollState.Error = null;
            SelectIndex = 2;
        }

        //导入最后一步，需要账号绑定的手机验证码确认
        if (_enrollState.RequiresActivation == true)
        {
            _enrollState.Error = null;
            RevocationCodeText = _enrollState.RevocationCode;
            SelectIndex = 3;
        }
    }
    
    /// <summary>
    /// 检查登陆是否成功
    /// </summary>
    /// <param name="result"></param>
    /// <returns>accessToken不为空返回true,否则返回false</returns>
    async Task<bool> CheckLoginResult()
    {
        if (string.IsNullOrEmpty(_steamLoginState.Message) == false)
        {
            if (_steamLoginState.Message.Length > 50)
                await IWindowManager.Instance.ShowTaskDialogAsync(
                    new MessageBoxWindowViewModel { Content = _steamLoginState.Message, IsCancelcBtn = true });
            else
                Toast.Show(ToastIcon.Info, _steamLoginState.Message, ToastLength.Long);
        }

        //已有令牌，无法导入
        if (_steamLoginState.Requires2FA == true)
        {
            Toast.Show(ToastIcon.Error, Strings.LocalAuth_SteamUser_Requires2FA, ToastLength.Long);
            Reset();
            //throw new SteamLoginRequires2FAException(Strings.LocalAuth_SteamUser_Requires2FA);
        }
        // //需要文字验证码
        // else if (_steamLoginState.RequiresCaptcha == true)
        // {
        //     CaptchaImageText = _steamLoginState.CaptchaUrl;
        //     SelectIndex = 1;
        //     return false;
        // }
        //需要邮箱验证码
        else if (_steamLoginState.RequiresEmailAuth == true)
        {
            EmailDomainText = string.IsNullOrEmpty(_steamLoginState.EmailDomain) == false
                ? $"******@{_steamLoginState.EmailDomain}"
                : string.Empty;
            SelectIndex = 1;
            return false;
        }

        return !string.IsNullOrEmpty(_steamLoginState.AccessToken);
    }

    public async Task LoginSteamImport()
    {
        if (_IsLogining)
        {
            Toast.Show(ToastIcon.Warning, "请勿频繁操作");
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

            if (_isFirstLogin)
            {
                await LoginSteamFirstAsync();
            }
            else if (_steamLoginState.RequiresCaptcha)
            {
                await LoginSteamWithCaptchaCodeAsync();
            }
            else if (_steamLoginState.RequiresEmailAuth)
            {
               await LoginSteamWithEmailCodeAsync();
            }

            if (await CheckLoginResult())
            {
                _enrollState.AccessToken ??= _steamLoginState.AccessToken;
                _enrollState.RefreshToken ??= _steamLoginState.RefreshToken;
                _enrollState.SteamId ??= _steamLoginState.SteamId.ToString();
                
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
                        await _saveAuthFun.Invoke(iADTO);
                        SelectIndex = 4;
                        return;
                    }
                    else
                    {
                        var error = string.IsNullOrEmpty(_enrollState.Error)
                            ? Strings.LocalAuth_SteamUser_Error
                            : _enrollState.Error;
                        Toast.Show(ToastIcon.Error, error, ToastLength.Long);
                        Reset();
                    }
                }
                else
                {
                    if (_enrollState.NoPhoneNumber)
                    {
                        if (string.IsNullOrEmpty(PhoneNumberText))
                        {
                            Toast.Show(ToastIcon.Warning, "请正确输入手机号。");
                            return;
                        }
                        
                        var reslut = await steamAuthenticator.AddPhoneNumberAsync(_enrollState.AccessToken!,
                            _enrollState.SteamId,
                            PhoneNumberText, _isConfirmMailUrl);
                        
                        if (!string.IsNullOrEmpty(reslut))
                        {
                            Toast.Show(ToastIcon.Info, reslut);
                            _isConfirmMailUrl = true;
                            return;
                        }

                        _isConfirmMailUrl = false;
                        //添加手机号时确认邮箱链接需要时间
                        await Task.Delay(2000);
                    }
                    await AddAuthenticatorAsync();
                    await CheckAddAuthenticatorResult();
                }
            }
            
            if (isSupportedToastService)
            {
                ToastService.Current.Set();
            }
        }
        catch (Exception ex)
        {
            if (ex is WinAuthSteamToManyRequestException or WinAuthInvalidEnrollResponseException)
            {
                Toast.Show(ToastIcon.Error, ex.Message, ToastLength.Long);
                return;
            }
            else if (ex is SteamLoginRequires2FAException slr2ex)
            {
                Toast.Show(ToastIcon.Error, slr2ex.Message, ToastLength.Long);
                return;
            }
            else if (ex is SteamLoginNullCatpchaCodeException slnex)
            {
                Toast.Show(ToastIcon.Error, slnex.Message, ToastLength.Long);
                return;
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

    public async Task ShowCaptchaUrl() => await AuthenticatorService.ShowCaptchaUrl(_steamLoginState.CaptchaUrl);
}

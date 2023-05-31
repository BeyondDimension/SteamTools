using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorPageViewModel : TabItemViewModel
{
    public override string Name => "AuthenticatorPage";

    SteamAuthenticator? steamAuthenticator;

    SteamAuthenticator.EnrollState enrollState = new() { RequiresLogin = true };

    public AuthenticatorPageViewModel()
    {
        ImportTabControlIsVisible = true;
    }
    //[Reactive]
    //public ObservableCollection<string> AuthenticatorTab { get; set; }

    public bool ImportTabControlIsVisible { get; set; }

    public string? UserName
    {
        get => enrollState.Username;
        set
        {
            enrollState.Username = value;
            this.RaisePropertyChanged();
        }
    }

    public string? Password
    {
        get => enrollState.Password;
        set
        {
            enrollState.Password = value;
            this.RaisePropertyChanged();
        }
    }

    public string? CaptchaText
    {
        get => enrollState.CaptchaText;
        set
        {
            enrollState.CaptchaText = value;
            this.RaisePropertyChanged();
        }
    }

    public bool RequiresLogin
    {
        get => enrollState.RequiresLogin;
        set
        {
            enrollState.RequiresLogin = value;
            this.RaisePropertyChanged();
        }
    }

    public string? ActivationCode
    {
        get => enrollState.ActivationCode;
        set
        {
            enrollState.ActivationCode = value;
            this.RaisePropertyChanged();
        }
    }

    public bool RequiresActivation
    {
        get => enrollState.RequiresActivation;
        set
        {
            this.RaisePropertyChanged();
        }
    }

    public string? RevocationCode
    {
        get => enrollState.RevocationCode;
        set
        {
            this.RaisePropertyChanged();
        }
    }

    public string? CaptchaImage
    {
        get => enrollState.CaptchaUrl;
        set
        {
            this.RaisePropertyChanged();
        }
    }

    public string? EmailDomain
    {
        get => enrollState.EmailDomain;
        set
        {
            this.RaisePropertyChanged();
        }
    }

    private bool _RequiresAdd;

    public bool RequiresAdd
    {
        get => _RequiresAdd;
        set => this.RaiseAndSetIfChanged(ref _RequiresAdd, value);
    }

    public string? EmailAuthText
    {
        get => enrollState.EmailAuthText;
        set
        {
            enrollState.EmailAuthText = value;
            this.RaisePropertyChanged();
        }
    }

    /// <summary>
    /// 重置界面状态
    /// </summary>
    public void Reset()
    {
        enrollState = new() { RequiresLogin = true };
        RequiresLogin = true;
        RequiresAdd = false;
        UserName = null;
        Password = null;
    }

    private bool _IsLogining;

    /// <summary>
    /// 登录Steam导入令牌
    /// </summary>
    public async void LoginSteamImport()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            return;
        }
        if (_IsLogining)
        {
            return;
        }
        _IsLogining = true;
        if (steamAuthenticator == null)
        {
            steamAuthenticator = new SteamAuthenticator();
        }
        enrollState.Language = ResourceService.GetCurrentCultureSteamLanguageName();
        try
        {
            var isSupportedToastService = ToastService.IsSupported;
            if (isSupportedToastService)
            {
                ToastService.Current.Set(Strings.Logining);
            }
            var result = await steamAuthenticator.EnrollAsync(enrollState);
            if (isSupportedToastService)
            {
                ToastService.Current.Set();
            }
            if (!result)
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
                    return;
                }
                //频繁登录需要图片验证码
                if (enrollState.RequiresCaptcha == true)
                {
                    CaptchaText = null;
                    CaptchaImage = null;
                    CaptchaImage = enrollState.CaptchaUrl;
                    return;
                }
                //需要邮箱验证码
                if (enrollState.RequiresEmailAuth == true)
                {
                    RequiresLogin = false;
                    CaptchaText = null;
                    CaptchaImage = null;
                    EmailDomain = string.IsNullOrEmpty(enrollState.EmailDomain) == false ? $"******@{enrollState.EmailDomain}" : string.Empty;
                    return;
                }
                //导入最后一步，需要账号绑定的手机验证码确认
                if (enrollState.RequiresActivation == true)
                {
                    EmailDomain = null;
                    enrollState.Error = null;
                    RequiresLogin = false;
                    RequiresActivation = true;
                    RevocationCode = enrollState.RevocationCode;
                    return;
                }

                if (enrollState.RequiresLogin == true)
                {
                    return;
                }
                string error = string.IsNullOrEmpty(enrollState.Error) ? Strings.LocalAuth_SteamUser_Error : enrollState.Error;
                //await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel { Content = error, IsCancelcBtn = true });
                Toast.Show(error, ToastLength.Long);
                return;
            }
            //通过所有验证，开始导入令牌
            else
            {
                RequiresActivation = false;
                RequiresAdd = true;

                var iADTO = new AuthenticatorDTO() { Name = $"Steam({UserName})", Value = steamAuthenticator };

                AuthenticatorService.AddOrUpdateSaveAuthenticators(iADTO, false, null);
                Toast.Show(Strings.LocalAuth_SteamUserImportSuccess);
            }

        }
        catch (Exception ex)
        {
            Log.Error(nameof(AuthenticatorPageViewModel), ex, nameof(LoginSteamImport));
            await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel { Content = ex + "\rError " + nameof(LoginSteamImport), IsCancelcBtn = true });
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

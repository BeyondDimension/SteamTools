using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.IO;
using System.Net;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class AddAuthWindowViewModel : WindowViewModel
    {
        public AddAuthWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LocalAuth_AddAuth;
        }
        private GAPAuthenticatorValueDTO.SteamAuthenticator _SteamAuthenticator;
        private GAPAuthenticatorValueDTO.SteamAuthenticator.EnrollState _Enroll = new();

        public string AuthName { get; set; }

        public string UUID { get; set; }

        public string SteamGuard { get; set; }

        public bool? RequiresCaptcha
        {
            get
            {
                return _Enroll.RequiresCaptcha;
            }
        }

        public bool? RequiresEmailAuth
        {
            get
            {
                return _Enroll.RequiresEmailAuth;
            }
        }

        public bool? RequiresLogin
        {
            get
            {
                return _Enroll.RequiresLogin;
            }
        }

        public bool? RequiresActivation
        {
            get
            {
                return _Enroll.RequiresActivation;
            }
        }

        public string? UserName
        {
            get
            {
                return _Enroll.Username;
            }
            set
            {
                _Enroll.Username = value;
            }
        }

        public string? Password
        {
            get
            {
                return _Enroll.Password;
            }
            set
            {
                _Enroll.Password = value;
            }
        }

        public string? CaptchaText
        {
            get
            {
                return _Enroll.CaptchaText;
            }
            set
            {
                _Enroll.CaptchaText = value;
            }
        }

        public string? EmailAuthText
        {
            get
            {
                return _Enroll.EmailAuthText;
            }
            set
            {
                _Enroll.EmailAuthText = value;
            }
        }

        public string? ActivationCode
        {
            get
            {
                return _Enroll.ActivationCode;
            }
            set
            {
                _Enroll.ActivationCode = value;
            }
        }

        public string? RevocationCode
        {
            get
            {
                return _Enroll.RevocationCode;
            }
        }

        private string? _CaptchaImageUrl;
        public string? CaptchaImageUrl
        {
            get => _CaptchaImageUrl;
            set => this.RaiseAndSetIfChanged(ref _CaptchaImageUrl, value);
        }

        private string? _EmailDomain;
        public string? EmailDomain
        {
            get => _EmailDomain;
            set => this.RaiseAndSetIfChanged(ref _EmailDomain, value);
        }

        private bool _RequiresAdd;
        public bool RequiresAdd
        {
            get => _RequiresAdd;
            set => this.RaiseAndSetIfChanged(ref _RequiresAdd, value);
        }

        public void ImportSteamGuard()
        {
            if (AuthService.Current.ImportSteamGuard(AuthName, UUID, SteamGuard))
            {
                ToastService.Current.Notify(AppResources.LocalAuth_AddAuthSuccess);
            }
            else
            {
                ToastService.Current.Notify(AppResources.LocalAuth_ImportFaild);
            }
        }


        public void LoginSteamImport()
        {
            if (_SteamAuthenticator == null)
                _SteamAuthenticator = new GAPAuthenticatorValueDTO.SteamAuthenticator();

            var result = _SteamAuthenticator.Enroll(_Enroll);

            if (result == false)
            {
                if (string.IsNullOrEmpty(_Enroll.Error) == false)
                {
                    ToastService.Current.Notify(_Enroll.Error);
                }

                if (_Enroll.Requires2FA == true)
                {
                    ToastService.Current.Notify("您的帐户似乎已添加了身份验证器");
                    return;
                }

                if (_Enroll.RequiresCaptcha == true)
                {
                    CaptchaImageUrl = _Enroll.CaptchaUrl;
                    return;
                }

                if (_Enroll.RequiresEmailAuth == true)
                {
                    EmailDomain = string.IsNullOrEmpty(_Enroll.EmailDomain) == false ? "***@" + _Enroll.EmailDomain : string.Empty;
                    return;
                }

                if (_Enroll.RequiresLogin == true)
                {
                    return;
                }

                if (_Enroll.RequiresActivation == true)
                {
                    _Enroll.Error = null;

                    //this.Authenticator.AuthenticatorData = m_steamAuthenticator;

                    AuthService.Current.Authenticators.Add(new MyAuthenticator(new GAPAuthenticatorDTO
                    {
                        Name = nameof(GamePlatform.Steam) + "(" + UserName + ")",
                        Value = _SteamAuthenticator
                    }));

                    //RevocationCode = _Enroll.RevocationCode;
                    return;
                }

                string error = _Enroll.Error;
                if (string.IsNullOrEmpty(error) == true)
                {
                    error = "无法将添加身份验证器添加到您的帐户， 请稍后再试。";
                }
                ToastService.Current.Notify(error);
                return;
            }

            RequiresAdd = true;
        }
    }
}
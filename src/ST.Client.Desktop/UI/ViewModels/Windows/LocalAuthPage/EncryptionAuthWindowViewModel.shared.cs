using ReactiveUI;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public partial class EncryptionAuthWindowViewModel
    {
        private readonly IGameAccountPlatformAuthenticatorRepository repository = DI.Get<IGameAccountPlatformAuthenticatorRepository>();

        public EncryptionAuthWindowViewModel() : base()
        {
            Title =
#if !__MOBILE__
                ThisAssembly.AssemblyTrademark + " | " +
#endif
                AppResources.LocalAuth_ProtectionAuth;
            Initialize();
        }

        private async void Initialize()
        {
            var (success, password) = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (!success)
            {
                this.Close();
            }

            var auths = await repository.GetAllSourceAsync();
            IsPasswordEncrypt = repository.HasSecondaryPassword(auths);
#if !__MOBILE__
            IsOnlyCurrentComputerEncrypt = repository.HasLocal(auths);
#endif
            if (IsPasswordEncrypt)
            {
                Password = password;
                VerifyPassword = password;
            }
        }

        private string? _Password;
        public string? Password
        {
            get => _Password;
            set => this.RaiseAndSetIfChanged(ref _Password, value);
        }

        private string? _VerifyPassword;
        public string? VerifyPassword
        {
            get => _VerifyPassword;
            set => this.RaiseAndSetIfChanged(ref _VerifyPassword, value);
        }

        private bool _IsPasswordEncrypt;
        public bool IsPasswordEncrypt
        {
            get => _IsPasswordEncrypt;
            set => this.RaiseAndSetIfChanged(ref _IsPasswordEncrypt, value);
        }

#if !__MOBILE__
        private bool _IsOnlyCurrentComputerEncrypt;
        public bool IsOnlyCurrentComputerEncrypt
        {
            get => _IsOnlyCurrentComputerEncrypt;
            set => this.RaiseAndSetIfChanged(ref _IsOnlyCurrentComputerEncrypt, value);
        }
#endif

        public async void EncryptionAuth()
        {
            var auths = await repository.GetAllSourceAsync();
            var hasPassword = repository.HasSecondaryPassword(auths);
            var hasLocal = repository.HasLocal(auths);

            if (IsPasswordEncrypt)
            {
                if (!string.IsNullOrWhiteSpace(VerifyPassword) && Password == VerifyPassword)
                {
                    //if (IsOnlyCurrentComputerEncrypt)
                    //{
                    //    if (hasPassword && hasLocal)
                    //    {
                    //        //Toast.Show("已经设置了密码和本机加密");
                    //        Toast.Show(AppResources.LocalAuth_ProtectionAuth_NoChangeTip);
                    //        return;
                    //    }
                    //}
                    //else
                    //{
                    //    if (hasPassword)
                    //    {
                    //        //Toast.Show("已经开启了加密");
                    //        Toast.Show(AppResources.LocalAuth_ProtectionAuth_NoChangeTip);
                    //        return;
                    //    }
                    //}
                }
                else
                {
                    Toast.Show(AppResources.LocalAuth_ProtectionAuth_PasswordErrorTip);
                    return;
                }
            }
#if !__MOBILE__
            else if (IsOnlyCurrentComputerEncrypt)
            {
                if (hasLocal)
                {
                    //Toast.Show("已经设置了本机加密");
                    Toast.Show(AppResources.LocalAuth_ProtectionAuth_NoChangeTip);
                    return;
                }
                VerifyPassword = null;
            }
#endif
            else
            {
                if (!hasPassword && !hasLocal)
                {
                    Toast.Show(AppResources.LocalAuth_ProtectionAuth_NoChangeTip);
                    return;
                }
                VerifyPassword = null;
            }

            this.Close();

            AuthService.Current.SwitchEncryptionAuthenticators(
#if !__MOBILE__
                IsOnlyCurrentComputerEncrypt
#else
                false
#endif
                , VerifyPassword);
        }
    }
}
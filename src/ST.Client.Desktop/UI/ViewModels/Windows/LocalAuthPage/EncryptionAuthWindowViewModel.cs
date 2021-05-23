using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class EncryptionAuthWindowViewModel : WindowViewModel
    {
        private readonly IGameAccountPlatformAuthenticatorRepository repository = DI.Get<IGameAccountPlatformAuthenticatorRepository>();

        public EncryptionAuthWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LocalAuth_ProtectionAuth;
            Initialize();
        }

        private async void Initialize()
        {
            var result = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (!result.success)
            {
                this.Close();
            }

            var auths = await repository.GetAllSourceAsync();
            IsPasswordEncrypt = repository.HasSecondaryPassword(auths);
            IsOnlyCurrentComputerEncrypt = repository.HasLocal(auths);
            if (IsPasswordEncrypt)
            {
                Password = result.password;
                VerifyPassword = result.password;
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

        private bool _IsOnlyCurrentComputerEncrypt;
        public bool IsOnlyCurrentComputerEncrypt
        {
            get => _IsOnlyCurrentComputerEncrypt;
            set => this.RaiseAndSetIfChanged(ref _IsOnlyCurrentComputerEncrypt, value);
        }

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

            AuthService.Current.SwitchEncryptionAuthenticators(IsOnlyCurrentComputerEncrypt, VerifyPassword);
        }
    }
}
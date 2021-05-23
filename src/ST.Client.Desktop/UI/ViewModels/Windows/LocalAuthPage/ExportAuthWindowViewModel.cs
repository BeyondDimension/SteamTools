using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class ExportAuthWindowViewModel : WindowViewModel
    {
        public ExportAuthWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LocalAuth_ProtectionAuth;
        }


        private bool _IsPasswordEncrypt;
        public bool IsPasswordEncrypt
        {
            get => _IsPasswordEncrypt;
            set => this.RaiseAndSetIfChanged(ref _IsPasswordEncrypt, value);
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

        private bool _IsOnlyCurrentComputerEncrypt;
        public bool IsOnlyCurrentComputerEncrypt
        {
            get => _IsOnlyCurrentComputerEncrypt;
            set => this.RaiseAndSetIfChanged(ref _IsOnlyCurrentComputerEncrypt, value);
        }

        private string? _Path;
        public string? Path
        {
            get => _Path;
            set => this.RaiseAndSetIfChanged(ref _Path, value);
        }


        public async void ExportAuth()
        {
            var result = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (!result.success)
            {
                this.Close();
            }

            if (string.IsNullOrEmpty(Path))
            {
                Toast.Show(AppResources.LocalAuth_ProtectionAuth_PathError);
                return;
            }

            if (IsPasswordEncrypt)
            {
                if (string.IsNullOrWhiteSpace(VerifyPassword) && VerifyPassword != Password)
                {
                    Toast.Show(AppResources.LocalAuth_ProtectionAuth_PasswordErrorTip);
                    return;
                }
            }
            else
            {
                VerifyPassword = null;
            }

            AuthService.Current.ExportAuthenticators(Path, IsOnlyCurrentComputerEncrypt, VerifyPassword);

            this.Close();

            Toast.Show(string.Format(AppResources.LocalAuth_ExportAuth_ExportSuccess, Path));
        }
    }
}
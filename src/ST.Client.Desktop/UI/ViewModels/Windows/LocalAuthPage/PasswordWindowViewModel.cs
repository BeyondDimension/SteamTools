using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Properties;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class PasswordWindowViewModel : WindowViewModel
    {
        public PasswordWindowViewModel() : base()
        {
            Title = AppResources.LocalAuth_PasswordRequired;
        }

        private string? _Password;
        public string? Password
        {
            get => _Password;
            set => this.RaiseAndSetIfChanged(ref _Password, value);
        }


        public static async Task<string?> ShowPasswordDialog()
        {
            var vm = new PasswordWindowViewModel();
            await IShowWindowService.Instance.ShowDialog(CustomWindow.Password, vm, string.Empty, ResizeModeCompat.CanResize);
            return vm.Password;
        }


        public void Ok()
        {
            if (string.IsNullOrEmpty(Password))
            {
                Toast.Show(AppResources.LocalAuth_ProtectionAuth_PasswordErrorTip);
                return;
            }
            this.Close();
        }

        public void Cancel()
        {
            Password = null;
            this.Close();
        }
    }
}
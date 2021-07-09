using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Threading.Tasks;
using System.Windows;
#if __MOBILE__
using WindowViewModel = System.Application.UI.ViewModels.PageViewModel;
#endif

namespace System.Application.UI.ViewModels
{
    public class PasswordWindowViewModel : WindowViewModel, ITextBoxWindowViewModel
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

        string? ITextBoxWindowViewModel.Value { get => Password; set => Password = value; }

        public static async Task<string?> ShowPasswordDialog()
        {
            var vm = new PasswordWindowViewModel();
            await IShowWindowService.Instance.ShowDialog(CustomWindow.Password, vm, string.Empty, ResizeModeCompat.CanResize);
            return vm.Password;
        }

        public bool InputValidator()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                Toast.Show(AppResources.LocalAuth_ProtectionAuth_PasswordErrorTip);
                return false;
            }
            return true;
        }

#if !__MOBILE__
        public void Ok()
        {
            if (!InputValidator())
            {
                return;
            }
            this.Close();
        }

        public void Cancel()
        {
            Password = string.Empty;
            this.Close();
        }
#endif
    }
}
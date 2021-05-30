using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace System.Application.UI.ViewModels
{
    [QueryProperty(nameof(LoginState_), nameof(LoginState))]
    partial class LoginOrRegisterPageViewModel : PageViewModel
    {
        internal static Task FastLoginOrRegisterAsync(Action? close = null, FastLoginChannel channel = FastLoginChannel.Steam, bool isBind = false)
        {
            Toast.Show(channel.ToString());
            return Task.CompletedTask;
        }

        static async Task GoToLoginOrRegisterByPhoneNumberAsync()
        {
            var url = $"//{AppShell.Route_LoginOrRegister_Secondary}?{nameof(LoginState)}=1";
            await Shell.Current.GoToAsync(url);
        }

        public short LoginState_
        {
            set => LoginState = value == default ? _LoginStateDefault : value;
        }

        public ICommand? TbPhoneNumberReturnCommand { get; set; }
    }
}
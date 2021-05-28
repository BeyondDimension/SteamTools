using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    partial class LoginOrRegisterPageViewModel : PageViewModel
    {
        internal static Task FastLoginOrRegisterAsync(Action? close = null, FastLoginChannel channel = FastLoginChannel.Steam, bool isBind = false)
        {
            Toast.Show(channel.ToString());
            return Task.CompletedTask;
        }
    }
}
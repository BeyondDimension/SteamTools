using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    partial class LoginOrRegisterPageViewModel : ViewModelBase
    {
        internal static Task FastLoginOrRegisterAsync(Action? close = null, FastLoginChannel channel = FastLoginChannel.Steam, bool isBind = false)
        {
            return Task.CompletedTask;
        }
    }
}
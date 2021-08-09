using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    partial class LoginOrRegisterWindowViewModel : WindowViewModel
    {
        internal Task GoToLoginOrRegisterByPhoneNumberAsync()
        {
            LoginState = 1;
            return Task.CompletedTask;
        }
       
    }
}
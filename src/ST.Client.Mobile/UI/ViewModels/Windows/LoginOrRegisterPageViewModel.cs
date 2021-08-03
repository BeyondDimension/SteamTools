using System.Threading.Tasks;
using System.Windows.Input;

namespace System.Application.UI.ViewModels
{
    partial class LoginOrRegisterPageViewModel : PageViewModel
    {
        static Task GoToLoginOrRegisterByPhoneNumberAsync()
        {
            return Task.CompletedTask;
        }

        public ICommand? TbPhoneNumberReturnCommand { get; set; }
    }
}
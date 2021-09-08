using Microsoft.Identity.Client;
using System.Application.Services;
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
            switch (channel)
            {
                case FastLoginChannel.Microsoft:
                    MicrosoftFastLoginOrRegister();
                    static async void MicrosoftFastLoginOrRegister()
                    {
                        var publicClientApp = DI.Get_Nullable<IPublicClientApplication>();
                        if (publicClientApp == null) return;
                        var uiHost = IMobilePlatformService.Instance.CurrentPlatformUIHost;
                        // 页面后退不会结束等待，此处使用 ReactiveCommand.CreateFromTask，所以需要 async void
                        var authResult = await publicClientApp
                            .AcquireTokenInteractive(new string[] { "user.read" })
                            .WithParentActivityOrWindow(uiHost)
                            .ExecuteAsync();
                        var dstr = Serializable.SJSON(authResult);
                        Toast.Show(dstr);
                    }
                    break;
                case FastLoginChannel.Steam:
                case FastLoginChannel.QQ:
                case FastLoginChannel.Apple:
                    Toast.Show($"TODO: {channel}");
                    break;
            }
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
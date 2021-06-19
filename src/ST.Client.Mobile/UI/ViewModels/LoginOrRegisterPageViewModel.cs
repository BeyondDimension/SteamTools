using Microsoft.Identity.Client;
using ReactiveUI;
using System.Application.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System.Application.UI.ViewModels
{
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

        static Task GoToLoginOrRegisterByPhoneNumberAsync()
        {
            return Task.CompletedTask;
        }

        public ICommand? TbPhoneNumberReturnCommand { get; set; }
    }
}
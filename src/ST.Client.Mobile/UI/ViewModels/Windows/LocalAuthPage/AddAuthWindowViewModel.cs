using ReactiveUI;
using System.Properties;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Application.UI.ViewModels
{
    partial class AddAuthWindowViewModel : PageViewModel
    {
        public static async Task FilePickAsync(Action<string> action)
        {
            try
            {
                var result = await FilePicker.PickAsync(new()
                {
                    PickerTitle = ThisAssembly.AssemblyTrademark,
                });
                if (result != null)
                {
                    action(result.FileName);
                }
            }
            catch (PermissionException e)
            {
                Toast.Show(e.Message);
            }
            catch
            {
                // The user canceled or something went wrong
            }
        }

        private string? _LoginSteamLoadingText;
        public string? LoginSteamLoadingText
        {
            get => _LoginSteamLoadingText;
            set => this.RaiseAndSetIfChanged(ref _LoginSteamLoadingText, value);
        }
    }
}
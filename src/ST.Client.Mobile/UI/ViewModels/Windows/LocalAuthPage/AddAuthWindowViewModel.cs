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
            catch
            {
                // The user canceled or something went wrong
            }
        }
    }
}
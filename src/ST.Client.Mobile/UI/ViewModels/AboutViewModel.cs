using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace System.Application.UI.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async ()
                //=> await Browser.OpenAsync("https://aka.ms/xamarin-quickstart")
                => await Shell.Current.GoToAsync($"//LoginOrRegister")
                );
        }

        public ICommand OpenWebCommand { get; }
    }
}
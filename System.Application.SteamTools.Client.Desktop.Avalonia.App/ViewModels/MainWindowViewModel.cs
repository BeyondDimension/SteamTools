using ReactiveUI;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class MainWindowViewModel : WindowViewModel
    {
        bool mTopmost;

        public bool Topmost
        {
            get => mTopmost;
            set => this.RaiseAndSetIfChanged(ref mTopmost, value);
        }

        public MainWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark;
            SettingsPage = new SettingsPageViewModel();
        }

        public SettingsPageViewModel SettingsPage { get; }
    }
}
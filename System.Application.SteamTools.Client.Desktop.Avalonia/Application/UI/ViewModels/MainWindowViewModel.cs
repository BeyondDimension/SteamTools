using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
using System.Text;
using System.Threading.Tasks;

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
        public SettingsPageViewModel SettingsPage { get; }


        public MainWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark;
            SettingsPage = new SettingsPageViewModel();
        }
    }
}

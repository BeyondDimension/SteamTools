using ReactiveUI;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.UI.ViewModels
{
    public class SettingsPageViewModel : TabItemViewModel
    {
        public static SettingsPageViewModel Instance { get; } = new SettingsPageViewModel();

        public override string Name
        {
            get => AppResources.Settings;
            protected set { throw new NotImplementedException(); }
        }

        public SettingsPageViewModel()
        {

        }

        KeyValuePair<string, string> _CurrentLanguage = R.Languages.First();
        public KeyValuePair<string, string> CurrentLanguage
        {
            get => _CurrentLanguage;
            set
            {
                this.RaiseAndSetIfChanged(ref _CurrentLanguage, value);
                R.ChangeLanguage(_CurrentLanguage.Key);
            }
        }


    }
}
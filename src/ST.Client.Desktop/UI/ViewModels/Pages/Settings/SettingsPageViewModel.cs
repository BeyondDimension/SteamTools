using ReactiveUI;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class SettingsPageViewModel : TabItemViewModel
    {
        public static SettingsPageViewModel Instance { get; } = new();

        public override string Name
        {
            get => AppResources.Settings;
            protected set { throw new NotImplementedException(); }
        }

        KeyValuePair<string, string> _SelectLanguage;
        public KeyValuePair<string, string> SelectLanguage
        {
            get => _SelectLanguage;
            protected set => this.RaiseAndSetIfChanged(ref _SelectLanguage, value);
        }

        public SettingsPageViewModel()
        {
            SelectLanguage = R.Languages.SingleOrDefault(x => x.Key == UISettings.Language.Value);
            this.WhenAnyValue(x => x.SelectLanguage)
                  .DistinctUntilChanged()
                  .Subscribe(x => UISettings.Language.Value = x.Key);
        }
    }
}
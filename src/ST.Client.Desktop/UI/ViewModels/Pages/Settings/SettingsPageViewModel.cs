using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Drawing;
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

        KeyValuePair<string, string> _SelectFont;
        public KeyValuePair<string, string> SelectFont
        {
            get => _SelectFont;
            protected set => this.RaiseAndSetIfChanged(ref _SelectFont, value);
        }

        public SettingsPageViewModel()
        {
            IconKey = nameof(SettingsPageViewModel).Replace("ViewModel", "Svg");

            SelectLanguage = R.Languages.FirstOrDefault(x => x.Key == UISettings.Language.Value);
            this.WhenValueChanged(x => x.SelectLanguage, false)
                  .Subscribe(x => UISettings.Language.Value = x.Key);

            SelectFont = R.Fonts.FirstOrDefault(x => x.Value == UISettings.FontName.Value);
            this.WhenValueChanged(x => x.SelectFont, false)
                  .Subscribe(x => UISettings.FontName.Value = x.Value);
        }
    }
}
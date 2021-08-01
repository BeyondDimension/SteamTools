using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class SettingsPageViewModel :
#if !__MOBILE__
        TabItemViewModel
#else
        ViewModelBase
#endif
    {
        public static SettingsPageViewModel Instance { get; } = new();

        public
#if !__MOBILE__
            override
#endif
            string Name
        {
            get => AppResources.Settings;
            protected set { throw new NotImplementedException(); }
        }

        KeyValuePair<string, string> _SelectLanguage;
        public KeyValuePair<string, string> SelectLanguage
        {
            get => _SelectLanguage;
            set => this.RaiseAndSetIfChanged(ref _SelectLanguage, value);
        }

#if !__MOBILE__
        KeyValuePair<string, string> _SelectFont;
        public KeyValuePair<string, string> SelectFont
        {
            get => _SelectFont;
            set => this.RaiseAndSetIfChanged(ref _SelectFont, value);
        }

        IReadOnlyCollection<UpdateChannelType>? _UpdateChannels;
        public IReadOnlyCollection<UpdateChannelType>? UpdateChannels
        {
            get => _UpdateChannels;
            set => this.RaiseAndSetIfChanged(ref _UpdateChannels, value);
        }
#endif

        public SettingsPageViewModel()
        {
#if !__MOBILE__
            IconKey = nameof(SettingsPageViewModel).Replace("ViewModel", "Svg");
#endif

            SelectLanguage = R.Languages.FirstOrDefault(x => x.Key == UISettings.Language.Value);
            this.WhenValueChanged(x => x.SelectLanguage, false)
                  .Subscribe(x => UISettings.Language.Value = x.Key);

#if !__MOBILE__
            SelectFont = R.Fonts.FirstOrDefault(x => x.Value == UISettings.FontName.Value);
            this.WhenValueChanged(x => x.SelectFont, false)
                  .Subscribe(x => UISettings.FontName.Value = x.Value);


            UpdateChannels = Enum.GetValues<UpdateChannelType>();
#endif
        }

#if __MOBILE__
        public static string[] GetThemes() => new[]
        {
            AppResources.Settings_UI_SystemDefault,
            AppResources.Settings_UI_Light,
            AppResources.Settings_UI_Dark,
        };
#endif



    }
}
using Avalonia.Controls;
using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.UI.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase, ILocalizationViewModel
    {
        #region ResStrings

        string mSettings_General = string.Empty;

        [ResString]
        public string Settings_General
        {
            get => mSettings_General;
            set => this.RaiseAndSetIfChanged(ref mSettings_General, value);
        }

        string mSettings_Auth = string.Empty;

        [ResString]
        public string Settings_Auth
        {
            get => mSettings_Auth;
            set => this.RaiseAndSetIfChanged(ref mSettings_Auth, value);
        }

        string mLanguage = string.Empty;

        [ResString]
        public string Language
        {
            get => mLanguage;
            set => this.RaiseAndSetIfChanged(ref mLanguage, value);
        }

        #endregion

        public SettingsPageViewModel()
        {
            Languages = ILocalizationService.Languages.Select(Convert).ToList();
        }

        int mCurrentLanguageIndex;

        public int CurrentLanguageIndex
        {
            get => mCurrentLanguageIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref mCurrentLanguageIndex, value);
                if (!IsInDesignMode)
                {
                    var localizationService = DI.Get<ILocalizationService>();
                    var cultureName = Languages[value].Tag?.ToString();
                    if (string.IsNullOrWhiteSpace(cultureName))
                    {
                        cultureName = localizationService.DefaultCurrentUICulture?.Name;
                    }
                    if (!string.IsNullOrWhiteSpace(cultureName))
                    {
                        localizationService.ChangeLanguage(cultureName);
                    }
                }
            }
        }

        public List<ComboBoxItem> Languages { get; }

        static ComboBoxItem Convert(KeyValuePair<string, string> pair)
            => new ComboBoxItem { Tag = pair.Key, Content = pair.Value };
    }
}
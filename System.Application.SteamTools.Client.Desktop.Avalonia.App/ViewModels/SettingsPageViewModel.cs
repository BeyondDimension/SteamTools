using Avalonia.Controls;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.UI.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public SettingsPageViewModel()
        {
            Languages = R.Languages.Select(Convert).ToList();
        }

        int mCurrentLanguageIndex;

        public int CurrentLanguageIndex
        {
            get => mCurrentLanguageIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref mCurrentLanguageIndex, value);
                var cultureName = Languages[value].Tag?.ToString();
                if (string.IsNullOrWhiteSpace(cultureName))
                {
                    cultureName = R.DefaultCurrentUICulture?.Name;
                }
                if (!string.IsNullOrWhiteSpace(cultureName))
                {
                    R.ChangeLanguage(cultureName);
                }
                //if (!IsInDesignMode)
                //{
                //    var localizationService = DI.Get<ILocalizationService>();
                //    var cultureName = Languages[value].Tag?.ToString();
                //    if (string.IsNullOrWhiteSpace(cultureName))
                //    {
                //        cultureName = localizationService.DefaultCurrentUICulture?.Name;
                //    }
                //    if (!string.IsNullOrWhiteSpace(cultureName))
                //    {
                //        localizationService.ChangeLanguage(cultureName);
                //    }
                //}
            }
        }

        public List<ComboBoxItem> Languages { get; }

        static ComboBoxItem Convert(KeyValuePair<string, string> pair)
            => new ComboBoxItem { Tag = pair.Key, Content = pair.Value };
    }
}
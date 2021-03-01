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
            //Languages = R.Languages.Select(Convert).ToList();
        }

        //int mCurrentLanguageIndex;

        //public int CurrentLanguageIndex
        //{
        //    get => mCurrentLanguageIndex;
        //    set
        //    {
        //        this.RaiseAndSetIfChanged(ref mCurrentLanguageIndex, value);
        //        var cultureName = Languages[value].Tag?.ToString();
        //        if (string.IsNullOrWhiteSpace(cultureName))
        //        {
        //            cultureName = R.DefaultCurrentUICulture?.Name;
        //        }
        //        if (!string.IsNullOrWhiteSpace(cultureName))
        //        {
        //            R.ChangeLanguage(cultureName);
        //        }
        //        //if (!IsInDesignMode)
        //        //{
        //        //    var localizationService = DI.Get<ILocalizationService>();
        //        //    var cultureName = Languages[value].Tag?.ToString();
        //        //    if (string.IsNullOrWhiteSpace(cultureName))
        //        //    {
        //        //        cultureName = localizationService.DefaultCurrentUICulture?.Name;
        //        //    }
        //        //    if (!string.IsNullOrWhiteSpace(cultureName))
        //        //    {
        //        //        localizationService.ChangeLanguage(cultureName);
        //        //    }
        //        //}
        //    }
        //}

        //public List<Object> Languages { get; }

        //static Object Convert(KeyValuePair<string, string> pair)
        //    => new Object { Tag = pair.Key, Content = pair.Value };
    }
}
using MetroRadiance.UI;
using SteamTools.Models.Settings;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SteamTools.ViewModels
{
    public class SettingsPageViewModel : TabItemViewModel
    {
        public static SettingsPageViewModel Instance { get; } = new SettingsPageViewModel();

        public override string Name
        {
            get { return Properties.Resources.Settings; }
            protected set { throw new NotImplementedException(); }
        }


        #region ThemeWindows 

        private bool _ThemeWindows = ThemeService.Current.Theme == Theme.Windows;

        public bool ThemeWindows
        {
            get { return this._ThemeWindows; }
            set
            {
                if (this._ThemeWindows != value)
                {
                    this._ThemeWindows = value;
                    this.RaisePropertyChanged();

                    if (value) { ThemeService.Current.ChangeTheme(Theme.Windows); UISettings.Theme.Value = 0; }
                }
            }
        }

        #endregion

        #region Dark 

        private bool _Dark = ThemeService.Current.Theme == Theme.Dark;

        public bool Dark
        {
            get { return this._Dark; }
            set
            {
                if (this._Dark != value)
                {
                    this._Dark = value;
                    this.RaisePropertyChanged();
                    if (value) { ThemeService.Current.ChangeTheme(Theme.Dark); UISettings.Theme.Value = 1; }
                }
            }
        }

        #endregion

        #region Light 

        private bool _Light = ThemeService.Current.Theme == Theme.Light;

        public bool Light
        {
            get { return this._Light; }
            set
            {
                if (this._Light != value)
                {
                    this._Light = value;
                    this.RaisePropertyChanged();
                    if (value) { ThemeService.Current.ChangeTheme(Theme.Light); UISettings.Theme.Value = 2; }
                }
            }
        }

        #endregion

        #region AccentWindows 

        private bool _AccentWindows = ThemeService.Current.Accent == Accent.Windows;

        public bool AccentWindows
        {
            get { return this._AccentWindows; }
            set
            {
                if (this._AccentWindows != value)
                {
                    this._AccentWindows = value;
                    this.RaisePropertyChanged();

                    if (value) { ThemeService.Current.ChangeAccent(Accent.Windows); UISettings.Accent.Value = 0; }
                }
            }
        }

        #endregion

        #region Purple 

        private bool _Purple = ThemeService.Current.Accent.Specified == Accent.SpecifiedColor.Purple;

        public bool Purple
        {
            get { return this._Purple; }
            set
            {
                if (this._Purple != value)
                {
                    this._Purple = value;
                    this.RaisePropertyChanged();
                    if (value) { ThemeService.Current.ChangeAccent(Accent.Purple); UISettings.Accent.Value = 1; }
                }
            }
        }

        #endregion

        #region Blue 

        private bool _Blue = ThemeService.Current.Accent.Specified == Accent.SpecifiedColor.Blue;

        public bool Blue
        {
            get { return this._Blue; }
            set
            {
                if (this._Blue != value)
                {
                    this._Blue = value;
                    this.RaisePropertyChanged();
                    if (value) { ThemeService.Current.ChangeAccent(Accent.Blue); UISettings.Accent.Value = 2; }
                }
            }
        }

        #endregion

        #region Orange 

        private bool _Orange = ThemeService.Current.Accent.Specified == Accent.SpecifiedColor.Orange;

        public bool Orange
        {
            get { return this._Orange; }
            set
            {
                if (this._Orange != value)
                {
                    this._Orange = value;
                    this.RaisePropertyChanged();
                    if (value) { ThemeService.Current.ChangeAccent(Accent.Orange); UISettings.Accent.Value = 3; }
                }
            }
        }

        #endregion

        #region Red 

        private bool _Red = ThemeService.Current.Accent.Color == Colors.Red;

        public bool Red
        {
            get { return this._Red; }
            set
            {
                if (this._Red != value)
                {
                    this._Red = value;
                    this.RaisePropertyChanged();
                    if (value) { ThemeService.Current.ChangeAccent(Accent.FromColor(Colors.Red)); UISettings.Accent.Value = 4; }
                }
            }
        }

        #endregion

        #region Green 

        private bool _Green = ThemeService.Current.Accent.Color == Colors.Green;

        public bool Green
        {
            get { return this._Green; }
            set
            {
                if (this._Green != value)
                {
                    this._Green = value;
                    this.RaisePropertyChanged();
                    if (value) { ThemeService.Current.ChangeAccent(Accent.FromColor(Colors.Green)); UISettings.Accent.Value = 5; }
                }
            }
        }

        #endregion

        public IReadOnlyCollection<CultureViewModel> Cultures { get; }

        public SettingsPageViewModel()
        {
            this.Cultures = new[] { new CultureViewModel { DisplayName = "(auto)" } }
                .Concat(ResourceService.Current.SupportedCultures
                    .Select(x => new CultureViewModel { DisplayName = x.EnglishName, Name = x.Name })
                    .OrderBy(x => x.DisplayName))
                .ToList();

            switch (UISettings.Theme.Value)
            {
                case 0:
                    ThemeWindows = true;
                    break;
                case 1:
                    Dark = true;
                    break;
                case 2:
                    Light = true;
                    break;
            }
            switch (UISettings.Accent.Value)
            {
                case 0:
                    AccentWindows = true;
                    break;
                case 1:
                    Purple = true;
                    break;
                case 2:
                    Blue = true;
                    break;
                case 3:
                    Orange = true;
                    break;
                case 4:
                    Red = true;
                    break;
                case 5:
                    Green = true;
                    break;
            }
        }
    }

    public class CultureViewModel
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
    }
}

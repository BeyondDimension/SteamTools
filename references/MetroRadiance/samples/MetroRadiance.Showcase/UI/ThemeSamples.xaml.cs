using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Livet;
using MetroRadiance.UI;

namespace MetroRadiance.Showcase.UI
{
	public partial class ThemeSamples
	{
		public ThemeSamples()
		{
			this.InitializeComponent();
		}
	}

	public class ThemeViewModel : ViewModel
	{
		#region Windows 変更通知プロパティ

		private bool _Windows = ThemeService.Current.Theme == Theme.Windows;

		public bool Windows
		{
			get { return this._Windows; }
			set
			{
				if (this._Windows != value)
				{
					this._Windows = value;
					this.RaisePropertyChanged();

					if (value) ThemeService.Current.ChangeTheme(Theme.Windows);
				}
			}
		}

		#endregion

		#region Light 変更通知プロパティ

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

					if (value) ThemeService.Current.ChangeTheme(Theme.Light);
				}
			}
		}

		#endregion

		#region Dark 変更通知プロパティ

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

					if (value) ThemeService.Current.ChangeTheme(Theme.Dark);
				}
			}
		}

		#endregion
	}

	public class AccentViewModel : ViewModel
	{
		#region Windows 変更通知プロパティ

		private bool _Windows = ThemeService.Current.Accent.SyncToWindows;

		public bool Windows
		{
			get { return this._Windows; }
			set
			{
				if (this._Windows != value)
				{
					this._Windows = value;
					this.RaisePropertyChanged();

					if (value) ThemeService.Current.ChangeAccent(Accent.Windows);
				}
			}
		}

		#endregion

		#region Purple 変更通知プロパティ

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

					if (value) ThemeService.Current.ChangeAccent(Accent.Purple);
				}
			}
		}

		#endregion

		#region Blue 変更通知プロパティ

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

					if (value) ThemeService.Current.ChangeAccent(Accent.Blue);
				}
			}
		}

		#endregion

		#region Orange 変更通知プロパティ

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

					if (value) ThemeService.Current.ChangeAccent(Accent.Orange);
				}
			}
		}

		#endregion

		#region Red 変更通知プロパティ

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

					if (value) ThemeService.Current.ChangeAccent(Accent.FromColor(Colors.Red));
				}
			}
		}

		#endregion
	}
}

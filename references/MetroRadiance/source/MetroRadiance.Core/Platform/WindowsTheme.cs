using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MetroRadiance.Platform
{
	/// <summary>
	/// Windows OS のテーマ機能へアクセスできるようにします。
	/// </summary>
	public static class WindowsTheme
	{
		static WindowsTheme()
		{
			var version = Environment.OSVersion.Version;
			if (version.Major == 10)
			{
				if (version.Build >= 18282)
				{
					Theme = new ThemeValue();
					SystemTheme = new SystemThemeValue();
				}
				else if (version.Build >= 14316)
				{
					Theme = new ThemeValue();
					SystemTheme = new WindowsThemeConstantValue<Theme>(Platform.Theme.Dark);
				}
				else
				{
					Theme = new WindowsThemeConstantValue<Theme>(Platform.Theme.Light);
					SystemTheme = new WindowsThemeConstantValue<Theme>(Platform.Theme.Dark);
				}
				ColorPrevalence = new ColorPrevalenceValue();
				Transparency = new TransparencyValueWindows10();
			}
			else
			{
				Theme = new WindowsThemeConstantValue<Theme>(Platform.Theme.Light);
				SystemTheme = new WindowsThemeConstantValue<Theme>(Platform.Theme.Light);
				ColorPrevalence = new WindowsThemeConstantValue<bool>(true);
				if (version.Major == 6 && version.Minor == 0
					|| version.Major == 6 && version.Minor == 1)
				{
					Transparency = new TransparencyValueWindowsVistaOr7();
				}
				else
				{
					Transparency = new WindowsThemeConstantValue<bool>(false);
				}
			}
		}

		/// <summary>
		/// Windows の既定のアプリテーマ設定と、その変更通知機能へアクセスできるようにします。
		/// </summary>
		public static IWindowsThemeValue<Theme> Theme { get; }

		/// <summary>
		/// Windows の既定のシステムテーマ設定と、その変更通知機能へアクセスできるようにします。
		/// </summary>
		public static IWindowsThemeValue<Theme> SystemTheme { get; }

		/// <summary>
		/// Windows のアクセント カラー設定と、その変更通知機能へアクセスできるようにします。
		/// </summary>
		public static IWindowsThemeValue<Color> Accent { get; } = new AccentValue();

		public static IWindowsThemeValue<bool> HighContrast { get; } = new HighContrastValue();

		public static IWindowsThemeValue<bool> ColorPrevalence { get; }

		public static IWindowsThemeValue<bool> Transparency { get; }
		
		/// <summary>
		/// Windows の文字の大きさ設定と、その変更通知機能へアクセスできるようにします。
		/// </summary>
		public static IWindowsThemeValue<double> TextScaleFactor { get; } = new TextScaleFactorValue();
	}
}

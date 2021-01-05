using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using MetroRadiance.Interop.Win32;
using MetroRadiance.Media;
using Microsoft.Win32;

namespace MetroRadiance.Platform
{
	public enum Theme
	{
		Dark = 0,
		Light = 1,
	}

	internal class ThemeValue : WindowsThemeValue<Theme>
	{
		protected override Theme GetValue()
		{
			const string keyName = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
			const string valueName = "AppsUseLightTheme";

			return Registry.GetValue(keyName, valueName, null) as int? == 0 ? Theme.Dark : Theme.Light;
		}

		protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_SETTINGCHANGE)
			{
				var systemParmeter = Marshal.PtrToStringAuto(lParam);
				if (systemParmeter == "ImmersiveColorSet")
				{
					this.Update(this.GetValue());
					handled = true;
				}
			}

			return IntPtr.Zero;
		}
	}

	internal sealed class SystemThemeValue : ThemeValue
	{
		protected override Theme GetValue()
		{
			const string keyName = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
			const string valueName = "SystemUsesLightTheme";

			return (Registry.GetValue(keyName, valueName, null) as int? ?? (int)base.GetValue()) == 0 ? Theme.Dark : Theme.Light;
		}
	}

	internal sealed class AccentValue : WindowsThemeValue<Color>
	{
		protected override Color GetValue()
		{
			const string keyName = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\DWM";
			const string valueName = "ColorizationColor";
			uint color;

			var colorizationColor = Registry.GetValue(keyName, valueName, null) as int?;
			if (colorizationColor != null)
			{
				color = (uint)colorizationColor.Value;
			}
			else
			{
				bool opaque;
				// Note: return the modified value on Windows Vista & 7
				Dwmapi.DwmGetColorizationColor(out color, out opaque);
			}

			return ColorHelper.GetColorFromUInt32(color);
		}

		protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_DWMCOLORIZATIONCOLORCHANGED)
			{
				this.Update(this.GetValue());
				handled = true;
			}

			return IntPtr.Zero;
		}
	}


	internal sealed class HighContrastValue : WindowsThemeValue<bool>
	{
		protected override bool GetValue() => System.Windows.SystemParameters.HighContrast;

		protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_THEMECHANGED)
			{
				this.Update(this.GetValue());
				handled = true;
			}

			return IntPtr.Zero;
		}
	}

	internal sealed class ColorPrevalenceValue : WindowsThemeValue<bool>
	{
		protected override bool GetValue()
		{
			const string keyName = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
			const string valueName = "ColorPrevalence";

			return Registry.GetValue(keyName, valueName, null) as int? != 0;
		}

		protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_SETTINGCHANGE)
			{
				var systemParmeter = Marshal.PtrToStringAuto(lParam);
				if (systemParmeter == "ImmersiveColorSet")
				{
					this.Update(this.GetValue());
					handled = true;
				}
			}

			return IntPtr.Zero;
		}
	}

	internal sealed class TransparencyValueWindows10 : WindowsThemeValue<bool>
	{
		protected override bool GetValue()
		{
			const string keyName = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
			const string valueName = "EnableTransparency";

			return Registry.GetValue(keyName, valueName, null) as int? != 0;
		}

		protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_SETTINGCHANGE)
			{
				var systemParmeter = Marshal.PtrToStringAuto(lParam);
				if (systemParmeter == "ImmersiveColorSet")
				{
					this.Update(this.GetValue());
					handled = true;
				}
			}

			return IntPtr.Zero;
		}
	}

	internal sealed class TransparencyValueWindowsVistaOr7 : WindowsThemeValue<bool>
	{
		protected override bool GetValue()
		{
			uint color;
			bool opaque;
			Dwmapi.DwmGetColorizationColor(out color, out opaque);
			return !opaque;
		}

		protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_DWMCOLORIZATIONCOLORCHANGED)
			{
				var opaque = Convert.ToBoolean((long)lParam);
				this.Update(!opaque);
				handled = true;
			}

			return IntPtr.Zero;
		}
	}

	internal sealed class TextScaleFactorValue : WindowsThemeValue<double>
	{
		protected override double GetValue()
		{
			const string keyName = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Accessibility";
			const string valueName = "TextScaleFactor";

			return (Registry.GetValue(keyName, valueName, null) as int? ?? 100) / 100.0;
		}

		protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_SETTINGCHANGE)
			{
				var systemParmeter = Marshal.PtrToStringAuto(lParam);
				if (systemParmeter == "WindowsThemeElement")
				{
					this.Update(this.GetValue());
					handled = true;
				}
			}

			return IntPtr.Zero;
		}
	}
}

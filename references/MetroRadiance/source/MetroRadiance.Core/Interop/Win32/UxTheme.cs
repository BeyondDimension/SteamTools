using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MetroRadiance.Interop.Win32
{
	public static class UxTheme
	{
		[DllImport("uxtheme.dll", EntryPoint = "#94")]
		internal static extern int GetImmersiveColorSetCount();

		[DllImport("uxtheme.dll", EntryPoint = "#95")]
		internal static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);

		[DllImport("uxtheme.dll", EntryPoint = "#96", CharSet = CharSet.Unicode)]
		internal static extern uint GetImmersiveColorTypeFromName(string name);

		[DllImport("uxtheme.dll", EntryPoint = "#98")]
		internal static extern uint GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

		[DllImport("uxtheme.dll", EntryPoint = "#100", CharSet = CharSet.Unicode)]
		internal static extern IntPtr GetImmersiveColorNamedTypeByIndex(uint dwIndex);

		[DllImport("uxtheme.dll", EntryPoint = "#104")]
		internal static extern void RefreshImmersiveColorPolicyState(); // RS5 1809

		[DllImport("uxtheme.dll", EntryPoint = "#132")]
		internal static extern bool ShouldAppsUseDarkMode(); // RS5 1809

		[DllImport("uxtheme.dll", EntryPoint = "#133")]
		internal static extern bool AllowDarkModeForWindow(IntPtr hWnd, bool bAllowDarkMode); // RS5 1809

		[DllImport("uxtheme.dll", EntryPoint = "#135")]
		internal static extern bool AllowDarkModeForApp(bool bAllowDarkMode); // RS5 1809

		[DllImport("uxtheme.dll", EntryPoint = "#137")]
		internal static extern bool IsDarkModeAllowedForWindow(); // RS5 1809

		[DllImport("uxtheme.dll", EntryPoint = "#135")]
		internal static extern PreferredAppMode SetPreferredAppMode(PreferredAppMode appMode); // RS6 1903

		[DllImport("uxtheme.dll", EntryPoint = "#138")]
		internal static extern bool ShouldSystemUseDarkMode(); // RS6 1903

		[DllImport("uxtheme.dll", EntryPoint = "#139")]
		internal static extern bool IsDarkModeAllowedForApp(); // RS6 1903
	}
}

using System;
using System.Linq;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Platform
{
	public static class AppMode
	{	
		public static bool SetAppMode(PreferredAppMode appMode)
		{
			var build = Environment.OSVersion.Version.Build;
			if (build >= 18362)
			{
				// memo: Workaround for the problem of not switching from dark theme to light theme.
				if (appMode == PreferredAppMode.APPMODE_DEFAULT || appMode == PreferredAppMode.APPMODE_FORCELIGHT)
				{
					UxTheme.SetPreferredAppMode(PreferredAppMode.APPMODE_ALLOWDARK);
					UxTheme.RefreshImmersiveColorPolicyState();
				}

				var ret = UxTheme.SetPreferredAppMode(appMode) == appMode;
				UxTheme.RefreshImmersiveColorPolicyState();
				return ret;
			}
			else if (build >= 17763)
			{
				if (appMode == PreferredAppMode.APPMODE_DEFAULT || appMode == PreferredAppMode.APPMODE_FORCELIGHT)
				{
					return UxTheme.AllowDarkModeForApp(false);
				}
				else
				{
					return UxTheme.AllowDarkModeForApp(true);
				}
			}
			return false;
		}
	}
}

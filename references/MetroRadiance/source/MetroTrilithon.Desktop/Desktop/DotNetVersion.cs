using System;
using Microsoft.Win32;

// This example displays output like the following:
//       .NET Framework Version: 4.6.1
// 
// see also: https://docs.microsoft.com/ja-jp/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed

namespace MetroTrilithon.Desktop
{
	public class DotNetVersion
	{
		public static string GetVersion()
		{
			const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
			var version = "";
			
			using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
			{
				if (ndpKey?.GetValue("Release") is int value)
				{
					version = GetVersionCore(value);
				}
			}

			if (string.IsNullOrEmpty(version))
			{
				version = Environment.Version.ToString();
			}

			return $".NET Framework Version: {version}";
		}

		private static string GetVersionCore(int releaseKey)
		{
			if (releaseKey >= 461808) return "4.7.2 or later";
			if (releaseKey >= 461308) return "4.7.1";
			if (releaseKey >= 460798) return "4.7";
			if (releaseKey >= 394802) return "4.6.2";
			if (releaseKey >= 394254) return "4.6.1";
			if (releaseKey >= 393295) return "4.6";
			if (releaseKey >= 379893) return "4.5.2";
			if (releaseKey >= 378675) return "4.5.1";
			if (releaseKey >= 378389) return "4.5";

			return "";
		}
	}
}

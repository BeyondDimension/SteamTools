using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SteamTools.Models.Settings
{
	public static class GeneralSettings
	{


		private static string GetKey([CallerMemberName] string propertyName = "")
		{
			return nameof(GeneralSettings) + "." + propertyName;
		}
	}
}

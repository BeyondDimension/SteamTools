using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SteamTools.Win32;

namespace SteamTools.Models
{
	internal static class Helper
	{
		static Helper()
		{
			var version = Environment.OSVersion.Version;
			IsWindows8OrGreater = (version.Major == 6 && version.Minor >= 2) || version.Major > 6;
		}


		/// <summary>
		/// 检查它是否在Windows 8或更高版本上运行。
		/// </summary>
		public static bool IsWindows8OrGreater { get; private set; }

		/// <summary>
		/// 获取它是否正在设计器的上下文中运行。
		/// </summary>
		public static bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());
		

		public static void SetMMCSSTask()
		{
			var index = 0u;
			NativeMethods.AvSetMmThreadCharacteristics("Games", ref index);
		}

		public static Color StringToColor(string colorCode)
		{
			try
			{
				if (colorCode.StartsWith("#"))
				{
					if (colorCode.Length == 7)
					{
						// #rrggbb style
						return Color.FromRgb(
							Convert.ToByte(colorCode.Substring(1, 2), 16),
							Convert.ToByte(colorCode.Substring(3, 2), 16),
							Convert.ToByte(colorCode.Substring(5, 2), 16));
					}
					if (colorCode.Length == 9)
					{
						// #aarrggbb style
						return Color.FromArgb(
							Convert.ToByte(colorCode.Substring(1, 2), 16),
							Convert.ToByte(colorCode.Substring(3, 2), 16),
							Convert.ToByte(colorCode.Substring(5, 2), 16),
							Convert.ToByte(colorCode.Substring(7, 2), 16));
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}

			return Colors.Transparent;
		}

	}
}

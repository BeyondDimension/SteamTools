using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MetroRadiance.UI
{
	public static class ThemeExtensions
	{
		/// <summary>
		/// 現在の <see cref="Color"/> 構造体から、MetroRadiance の <see cref="ThemeService"/> で使用する <see cref="Accent"/> を作成します。
		/// </summary>
		public static Accent ToAccent(this Color color)
		{
			return Accent.FromColor(color);
		}
	}
}

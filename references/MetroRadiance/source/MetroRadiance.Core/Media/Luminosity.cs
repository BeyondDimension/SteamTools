using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MetroRadiance.Media
{
	public class Luminosity
	{
		/// <summary>
		/// RGB 色空間で表現される色から輝度を求めます。
		/// </summary>
		public static byte FromRgb(Color c)
		{
			return (byte)(c.R * 0.299 + c.G * 0.587 + c.B * 0.114);
		}
	}
}

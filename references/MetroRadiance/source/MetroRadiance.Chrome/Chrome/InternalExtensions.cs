using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroRadiance.Interop;

namespace MetroRadiance.Chrome
{
	internal static class InternalExtensions
	{
		public static int DpiRoundX(this double value, Dpi dpi)
		{
			return (int)Math.Round(value * dpi.ScaleX, MidpointRounding.AwayFromZero);
		}

		public static int DpiRoundY(this double value, Dpi dpi)
		{
			return (int)Math.Round(value * dpi.ScaleY, MidpointRounding.AwayFromZero);
		}

		public static bool IsAuto(this double value)
		{
			return double.IsNaN(value);
		}

		/// <summary>
		/// 現在の値が Auto の場合は <paramref name="default"/> を、それ以外の場合は現在の値を返します。
		/// </summary>
		public static double SpecifiedOrDefault(this double value, double @default)
		{
			return double.IsNaN(value) ? @default : value;
		}

		public static void Dump(this object obj)
		{
			System.Diagnostics.Debug.WriteLine(obj);
		}
	}
}

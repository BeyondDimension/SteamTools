using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MetroRadiance.Interop
{
	public static class Extensions
	{
		/// <summary>
		/// 現在の <see cref="Visual"/> から、WPF が認識しているシステム DPI を取得します。
		/// </summary>
		/// <returns>
		/// X 軸 および Y 軸それぞれの DPI 設定値を表す <see cref="Dpi"/> 構造体。
		/// </returns>
		public static Dpi? GetSystemDpi(this Visual visual)
		{
			var source = PresentationSource.FromVisual(visual);
			if (source?.CompositionTarget != null)
			{
				return new Dpi(
					(uint)(Dpi.Default.X * source.CompositionTarget.TransformToDevice.M11),
					(uint)(Dpi.Default.Y * source.CompositionTarget.TransformToDevice.M22));
			}

			return null;
		}
	}
}

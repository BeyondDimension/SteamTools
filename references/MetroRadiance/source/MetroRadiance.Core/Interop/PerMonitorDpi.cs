using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interop;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Interop
{
	/// <summary>
	/// Windows 8.1 の Per-Monitor DPI 機能へアクセスできるようにします。
	/// </summary>
	public static class PerMonitorDpi
	{
		/// <summary>
		/// Per-Monitor DPI 機能をサポートしているかどうかを示す値を取得します。
		/// </summary>
		/// <returns>
		/// 動作しているオペレーティング システムが Windows 8.1 (NT 6.3)、もしくは Windows 10 (10.0.x) の場合は true、それ以外の場合は false。
		/// </returns>
		public static bool IsSupported
		{
			get
			{
#if DEBUG
				return true;
#else
				var version = Environment.OSVersion.Version;
				return (version.Major == 6 && version.Minor == 3) || version.Major == 10;
#endif
			}
		}

		/// <summary>
		/// 現在の <see cref="HwndSource"/> が描画されているモニターの DPI 設定値を取得します。
		/// </summary>
		/// <param name="hwndSource">DPI を取得する対象の Win32 ウィンドウを特定する <see cref="HwndSource"/> オブジェクト。</param>
		/// <param name="dpiType">DPI の種類。既定値は <see cref="MonitorDpiType.Default"/> (<see cref="MonitorDpiType.EffectiveDpi"/> と同値) です。</param>
		/// <returns><paramref name="hwndSource"/> が描画されているモニターの DPI 設定値。サポートされていないシステムの場合は <see cref="Dpi.Default"/>。</returns>
		public static Dpi GetDpi(this HwndSource hwndSource, MonitorDpiType dpiType = MonitorDpiType.Default)
		{
			return GetDpi(hwndSource.Handle, dpiType);
		}

		/// <summary>
		/// 指定したハンドルのウィンドウが描画されているモニターの DPI 設定値を取得します。
		/// </summary>
		/// <param name="hWnd">DPI を取得する対象の Win32 ウィンドウを示すウィンドウ ハンドル。</param>
		/// <param name="dpiType">DPI の種類。既定値は <see cref="MonitorDpiType.Default"/> (<see cref="MonitorDpiType.EffectiveDpi"/> と同値) です。</param>
		/// <returns><paramref name="hWnd"/> のウィンドウが描画されているモニターの DPI 設定値。サポートされていないシステムの場合は <see cref="Dpi.Default"/>。</returns>
		public static Dpi GetDpi(IntPtr hWnd, MonitorDpiType dpiType = MonitorDpiType.Default)
		{
			if (!IsSupported) return Dpi.Default;

			var hmonitor = User32.MonitorFromWindow(
				hWnd,
				MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);

			uint dpiX = 1, dpiY = 1;
			SHCore.GetDpiForMonitor(hmonitor, dpiType, ref dpiX, ref dpiY);

			return new Dpi(dpiX, dpiY);
		}
	}
}

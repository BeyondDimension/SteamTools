// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	public enum ShowWindowFlags
	{
		/// <summary>
		/// ウィンドウを非表示にして、ほかのウィンドウをアクティブにします。
		/// </summary>
		SW_HIDE = 0,

		/// <summary>
		/// ウィンドウをアクティブにし、表示します。ウィンドウが最小化、または最大化されていると、ウィンドウは元の位置とサイズで復元されます。
		/// </summary>
		SW_SHOWNORMAL = 1,

		/// <summary>
		/// ウィンドウをアクティブにし、アイコンとして表示します。
		/// </summary>
		SW_SHOWMINIMIZED = 2,

		/// <summary>
		/// ウィンドウをアクティブにし、最大化します。
		/// </summary>
		SW_SHOWMAXIMIZED = 3,

		/// <summary>
		/// ウィンドウをアイコンとして表示します。 現在アクティブなウィンドウはアクティブなまま表示します。
		/// </summary>
		SW_SHOWNOACTIVATE = 4,

		/// <summary>
		/// ウィンドウをアクティブにし、現在のサイズと位置で表示します。
		/// </summary>
		SW_SHOW = 5,

		/// <summary>
		/// 指定されたウィンドウを最小化して、システム リストにあるトップレベル ウィンドウをアクティブにします。
		/// </summary>
		SW_MINIMIZE = 6,

		/// <summary>
		/// ウィンドウを直前のサイズと位置で表示します。 現在アクティブなウィンドウはアクティブなまま表示します。
		/// </summary>
		SW_SHOWMINNOACTIVE = 7,

		/// <summary>
		/// ウィンドウを現在の状態で表示します。 現在アクティブなウィンドウはアクティブなまま表示します。
		/// </summary>
		SW_SHOWNA = 8,

		/// <summary>
		/// ウィンドウをアクティブにし、表示します。 ウィンドウが最小化、または最大化されていると、ウィンドウは元の位置とサイズに復元されます。
		/// </summary>
		SW_RESTORE = 9,

		/// <summary>
		/// アプリケーションを起動したプログラムが 関数に渡した 構造体で指定された SW_ フラグに従って表示状態を設定します。
		/// </summary>
		SW_SHOWDEFAULT = 10,
	}
}

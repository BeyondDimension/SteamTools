// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	public enum ClassLongPtrIndex
	{
		/// <summary>
		/// ウィンドウクラスを一意的に識別するアトム値を取得します。これは、RegisterClassEx 関数が返すアトムと同じです。
		/// </summary>
		GCW_ATOM = -32,

		/// <summary>
		/// クラスに関連付けられた拡張クラスメモリのサイズをバイト単位で取得します。
		/// </summary>
		GCL_CBCLSEXTRA = -20,

		/// <summary>
		/// ウィンドウに関連付けられた拡張ウィンドウメモリのサイズをバイト単位で取得します。
		/// </summary>
		GCL_CBWNDEXTRA = -18,

		/// <summary>
		/// クラスに関連付けられた背景ブラシのハンドルを取得します。
		/// </summary>
		GCLP_HBRBACKGROUND = -10,

		/// <summary>
		/// クラスに関連付けられたカーソルのハンドルを取得します。
		/// </summary>
		GCLP_HCURSOR = -12,

		/// <summary>
		/// クラスに関連付けられたアイコンのハンドルを取得します。
		/// </summary>
		GCLP_HICON = -14,

		/// <summary>
		/// クラスに関連付けられた小さいアイコンのハンドルを取得します。
		/// </summary>
		GCLP_HICONSM = -34,

		/// <summary>
		/// クラスを登録したモジュールのハンドルを取得します。
		/// </summary>
		GCLP_HMODULE = -16,

		/// <summary>
		/// クラスに関連付けられたメニューリソースを識別するメニュー名文字列へのポインタを取得します。
		/// </summary>
		GCLP_MENUNAME = -8,

		/// <summary>
		/// ウィンドウクラスのスタイルビットを取得します。
		/// </summary>
		GCL_STYLE = -26,

		/// <summary>
		/// ウィンドウプロシージャのアドレス、またはウィンドウプロシージャのアドレスを表すハンドルを取得します。ウィンドウプロシージャを呼び出すには、CallWindowProc 関数を使わなければなりません。
		/// </summary>
		GCLP_WNDPROC = -24,
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroRadiance.UI.Controls
{
	/// <summary>
	/// ウィンドウ操作を示す識別子を定義します。
	/// </summary>
	public enum WindowAction
	{
		/// <summary>
		/// ウィンドウ操作は実行されません。
		/// </summary>
		None,

		/// <summary>
		/// ウィンドウをアクティブ化します。
		/// </summary>
		Active,

		/// <summary>
		/// ウィンドウを閉じます。
		/// </summary>
		Close,

		/// <summary>
		/// ウィンドウを通常状態にします。
		/// </summary>
		Normalize,

		/// <summary>
		/// ウィンドウを最大化します。
		/// </summary>
		Maximize,

		/// <summary>
		/// ウィンドウを最小化します。
		/// </summary>
		Minimize,

		/// <summary>
		/// ウィンドウのシステム メニューを開きます。
		/// </summary>
		OpenSystemMenu,
	}
}

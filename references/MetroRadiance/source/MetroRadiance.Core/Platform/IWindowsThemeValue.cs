using System;
using System.ComponentModel;
using MetroRadiance.Utilities;

namespace MetroRadiance.Platform
{
	public interface IWindowsThemeValue<T>
	{
		/// <summary>
		/// 設定値が動的に変更されるかを取得します。
		/// </summary>
		bool IsDynamic { get; }

		/// <summary>
		/// 現在の設定値を取得します。
		/// </summary>
		T Current { get; }

		/// <summary>
		/// テーマ設定が変更されると発生します。
		/// </summary>
		event EventHandler<T> Changed;
	}

	public static class IWindowsThemeValueExtensions
	{
		/// <summary>
		/// テーマ設定が変更されたときに通知を受け取るメソッドを登録します。
		/// </summary>
		/// <param name="callback">テーマ設定が変更されたときに通知を受け取るメソッド。</param>
		/// <returns>通知の購読を解除するときに使用する <see cref="IDisposable"/> オブジェクト。</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IDisposable RegisterListener<T>(this IWindowsThemeValue<T> that, Action<T> callback)
		{
			EventHandler<T> handler = (sender, e) => callback?.Invoke(e);
			that.Changed += handler;

			return Disposable.Create(() => that.Changed -= handler);
		}
	}
}

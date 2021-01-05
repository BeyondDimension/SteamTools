using System;

namespace MetroRadiance.Platform
{
	public sealed class WindowsThemeConstantValue<T> : IWindowsThemeValue<T>
	{
		internal WindowsThemeConstantValue(T constant)
		{
			this.Current = constant;
		}

		/// <summary>
		/// 設定値が動的に変更されるかを取得します。
		/// </summary>
		public bool IsDynamic => false;

		/// <summary>
		/// 現在の設定値を取得します。
		/// </summary>
		public T Current { get; }

		/// <summary>
		/// テーマ設定が変更されると発生します。
		/// </summary>
		#pragma warning disable 67
		public event EventHandler<T> Changed;
		#pragma warning restore 67
	}
}

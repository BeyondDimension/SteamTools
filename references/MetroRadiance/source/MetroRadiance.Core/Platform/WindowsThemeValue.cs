using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interop;
using MetroRadiance.Utilities;

namespace MetroRadiance.Platform
{
	public abstract class WindowsThemeValue<T> : IWindowsThemeValue<T>
	{
		private event EventHandler<T> _changedEvent;
		private readonly HashSet<EventHandler<T>> _handlers = new HashSet<EventHandler<T>>();
		private ListenerWindow _listenerWindow;
		private T _current;
		private bool _hasCache;

		private bool RequireCallGetValue => !this._hasCache || this._listenerWindow == null;

		/// <summary>
		/// 設定値が動的に変更されるかを取得します。
		/// </summary>
		public bool IsDynamic => true;

		/// <summary>
		/// 現在の設定値を取得します。
		/// </summary>
		public T Current
		{
			get
			{
				if (this.RequireCallGetValue)
				{
					this._current = this.GetValue();
					this._hasCache = true;
				}

				return this._current;
			}
			protected set
			{
				this._current = value;
				this._hasCache = true;
			}
		}

		/// <summary>
		/// テーマ設定が変更されると発生します。
		/// </summary>
		public event EventHandler<T> Changed
		{
			add { this.Add(value); }
			remove { this.Remove(value); }
		}

		private void Add(EventHandler<T> listener)
		{
			if (this._handlers.Add(listener))
			{
				this._changedEvent += listener;

				if (this._listenerWindow == null)
				{
					this._listenerWindow = new ListenerWindow(this.GetType().Name, this.WndProc);
					this._listenerWindow.Show();
				}
			}
		}

		private void Remove(EventHandler<T> listener)
		{
			if (this._handlers.Remove(listener))
			{
				this._changedEvent -= listener;

				if (this._handlers.Count == 0)
				{
					this._listenerWindow?.Close();
					this._listenerWindow = null;
					this._hasCache = false;
				}
			}
		}

		protected void Update(T data)
		{
			this.Current = data;
			this._changedEvent?.Invoke(this, data);
		}

		protected abstract T GetValue();

		protected abstract IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);

		private class ListenerWindow : TransparentWindow
		{
			private readonly HwndSourceHook _hook;

			public ListenerWindow(string name, HwndSourceHook hook)
			{
				this.Name = $"{name} listener window";
				this._hook = hook;
			}

			protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				var result = this._hook(hwnd, msg, wParam, lParam, ref handled);
				return handled ? result : base.WndProc(hwnd, msg, wParam, lParam, ref handled);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using Livet;
using Livet.Messaging;
using Livet.Messaging.Windows;
using MetroTrilithon.Threading.Tasks;
using MetroTrilithon.UI.Interactivity;

namespace MetroTrilithon.Mvvm
{
	/// <summary>
	/// <see cref="Window"/> またはその派生型のためのデータを提供します。
	/// </summary>
	public class WindowViewModel : ViewModel
	{
		#region Title 変更通知プロパティ

		private string _Title;

		/// <summary>
		/// ウィンドウ タイトルを取得または設定します。
		/// </summary>
		public string Title
		{
			get { return this._Title; }
			set
			{
				if (this._Title != value)
				{
					this._Title = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region CanClose 変更通知プロパティ

		private bool _CanClose = true;

		/// <summary>
		/// ウィンドウを閉じることができるかどうかを示す値を取得または設定します。
		/// </summary>
		public virtual bool CanClose
		{
			get { return this._CanClose; }
			set
			{
				if (this._CanClose != value)
				{
					this._CanClose = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsClosed 変更通知プロパティ

		private bool _IsClosed;

		/// <summary>
		/// アタッチされたウィンドウが閉じられたかどうかを示す値を取得します。
		/// </summary>
		public bool IsClosed
		{
			get { return this._IsClosed; }
			private set
			{
				if (this._IsClosed != value)
				{
					this._IsClosed = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		/// <summary>
		/// <see cref="InitializeCore"/> メソッドが呼ばれたかどうか (通常、これはアタッチされたウィンドウの
		/// <see cref="Window.ContentRendered"/> イベントによって呼び出されます) を示す値を取得します。
		/// </summary>
		public bool IsInitialized { get; private set; }

		public bool DialogResult { get; protected set; }

		public WindowState WindowState { get; set; }


		/// <summary>
		/// このメソッドは、アタッチされたウィンドウで <see cref="Window.ContentRendered"/>
		/// イベントが発生したときに、Livet インフラストラクチャによって呼び出されます。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Initialize()
		{
			if (this.IsClosed) return;

			this.DialogResult = false;

			this.InitializeCore();
			this.IsInitialized = true;
		}

		/// <summary>
		/// 派生クラスでオーバーライドされると、アタッチされたウィンドウで <see cref="Window.ContentRendered"/>
		/// イベントが発生したときに呼び出される初期化処理を実行します。
		/// </summary>
		protected virtual void InitializeCore() { }

		/// <summary>
		/// このメソッドは、アタッチされたウィンドウで <see cref="Window.Closing"/>
		/// イベントがキャンセルされたときに、Livet インフラストラクチャによって呼び出されます。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void CloseCanceledCallback()
		{
			this.CloseCanceledCallbackCore();
		}

		/// <summary>
		/// 派生クラスでオーバーライドされると、アタッチされたウィンドウで <see cref="Window.Closing"/>
		/// イベントがキャンセルされたときに <see cref="Livet.Behaviors.WindowCloseCancelBehavior"/>
		/// によって呼び出されるコールバック処理を実行します。
		/// </summary>
		protected virtual void CloseCanceledCallbackCore() { }


		/// <summary>
		/// ウィンドウをアクティブ化することを試みます。最小化されている場合は通常状態にします。
		/// </summary>
		public virtual void Activate()
		{
			if (this.WindowState == WindowState.Minimized)
			{
				this.SendWindowAction(WindowAction.Normal);
			}

			this.SendWindowAction(WindowAction.Active);
		}

		/// <summary>
		/// ウィンドウを閉じます。
		/// </summary>
		public virtual void Close()
		{
			if (this.IsClosed) return;

			this.SendWindowAction(WindowAction.Close);
		}

		protected override void Dispose(bool disposing)
		{
			this.IsClosed = true;
			this.IsInitialized = false;

			base.Dispose(disposing);
		}

		protected void SendWindowAction(WindowAction action)
		{
			this.Messenger.Raise(new WindowActionMessage(action, "Window.WindowAction"));
		}

		protected void Transition(ViewModel viewModel, Type windowType, TransitionMode mode, bool isOwned)
		{
			var message = new TransitionMessage(windowType, viewModel, mode, isOwned ? "Window.Transition.Child" : "Window.Transition");
			this.Messenger.Raise(message);
		}

		protected void UpdateTaskbar(TaskbarItemProgressState state, double value)
		{
			var message = new TaskbarMessage("Window.UpdateTaskbar")
			{
				ProgressState = state,
				ProgressValue = value,
			};
			
			this.Messenger.RaiseAsync(message).Forget();
		}

		protected void InvokeOnUIDispatcher(Action action)
		{
			DispatcherHelper.UIDispatcher.BeginInvoke(action);
		}
	}
}

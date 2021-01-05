using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MetroRadiance.Utilities;

namespace MetroRadiance.UI.Controls
{
	/// <summary>
	/// ウィンドウのキャプション部分で使用するために最適化された <see cref="Button"/> コントロールを表します。
	/// </summary>
	public class CaptionButton : Button
	{
		static CaptionButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionButton), new FrameworkPropertyMetadata(typeof(CaptionButton)));
		}

		private Window owner;

		#region WindowAction 依存関係プロパティ

		/// <summary>
		/// ボタンに割り当てるウィンドウ操作を取得または設定します。
		/// </summary>
		public WindowAction WindowAction
		{
			get { return (WindowAction)this.GetValue(WindowActionProperty); }
			set { this.SetValue(WindowActionProperty, value); }
		}
		public static readonly DependencyProperty WindowActionProperty =
			DependencyProperty.Register("WindowAction", typeof(WindowAction), typeof(CaptionButton), new UIPropertyMetadata(WindowAction.None));

		#endregion
		
		#region Mode 依存関係プロパティ

		public CaptionButtonMode Mode
		{
			get { return (CaptionButtonMode)this.GetValue(ModeProperty); }
			set { this.SetValue(ModeProperty, value); }
		}
		public static readonly DependencyProperty ModeProperty =
			DependencyProperty.Register("Mode", typeof(CaptionButtonMode), typeof(CaptionButton), new UIPropertyMetadata(CaptionButtonMode.Normal));	

		#endregion

		#region IsChecked 依存関係プロパティ

		public bool IsChecked
		{
			get { return (bool)this.GetValue(IsCheckedProperty); }
			set { this.SetValue(IsCheckedProperty, value); }
		}
		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register("IsChecked", typeof(bool), typeof(CaptionButton), new UIPropertyMetadata(false));

		#endregion

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.owner = Window.GetWindow(this);
			if (this.owner != null)
			{
				this.owner.StateChanged += (sender, args) => this.ChangeVisibility();
				this.ChangeVisibility();
			}
		}

		protected override void OnClick()
		{
			this.WindowAction.Invoke(this);

			if (this.Mode == CaptionButtonMode.Toggle) this.IsChecked = !this.IsChecked;

			base.OnClick();
		}

		private void ChangeVisibility()
		{
			switch (this.WindowAction)
			{
				case WindowAction.Maximize:
					this.Visibility = this.owner.WindowState != WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
					break;
				case WindowAction.Minimize:
					this.Visibility = this.owner.WindowState != WindowState.Minimized ? Visibility.Visible : Visibility.Collapsed;
					break;
				case WindowAction.Normalize:
					this.Visibility = this.owner.WindowState != WindowState.Normal ? Visibility.Visible : Visibility.Collapsed;
					break;
			}
		}
	}
}

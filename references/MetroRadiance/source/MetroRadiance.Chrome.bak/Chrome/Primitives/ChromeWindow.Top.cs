using System;
using System.Linq;
using System.Windows;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Chrome.Primitives
{
	internal class TopChromeWindow : ChromeWindow
	{
		static TopChromeWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TopChromeWindow),
				new FrameworkPropertyMetadata(typeof(TopChromeWindow)));
			TitleProperty.OverrideMetadata(
				typeof(TopChromeWindow),
				new FrameworkPropertyMetadata(nameof(TopChromeWindow)));
		}

		private int _leftScaledOffset = 0;
		private int _rightScaledOffset = 0;

		public TopChromeWindow()
		{
			this.SizeToContent = SizeToContent.Height;
		}

		protected override void UpdateDpiResources()
		{
			this._leftScaledOffset = this.Offset.Left.DpiRoundX(this.CurrentDpi);
			this._rightScaledOffset = this.Offset.Right.DpiRoundX(this.CurrentDpi);
		}

		protected override int GetLeft(RECT owner)
		{
			return owner.Left - this._leftScaledOffset;
		}

		protected override int GetTop(RECT owner)
		{
			return owner.Top - this.GetHeight(owner);
		}

		protected override int GetWidth(RECT owner)
		{
			return owner.Width + this._leftScaledOffset + this._rightScaledOffset;
		}

		protected override int GetHeight(RECT owner)
		{
			return this.ActualHeight.DpiRoundY(this.SystemDpi);
		}

		protected override void OwnerSizeChangedCallback(object sender, EventArgs eventArgs)
		{
			this.UpdateSize();
		}
	}
}

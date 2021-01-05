using System;
using System.Linq;
using System.Windows;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Chrome.Primitives
{
	internal class BottomChromeWindow : ChromeWindow
	{
		static BottomChromeWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(BottomChromeWindow),
				new FrameworkPropertyMetadata(typeof(BottomChromeWindow)));
			TitleProperty.OverrideMetadata(
				typeof(BottomChromeWindow),
				new FrameworkPropertyMetadata(nameof(BottomChromeWindow)));
		}

		private int _leftScaledOffset = 0;
		private int _rightScaledOffset = 0;

		public BottomChromeWindow()
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
			return owner.Bottom;
		}

		protected override int GetWidth(RECT owner)
		{
			return owner.Width + this._leftScaledOffset + this._rightScaledOffset;
		}

		protected override int GetHeight(RECT owner)
		{
			return this.ActualHeight.DpiRoundY(this.SystemDpi);
		}
	}
}

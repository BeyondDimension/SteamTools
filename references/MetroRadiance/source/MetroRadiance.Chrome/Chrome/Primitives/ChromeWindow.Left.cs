using System;
using System.Linq;
using System.Windows;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Chrome.Primitives
{
	internal class LeftChromeWindow : ChromeWindow
	{
		static LeftChromeWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(LeftChromeWindow),
				new FrameworkPropertyMetadata(typeof(LeftChromeWindow)));
			TitleProperty.OverrideMetadata(
				typeof(LeftChromeWindow),
				new FrameworkPropertyMetadata(nameof(LeftChromeWindow)));
		}

		public LeftChromeWindow()
		{
			this.SizeToContent = SizeToContent.Width;
		}

		protected override void UpdateDpiResources() { }

		protected override int GetLeft(RECT owner)
		{
			return owner.Left - this.GetWidth(owner);
		}

		protected override int GetTop(RECT owner)
		{
			return owner.Top;
		}

		protected override int GetWidth(RECT owner)
		{
			return this.ActualWidth.DpiRoundX(this.SystemDpi);
		}

		protected override int GetHeight(RECT owner)
		{
			return owner.Height;
		}

		protected override void OwnerSizeChangedCallback(object sender, EventArgs eventArgs)
		{
			this.UpdateSize();
		}
	}
}

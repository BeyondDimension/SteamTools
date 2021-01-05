using System;
using System.Linq;
using System.Windows;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Chrome.Primitives
{
	internal class RightChromeWindow : ChromeWindow
	{
		static RightChromeWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(RightChromeWindow),
				new FrameworkPropertyMetadata(typeof(RightChromeWindow)));
			TitleProperty.OverrideMetadata(
				typeof(RightChromeWindow),
				new FrameworkPropertyMetadata(nameof(RightChromeWindow)));
		}

		public RightChromeWindow()
		{
			this.SizeToContent = SizeToContent.Width;
		}

		protected override void UpdateDpiResources() { }
		
		protected override int GetLeft(RECT owner)
		{
			return owner.Right;
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
	}
}

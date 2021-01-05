using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using MetroRadiance.Interop;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.UI.Controls
{
	public class CaptionIcon : Button
	{
		static CaptionIcon()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionIcon), new FrameworkPropertyMetadata(typeof(CaptionIcon)));
		}

		private bool isSystemMenuOpened;


		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			var window = Window.GetWindow(this);
			if (window == null) return;

			window.SourceInitialized += this.Initialize;
		}

		private void Initialize(object sender, EventArgs e)
		{
			var window = (Window)sender;
			window.SourceInitialized -= this.Initialize;

			var source = PresentationSource.FromVisual(window) as HwndSource;
			if (source != null)
			{
				source.AddHook(this.WndProc);
				window.Closed += (o, args) => source.RemoveHook(this.WndProc);
			}
		}


		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_NCLBUTTONDOWN)
			{
				this.isSystemMenuOpened = false;
			}

			return IntPtr.Zero;
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				var window = Window.GetWindow(this);
				if (e.ClickCount == 1)
				{
					if (!this.isSystemMenuOpened)
					{
						this.isSystemMenuOpened = true;

						var point = this.PointToScreen(new Point(0, this.ActualHeight));
						var dpi = Dpi.FromVisual(window);
						SystemCommands.ShowSystemMenu(window, dpi.PhysicalToLogical(point));
					}
					else
					{
						this.isSystemMenuOpened = false;
					}
				}
				else if (e.ClickCount == 2)
				{
					window.Close();
				}
			}
			else
			{
				base.OnMouseDown(e);
			}
		}

		protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
		{
			var window = Window.GetWindow(this);
			var point = this.PointToScreen(e.GetPosition(this));
			var dpi = Dpi.FromVisual(window);
			SystemCommands.ShowSystemMenu(window, dpi.PhysicalToLogical(point));
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			this.isSystemMenuOpened = false;
			base.OnMouseLeave(e);
		}
	}
}

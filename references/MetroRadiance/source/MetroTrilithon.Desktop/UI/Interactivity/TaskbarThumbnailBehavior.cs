using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using System.Windows.Shell;

namespace MetroTrilithon.UI.Interactivity
{
	public class TaskbarThumbnailBehavior : Behavior<FrameworkElement>
	{
		private Window _owner;

		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.Loaded += this.AssociatedObjectOnLoaded;
			this.AssociatedObject.Unloaded += this.AssociatedObjectOnUnloaded;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			this.AssociatedObject.Loaded -= this.AssociatedObjectOnLoaded;
			this.AssociatedObject.Unloaded -= this.AssociatedObjectOnUnloaded;
		}

		public void UpdateClipMargin()
		{
			var element = this.AssociatedObject;
			var window = this.GetWindow();
			if (window == null) return;

			var screenPoint = element.PointToScreen(new Point(.0, .0));
			var clientPoint = window.PointFromScreen(screenPoint);
			var clipMargin = new Thickness(
				clientPoint.X,
				clientPoint.Y,
				window.ActualWidth - (clientPoint.X + element.ActualWidth),
				window.ActualHeight - (clientPoint.Y + element.ActualHeight));

			(window.TaskbarItemInfo ?? (window.TaskbarItemInfo = new TaskbarItemInfo())).ThumbnailClipMargin = clipMargin;
		}

		public void ResetClipMargin()
		{
			var window = this.GetWindow();
			if (window?.TaskbarItemInfo == null) return;

			window.TaskbarItemInfo.ThumbnailClipMargin = new Thickness(.0);
		}

		private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			this.UpdateClipMargin();

			var window = this.GetWindow();
			if (window != null) window.LayoutUpdated += this.OwnerOnLayoutUpdated;
		}

		private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
		{
			this.ResetClipMargin();

			var window = this.GetWindow();
			if (window != null) window.LayoutUpdated -= this.OwnerOnLayoutUpdated;
		}

		private void OwnerOnLayoutUpdated(object sender, EventArgs e)
		{
			this.UpdateClipMargin();
		}

		private Window GetWindow()
		{
			if (this._owner == null)
			{
				var window = Window.GetWindow(this.AssociatedObject);
				if (window != null)
				{
					this._owner = window;
				}
				else
				{
					// is 何
					System.Diagnostics.Debug.WriteLine("[TaskbarThumbnailBehavior] Window.GetWindow failed.");
				}
			}

			return this._owner;
		}
	}
}

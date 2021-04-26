using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CefNet.Avalonia;
using System;
using System.Threading.Tasks;

namespace CefNet.Internal
{
	internal sealed class AvaloniaContextMenuRunner
	{
		private readonly MenuModel Model;
		private CefRunContextMenuCallback Callback;
		private ContextMenu Menu;
		private TaskCompletionSource<bool> _completionSource;

		public AvaloniaContextMenuRunner(CefMenuModel model, CefRunContextMenuCallback callback)
		{
			Model = MenuModel.FromCefMenu(model);
			Callback = callback;
		}

		public void Build()
		{
			if (Menu != null)
				throw new InvalidOperationException();

			Menu = new ContextMenu();
			Menu.MenuClosed += Menu_Closed;
			Build(Model, (AvaloniaList<object>)Menu.Items);
		}

		private void Menu_Closed(object sender, RoutedEventArgs e)
		{
			Cancel();
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			var clickedItem = sender as MenuItem;
			object cid = clickedItem?.Tag;
			if (cid != null)
			{
				Callback.Continue((int)cid, CefEventFlags.LeftMouseButton);
				Callback = null;
			}
		}

		public Task CompletionTask
		{
			get { return _completionSource?.Task; }
		}

		private void Build(MenuModel model, AvaloniaList<object> menu)
		{
			int count = model.Count;
			for (int i = 0; i < count; i++)
			{
				bool isSubmenu = false;
				MenuItem menuItem;
				switch (model.GetTypeAt(i))
				{
					case CefMenuItemType.Separator:
						menu.Add(new Separator());
						continue;
					case CefMenuItemType.Check:
						menuItem = new MenuItem();
						menuItem.Icon = new CheckBox() { IsChecked = model.IsCheckedAt(i), Margin = new Thickness(-2, 0, 0, 0), BorderThickness = new Thickness() };
						break;
					case CefMenuItemType.Radio:
						menuItem = new MenuItem();
						menuItem.Icon = new RadioButton() { IsChecked = model.IsCheckedAt(i), Margin = new Thickness(-2, 0, 0, 0), BorderThickness = new Thickness() };
						break;
					case CefMenuItemType.Command:
						menuItem = new MenuItem();
						break;
					case CefMenuItemType.Submenu:
						isSubmenu = true;
						menuItem = new MenuItem();
						if (model.IsEnabledAt(i))
						{
							Build(model.GetSubMenuAt(i), (AvaloniaList<object>)menuItem.Items);
						}
						break;
					default:
						continue;
				}
				if (!isSubmenu)
				{
					menuItem.Click += MenuItem_Click;
					menuItem.Tag = model.GetCommandIdAt(i);
				}
				menuItem.Header = model.GetLabelAt(i).Replace('&', '_');
				menuItem.IsEnabled = model.IsEnabledAt(i);
				//menuItem.Foreground = model.GetColorAt(i, CefMenuColorType.Text, out CefColor color) ? new SolidColorBrush(color.ToColor()) : SystemColors.MenuTextBrush;
				menu.Add(menuItem);
			}
		}

		public void RunMenuAt(Control control, Point point)
		{
			_completionSource = new TaskCompletionSource<bool>();
			Dispatcher.UIThread.Post(() =>
			{
				Menu.Open(control);
				_completionSource.TrySetResult(true);
			});
		}

		public void Cancel()
		{
			_completionSource?.TrySetResult(false);
			Callback?.Cancel();
			Callback = null;
		}

	}
}

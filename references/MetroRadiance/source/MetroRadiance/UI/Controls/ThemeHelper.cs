using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MetroRadiance.UI.Controls
{
	public static class ThemeHelper
	{
		private static readonly Dictionary<FrameworkElement, IDisposable> registeredElements = new Dictionary<FrameworkElement, IDisposable>();
		
		private static void RemoveResources(FrameworkElement element)
		{
			IDisposable disposable;
			if (registeredElements.TryGetValue(element, out disposable))
			{
				registeredElements.Remove(element);
				disposable.Dispose();
			}
		}

		#region HasThemeResources 添付プロパティ

		public static readonly DependencyProperty HasThemeResourcesProperty = DependencyProperty.RegisterAttached(
			"HasThemeResources",
			typeof(bool), 
			typeof(ThemeService),
			new PropertyMetadata(false, HasThemeResourcesChangedCallback));

		public static void SetHasThemeResources(FrameworkElement element, bool value)
		{
			element.SetValue(HasThemeResourcesProperty, value);
		}

		public static bool GetHasThemeResources(FrameworkElement element)
		{
			return (bool)element.GetValue(HasThemeResourcesProperty);
		}

		private static void HasThemeResourcesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			if (DesignerProperties.GetIsInDesignMode(d)) return;

			var element = (FrameworkElement)d;
			var oldValue = (bool)args.OldValue;
			var newValue = (bool)args.NewValue;

			Action logic = () =>
			{
				if (oldValue && !newValue)
				{
					// true -> false
					element.Unloaded -= ElementUnloadedCallback;
					RemoveResources(element);
				}
				else if (!oldValue && newValue)
				{
					// false -> true
					registeredElements[element] = ThemeService.Current.Register(element.Resources);
					element.Unloaded += ElementUnloadedCallback;
				}
			};

			if (element.IsInitialized)
			{
				logic();
			}
			else
			{
				EventHandler handler = null;
				handler = (sender, e) =>
				{
					element.Initialized -= handler;
					logic();
				};
				element.Initialized += handler;
			}
		}

		private static void ElementUnloadedCallback(object sender, RoutedEventArgs e)
		{
			var element = (FrameworkElement)sender;
			element.Unloaded -= ElementUnloadedCallback;
			RemoveResources(element);
		}

		#endregion
	}
}

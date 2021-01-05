using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MetroTrilithon.UI.Controls
{
	public class WebBrowserHelper
	{
		#region ScriptErrorsSuppressed 添付プロパティ

		public static readonly DependencyProperty ScriptErrorsSuppressedProperty =
			DependencyProperty.RegisterAttached(nameof(ScriptErrorsSuppressedProperty).Replace("Property", ""), typeof(bool), typeof(WebBrowserHelper), new PropertyMetadata(default(bool), ScriptErrorsSuppressedChangedCallback));

		public static void SetScriptErrorsSuppressed(WebBrowser browser, bool value)
		{
			browser.SetValue(ScriptErrorsSuppressedProperty, value);
		}

		public static bool GetScriptErrorsSuppressed(WebBrowser browser)
		{
			return (bool)browser.GetValue(ScriptErrorsSuppressedProperty);
		}

		private static void ScriptErrorsSuppressedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var browser = d as WebBrowser;
			if (browser == null) return;
			if (!(e.NewValue is bool)) return;

			try
			{
				var axIWebBrowser2 = GetAxWebbrowser2(browser);
				axIWebBrowser2.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, axIWebBrowser2, new[] { e.NewValue, });
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		#endregion

		#region AllowWebBrowserDrop

		public static readonly DependencyProperty AllowWebBrowserDropProperty =
			DependencyProperty.RegisterAttached(nameof(AllowWebBrowserDropProperty).Replace("Property", ""), typeof(bool), typeof(WebBrowserHelper), new PropertyMetadata(true, AllowWebBrowserDropChangedCallback));

		public static void SetAllowWebBrowserDrop(DependencyObject element, bool value)
		{
			element.SetValue(AllowWebBrowserDropProperty, value);
		}

		public static bool GetAllowWebBrowserDrop(DependencyObject element)
		{
			return (bool)element.GetValue(AllowWebBrowserDropProperty);
		}


		private static void AllowWebBrowserDropChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var browser = d as WebBrowser;
			if (browser == null) return;
			if (!(e.NewValue is bool)) return;

			try
			{
				var axIWebBrowser2 = GetAxWebbrowser2(browser);
				axIWebBrowser2.GetType().InvokeMember("RegisterAsDropTarget", BindingFlags.SetProperty, null, axIWebBrowser2, new[] { e.NewValue, });

			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		#endregion


		public static object GetAxWebbrowser2(WebBrowser browser)
		{
			var axIWebBrowser2Prop = typeof(WebBrowser).GetProperty("AxIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
			return axIWebBrowser2Prop?.GetValue(browser, null);
		}
	}
}

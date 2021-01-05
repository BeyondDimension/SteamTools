using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MetroRadiance.Chrome.Primitives;
using MetroRadiance.Platform;

namespace MetroRadiance.Chrome
{
	/// <summary>
	/// ウィンドウにアタッチされ、四辺にカスタム UI を表示する機能を提供します。
	/// </summary>
	public class WindowChrome : DependencyObject
	{
		private readonly TopChromeWindow _top = new TopChromeWindow();
		private readonly LeftChromeWindow _left = new LeftChromeWindow();
		private readonly RightChromeWindow _right = new RightChromeWindow();
		private readonly BottomChromeWindow _bottom = new BottomChromeWindow();

		#region Content wrappers

		public object Left
		{
			get { return this._left.Content; }
			set { this._left.Content = value; }
		}

		public object Right
		{
			get { return this._right.Content; }
			set { this._right.Content = value; }
		}

		public object Top
		{
			get { return this._top.Content; }
			set { this._top.Content = value; }
		}

		public object Bottom
		{
			get { return this._bottom.Content; }
			set { this._bottom.Content = value; }
		}

		#endregion

		#region BorderThickness dependency property

		public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
			nameof(BorderThickness), typeof(Thickness), typeof(WindowChrome), new PropertyMetadata(new Thickness(.99), BorderThicknessPropertyCallback));

		public Thickness BorderThickness
		{
			get { return (Thickness)this.GetValue(BorderThicknessProperty); }
			set { this.SetValue(BorderThicknessProperty, value); }
		}

		private static void BorderThicknessPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (WindowChrome)d;
			var newValue = (Thickness)e.NewValue;

			instance.UpdateThickness(newValue);
		}

		#endregion

		#region CanResize dependency property

		public static readonly DependencyProperty CanResizeProperty = DependencyProperty.Register(
			nameof(CanResize), typeof(bool), typeof(WindowChrome), new PropertyMetadata(GlowingEdge.CanResizeProperty.DefaultMetadata.DefaultValue, CanResizePropertyCallback));

		public bool CanResize
		{
			get { return (bool)this.GetValue(CanResizeProperty); }
			set { this.SetValue(CanResizeProperty, value); }
		}

		private static void CanResizePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (WindowChrome)d;
			var newValue = (bool)e.NewValue;

			instance._top.Edge.CanResize = newValue;
			instance._left.Edge.CanResize = newValue;
			instance._right.Edge.CanResize = newValue;
			instance._bottom.Edge.CanResize = newValue;
		}

		#endregion

		#region OverrideDefaultEdge dependency property

		public static readonly DependencyProperty OverrideDefaultEdgeProperty = DependencyProperty.Register(
			nameof(OverrideDefaultEdge), typeof(bool), typeof(WindowChrome), new PropertyMetadata(false, OverrideDefaultEdgePropertyCallback));

		public bool OverrideDefaultEdge
		{
			get { return (bool)this.GetValue(OverrideDefaultEdgeProperty); }
			set { this.SetValue(OverrideDefaultEdgeProperty, value); }
		}

		private static void OverrideDefaultEdgePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (WindowChrome)d;
			var oldValue = (bool)e.OldValue;
			var newValue = (bool)e.NewValue;

			if (!oldValue && newValue)
			{
				// false -> true
				instance._top.Edge.Visibility = Visibility.Collapsed;
				instance._left.Edge.Visibility = Visibility.Collapsed;
				instance._right.Edge.Visibility = Visibility.Collapsed;
				instance._bottom.Edge.Visibility = Visibility.Collapsed;
			}
			if (oldValue && !newValue)
			{
				// true -> false
				instance._top.Edge.Visibility = Visibility.Visible;
				instance._left.Edge.Visibility = Visibility.Visible;
				instance._right.Edge.Visibility = Visibility.Visible;
				instance._bottom.Edge.Visibility = Visibility.Visible;
			}
		}

		#endregion

		#region Instance attached property

		public static readonly DependencyProperty InstanceProperty = DependencyProperty.RegisterAttached(
			"Instance", typeof(WindowChrome), typeof(WindowChrome), new PropertyMetadata(default(WindowChrome), InstanceChangedCallback));

		public static void SetInstance(Window window, WindowChrome value)
		{
			window.SetValue(InstanceProperty, value);
		}

		public static WindowChrome GetInstance(Window window)
		{
			return (WindowChrome)window.GetValue(InstanceProperty);
		}

		private static void InstanceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var window = d as Window;
			if (window == null) return;

			var oldValue = (WindowChrome)e.OldValue;
			var newValue = (WindowChrome)e.NewValue;

			oldValue?.Detach();
			newValue?.Attach(window);
		}

		#endregion


		public WindowChrome()
		{
			this.UpdateThickness(this.BorderThickness);
		}

		/// <summary>
		/// 指定した WPF <see cref="Window"/> に、このクローム UI をアタッチします。
		/// </summary>
		public void Attach(Window window)
		{
			this.Detach();

			this._top.Attach(window);
			this._left.Attach(window);
			this._right.Attach(window);
			this._bottom.Attach(window);

			this.CanResize = true;
		}

		/// <summary>
		/// 指定したウィンドウに、このクローム UI をアタッチします。
		/// </summary>
		public void Attach(IChromeOwner window)
		{
			this.Detach();

			this._top.Attach(window);
			this._left.Attach(window);
			this._right.Attach(window);
			this._bottom.Attach(window);

			this.CanResize = false;
		}

		public void Detach()
		{
			this._top.Detach();
			this._left.Detach();
			this._right.Detach();
			this._bottom.Detach();
		}

		public void Close()
		{
			this.Detach();

			this._top.Close();
			this._left.Close();
			this._right.Close();
			this._bottom.Close();
		}

		private void UpdateThickness(Thickness thickness)
		{
			this._top.BorderThickness = thickness;
			this._left.BorderThickness = thickness;
			this._right.BorderThickness = thickness;
			this._bottom.BorderThickness = thickness;

			var offset = new Thickness(
				ChromeWindow.Thickness + thickness.Left,
				ChromeWindow.Thickness + thickness.Top,
				ChromeWindow.Thickness + thickness.Right,
				ChromeWindow.Thickness + thickness.Bottom);

			this._top.Offset = offset;
			this._left.Offset = offset;
			this._right.Offset = offset;
			this._bottom.Offset = offset;
		}
	}
}

using MetroRadiance.Chrome.Primitives;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MetroRadiance.Chrome
{
	[TemplatePart(Name = PART_BottomThumb, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_BottomLeftThumb, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_BottomRightThumb, Type = typeof(FrameworkElement))]
	public sealed class BottomGlowingEdge : GlowingEdge
	{
		private const string PART_BottomThumb = nameof(PART_BottomThumb);
		private const string PART_BottomLeftThumb = nameof(PART_BottomLeftThumb);
		private const string PART_BottomRightThumb = nameof(PART_BottomRightThumb);

		static BottomGlowingEdge()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(BottomGlowingEdge),
				new FrameworkPropertyMetadata(typeof(BottomGlowingEdge)));
		}

		private FrameworkElement _bottomThumb;
		private FrameworkElement _bottomLeftThumb;
		private FrameworkElement _bottomRightThumb;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._bottomThumb = (FrameworkElement)this.GetTemplateChild(PART_BottomThumb);
			this._bottomLeftThumb = (FrameworkElement)this.GetTemplateChild(PART_BottomLeftThumb);
			this._bottomRightThumb = (FrameworkElement)this.GetTemplateChild(PART_BottomRightThumb);
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			var point = e.GetPosition(this);
			var result = VisualTreeHelper.HitTest(this, point);
			if (result == null)
			{
				base.OnMouseLeftButtonDown(e);
			}

			var window = (BottomChromeWindow)Window.GetWindow(this);
			if (result.VisualHit == this._bottomThumb)
			{
				window.Resize(SizingMode.Bottom);
			}
			else if (result.VisualHit == this._bottomLeftThumb)
			{
				window.Resize(SizingMode.BottomLeft);
			}
			else if (result.VisualHit == this._bottomRightThumb)
			{
				window.Resize(SizingMode.BottomRight);
			}
		}

		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			var window = (BottomChromeWindow)Window.GetWindow(this);
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				window.DoubleClick(SizingMode.Bottom);
			}
			else
			{
				base.OnMouseDoubleClick(e);
			}
		}
	}
}

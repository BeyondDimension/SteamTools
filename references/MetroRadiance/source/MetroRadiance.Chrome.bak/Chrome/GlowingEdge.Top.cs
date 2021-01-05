using MetroRadiance.Chrome.Primitives;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MetroRadiance.Chrome
{
	[TemplatePart(Name = PART_TopThumb, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_TopLeftThumb, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_TopRightThumb, Type = typeof(FrameworkElement))]
	public sealed class TopGlowingEdge : GlowingEdge
	{
		private const string PART_TopThumb = nameof(PART_TopThumb);
		private const string PART_TopLeftThumb = nameof(PART_TopLeftThumb);
		private const string PART_TopRightThumb = nameof(PART_TopRightThumb);

		static TopGlowingEdge()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TopGlowingEdge),
				new FrameworkPropertyMetadata(typeof(TopGlowingEdge)));
		}

		private FrameworkElement _topThumb;
		private FrameworkElement _topLeftThumb;
		private FrameworkElement _topRightThumb;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._topThumb = (FrameworkElement)this.GetTemplateChild(PART_TopThumb);
			this._topLeftThumb = (FrameworkElement)this.GetTemplateChild(PART_TopLeftThumb);
			this._topRightThumb = (FrameworkElement)this.GetTemplateChild(PART_TopRightThumb);
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			var point = e.GetPosition(this);
			var result = VisualTreeHelper.HitTest(this, point);
			if (result == null)
			{
				base.OnMouseLeftButtonDown(e);
			}

			var window = (TopChromeWindow)Window.GetWindow(this);
			if (result.VisualHit == this._topThumb)
			{
				window.Resize(SizingMode.Top);
			}
			else if (result.VisualHit == this._topLeftThumb)
			{
				window.Resize(SizingMode.TopLeft);
			}
			else if (result.VisualHit == this._topRightThumb)
			{
				window.Resize(SizingMode.TopRight);
			}
		}

		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			var window = (TopChromeWindow)Window.GetWindow(this);
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				window.DoubleClick(SizingMode.Top);
			}
			else
			{
				base.OnMouseDoubleClick(e);
			}
		}
	}
}

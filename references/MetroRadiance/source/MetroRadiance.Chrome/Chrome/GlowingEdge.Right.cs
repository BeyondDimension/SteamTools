using MetroRadiance.Chrome.Primitives;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MetroRadiance.Chrome
{
	[TemplatePart(Name = PART_RightThumb, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_TopRightThumb, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_BottomRightThumb, Type = typeof(FrameworkElement))]
	public sealed class RightGlowingEdge : GlowingEdge
	{
		private const string PART_RightThumb = nameof(PART_RightThumb);
		private const string PART_TopRightThumb = nameof(PART_TopRightThumb);
		private const string PART_BottomRightThumb = nameof(PART_BottomRightThumb);

		static RightGlowingEdge()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(RightGlowingEdge),
				new FrameworkPropertyMetadata(typeof(RightGlowingEdge)));
		}

		private FrameworkElement _rightThumb;
		private FrameworkElement _topRightThumb;
		private FrameworkElement _bottomRightThumb;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._rightThumb = (FrameworkElement)this.GetTemplateChild(PART_RightThumb);
			this._topRightThumb = (FrameworkElement)this.GetTemplateChild(PART_TopRightThumb);
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

			var window = (RightChromeWindow)Window.GetWindow(this);
			if (result.VisualHit == this._rightThumb)
			{
				window.Resize(SizingMode.Right);
			}
			else if (result.VisualHit == this._topRightThumb)
			{
				window.Resize(SizingMode.TopRight);
			}
			else if (result.VisualHit == this._bottomRightThumb)
			{
				window.Resize(SizingMode.BottomRight);
			}
		}
	}
}

using MetroRadiance.Chrome.Primitives;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MetroRadiance.Chrome
{
	[TemplatePart(Name = PART_LeftThumb, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_TopLeftThumb, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PART_BottomLeftThumb, Type = typeof(FrameworkElement))]
	public sealed class LeftGlowingEdge : GlowingEdge
	{
		private const string PART_LeftThumb = nameof(PART_LeftThumb);
		private const string PART_TopLeftThumb = nameof(PART_TopLeftThumb);
		private const string PART_BottomLeftThumb = nameof(PART_BottomLeftThumb);

		static LeftGlowingEdge()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(LeftGlowingEdge),
				new FrameworkPropertyMetadata(typeof(LeftGlowingEdge)));
		}

		private FrameworkElement _leftThumb;
		private FrameworkElement _topLeftThumb;
		private FrameworkElement _bottomLeftThumb;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._leftThumb = (FrameworkElement)this.GetTemplateChild(PART_LeftThumb);
			this._topLeftThumb = (FrameworkElement)this.GetTemplateChild(PART_TopLeftThumb);
			this._bottomLeftThumb = (FrameworkElement)this.GetTemplateChild(PART_BottomLeftThumb);
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			var point = e.GetPosition(this);
			var result = VisualTreeHelper.HitTest(this, point);
			if (result == null)
			{
				base.OnMouseLeftButtonDown(e);
			}

			var window = (LeftChromeWindow)Window.GetWindow(this);
			if (result.VisualHit == this._leftThumb)
			{
				window.Resize(SizingMode.Left);
			}
			else if (result.VisualHit == this._topLeftThumb)
			{
				window.Resize(SizingMode.TopLeft);
			}
			else if (result.VisualHit == this._bottomLeftThumb)
			{
				window.Resize(SizingMode.BottomLeft);
			}
		}
	}
}

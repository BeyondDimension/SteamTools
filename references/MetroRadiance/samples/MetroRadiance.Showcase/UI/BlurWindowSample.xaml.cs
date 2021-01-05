using MetroRadiance.Interop.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MetroRadiance.Showcase.UI
{
	public partial class BlurWindowSample
	{
		public BlurWindowSample()
		{
			this.InitializeComponent();

			this.blurOpacitySlider.ValueChanged += (sender, e) => this.BlurOpacity = e.NewValue;
			this.leftBorderCheckBox.Checked += (sender, e) => this.BordersFlag |= AccentFlags.DrawLeftBorder;
			this.leftBorderCheckBox.Unchecked += (sender, e) => this.BordersFlag ^= AccentFlags.DrawLeftBorder;
			this.topBorderCheckBox.Checked += (sender, e) => this.BordersFlag |= AccentFlags.DrawTopBorder;
			this.topBorderCheckBox.Unchecked += (sender, e) => this.BordersFlag ^= AccentFlags.DrawTopBorder;
			this.rightBorderCheckBox.Checked += (sender, e) => this.BordersFlag |= AccentFlags.DrawRightBorder;
			this.rightBorderCheckBox.Unchecked += (sender, e) => this.BordersFlag ^= AccentFlags.DrawRightBorder;
			this.bottomBorderCheckBox.Checked += (sender, e) => this.BordersFlag |= AccentFlags.DrawBottomBorder;
			this.bottomBorderCheckBox.Unchecked += (sender, e) => this.BordersFlag ^= AccentFlags.DrawBottomBorder;
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			this.DragMove();
		}
	}
}

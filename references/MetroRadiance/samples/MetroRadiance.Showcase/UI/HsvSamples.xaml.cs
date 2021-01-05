using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using MetroRadiance.Media;

namespace MetroRadiance.Showcase.UI
{
	partial class HsvSamples
	{
		public HsvSamples()
		{
			this.InitializeComponent();

			this.hSlider.ValueChanged += (sender, e) => this.Update();
			this.sSlider.ValueChanged += (sender, e) => this.Update();
			this.vSlider.ValueChanged += (sender, e) => this.Update();

			this.Update();
		}


		private void Update()
		{
			var h = this.hSlider.Value;
			var s = this.sSlider.Value / 100.0;
			var v = this.vSlider.Value / 100.0;

			var hsv = HsvColor.FromHsv(h, s, v);
			var c = hsv.ToRgb();

			var l = Luminosity.FromRgb(c);
			var w = l <= 128;

			this.colorbox.Background = new SolidColorBrush(c);
			this.colorbox.Foreground = w ? Brushes.White : Brushes.Black;
			this.colorbox.Text = $"Color: {c}, Luminosity: {l}";
		}
	}
}

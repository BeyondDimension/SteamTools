using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using MetroRadiance.Media;
using MetroRadiance.Platform;

namespace MetroRadiance.Showcase.UI
{
	public partial class ImmersiveColorSamples
	{
		public ImmersiveColorSamples()
		{
			this.InitializeComponent();

			this.DataContext = typeof(ImmersiveColorNames).GetFields(BindingFlags.Static | BindingFlags.Public)
				.Select(x => (string)x.GetValue(null))
				.Select(name =>
				{
					var background = new SolidColorBrush(ImmersiveColor.GetColorByTypeName(name));
					var luminocity = Luminosity.FromRgb(background.Color);
					var foreground = new SolidColorBrush(luminocity < 128 ? Colors.White : Colors.Black);

					return new { name, background, foreground, };
				})
				.ToArray();
		}
	}
}

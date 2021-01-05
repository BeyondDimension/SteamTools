using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MetroRadiance.UI.Controls
{
	[TemplatePart(Name = PART_CountHost, Type = typeof(TextBlock))]
	public class Badge : Control
	{
		private const string PART_CountHost = "PART_CountHost";

		static Badge()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Badge), new FrameworkPropertyMetadata(typeof(Badge)));
		}

		private TextBlock block;
		private double initialSize;

		#region Count 依存関係プロパティ

		public int? Count
		{
			get { return (int?)this.GetValue(CountProperty); }
			set { this.SetValue(CountProperty, value); }
		}
		public static readonly DependencyProperty CountProperty =
			DependencyProperty.Register(nameof(Count), typeof(int?), typeof(Badge), new UIPropertyMetadata(null, CountPropertyChangedCallback));

		private static void CountPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var source = (Badge)d;
			source.SetCount((int?)e.NewValue);
		}

		#endregion

		public Badge()
		{
			this.SetCount(null);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.block = this.GetTemplateChild(PART_CountHost) as TextBlock;
			if (this.block != null)
			{
				this.initialSize = this.block.FontSize;
				this.SetCount(this.Count);
			}
		}

		private void SetCount(int? count)
		{
			if (count.HasValue)
			{
				if (this.block != null)
				{
					this.block.Text = count.Value.ToString(CultureInfo.InvariantCulture);
					this.block.FontSize = count.Value >= 10 ? this.initialSize - 1 : this.initialSize;
				}
				this.Visibility = Visibility.Visible;
			}
			else
			{
				this.Visibility = Visibility.Hidden;
			}
		}
	}
}

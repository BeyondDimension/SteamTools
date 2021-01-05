using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MetroRadiance.UI.Controls
{
	public class LinkButton : Button
	{
		static LinkButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkButton), new FrameworkPropertyMetadata(typeof(LinkButton)));
		}

		#region Text 依存関係プロパティ

		public string Text
		{
			get { return (string)this.GetValue(TextProperty); }
			set { this.SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(LinkButton), new UIPropertyMetadata(""));

		#endregion

		#region TextTrimming 依存関係プロパティ

		public TextTrimming TextTrimming
		{
			get { return (TextTrimming)this.GetValue(TextTrimmingProperty); }
			set { this.SetValue(TextTrimmingProperty, value); }
		}
		public static readonly DependencyProperty TextTrimmingProperty =
			DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(LinkButton), new UIPropertyMetadata(TextTrimming.CharacterEllipsis));

		#endregion

		#region TextWrapping 依存関係プロパティ

		public TextWrapping TextWrapping
		{
			get { return (TextWrapping)this.GetValue(TextWrappingProperty); }
			set { this.SetValue(TextWrappingProperty, value); }
		}
		public static readonly DependencyProperty TextWrappingProperty =
			DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(LinkButton), new UIPropertyMetadata(TextWrapping.NoWrap));

		#endregion

	}
}

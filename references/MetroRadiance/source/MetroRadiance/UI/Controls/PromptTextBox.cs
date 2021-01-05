using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MetroRadiance.UI.Controls
{
	/// <summary>
	/// 未入力時にプロンプトを表示できる <see cref="TextBox"/> を表します。
	/// </summary>
	[TemplateVisualState(Name = "Empty", GroupName = "TextStates")]
	[TemplateVisualState(Name = "NotEmpty", GroupName = "TextStates")]
	public class PromptTextBox : TextBox
	{
		static PromptTextBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PromptTextBox), new FrameworkPropertyMetadata(typeof(PromptTextBox)));
		}

		public PromptTextBox()
		{
			this.UpdateTextStates(true);
			this.TextChanged += (sender, e) => this.UpdateTextStates(true);
			this.GotKeyboardFocus += (sender, e) => this.UpdateTextStates(true);
		}

		#region Prompt 依存関係プロパティ

		public string Prompt
		{
			get { return (string)this.GetValue(PromptProperty); }
			set { this.SetValue(PromptProperty, value); }
		}
		public static readonly DependencyProperty PromptProperty =
			DependencyProperty.Register("Prompt", typeof(string), typeof(PromptTextBox), new UIPropertyMetadata("", PromptChangedCallback));

		private static void PromptChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion

		#region PromptBrush 依存関係プロパティ

		public Brush PromptBrush
		{
			get { return (Brush)this.GetValue(PromptBrushProperty); }
			set { this.SetValue(PromptBrushProperty, value); }
		}
		public static readonly DependencyProperty PromptBrushProperty =
			DependencyProperty.Register("PromptBrush", typeof(Brush), typeof(PromptTextBox), new UIPropertyMetadata(Brushes.Gray));

		#endregion


		private void UpdateTextStates(bool useTransitions)
		{
			VisualStateManager.GoToState(this, string.IsNullOrEmpty(this.Text) ? "Empty" : "NotEmpty", useTransitions);
		}
	}
}

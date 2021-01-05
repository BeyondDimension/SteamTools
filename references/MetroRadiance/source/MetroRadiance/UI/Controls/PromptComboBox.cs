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
	/// 未入力時にプロンプトを表示できる <see cref="ComboBox"/> を表します。
	/// </summary>
	[TemplateVisualState(Name = "Empty", GroupName = "TextStates")]
	[TemplateVisualState(Name = "NotEmpty", GroupName = "TextStates")]
	public class PromptComboBox : ComboBox
	{
		static PromptComboBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PromptComboBox), new FrameworkPropertyMetadata(typeof(PromptComboBox)));
		}

		public PromptComboBox()
		{
			this.UpdateTextStates(true);
			this.SelectionChanged += (sender, e) => this.UpdateTextStates(true);
			this.GotKeyboardFocus += (sender, e) => this.UpdateTextStates(true);
			//this.KeyDown += (sender, e) => this.UpdateTextStates(true);
			//this.KeyUp += (sender, e) => this.UpdateTextStates(true);
		}


		#region Prompt 依存関係プロパティ

		public string Prompt
		{
			get { return (string)this.GetValue(PromptProperty); }
			set { this.SetValue(PromptProperty, value); }
		}
		public static readonly DependencyProperty PromptProperty =
			DependencyProperty.Register("Prompt", typeof(string), typeof(PromptComboBox), new UIPropertyMetadata(""));

		#endregion

		#region PromptBrush 依存関係プロパティ

		public Brush PromptBrush
		{
			get { return (Brush)this.GetValue(PromptBrushProperty); }
			set { this.SetValue(PromptBrushProperty, value); }
		}
		public static readonly DependencyProperty PromptBrushProperty =
			DependencyProperty.Register("PromptBrush", typeof(Brush), typeof(PromptComboBox), new UIPropertyMetadata(Brushes.Gray));

		#endregion

		#region EditableText 依存関係プロパティ

		public string EditableText
		{
			get { return (string)this.GetValue(EditableTextProperty); }
			set { this.SetValue(EditableTextProperty, value); }
		}

		public static readonly DependencyProperty EditableTextProperty =
			DependencyProperty.Register("EditableText", typeof(string), typeof(PromptComboBox), new UIPropertyMetadata("", EditableTextChangedCallback));

		private static void EditableTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (PromptComboBox)d;
			instance.UpdateTextStates(true);
		}

		#endregion


		private void UpdateTextStates(bool useTransitions)
		{
			VisualStateManager.GoToState(this, string.IsNullOrEmpty(this.EditableText) ? "Empty" : "NotEmpty", useTransitions);
		}
	}
}

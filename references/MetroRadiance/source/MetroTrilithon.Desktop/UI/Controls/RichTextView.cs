using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace MetroTrilithon.UI.Controls
{
	[ContentProperty(nameof(RichTextTemplates))]
	public class RichTextView : RichTextBox
	{
		static RichTextView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(RichTextView),
				new FrameworkPropertyMetadata(typeof(RichTextView)));
		}


		#region Source 依存関係プロパティ

		public IEnumerable<RichText> Source
		{
			get { return (IEnumerable<RichText>)this.GetValue(SourceProperty); }
			set { this.SetValue(SourceProperty, value); }
		}
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register(nameof(Source), typeof(IEnumerable<RichText>), typeof(RichTextView), new UIPropertyMetadata(null, SourcePropertyChangedCallback));

		private static void SourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var source = (RichTextView)d;

			if (!DesignerProperties.GetIsInDesignMode(source))
			{
				source.UpdateDocument();
			}
		}

		#endregion

		#region DataTemplates 依存関係プロパティ

		public Collection<DataTemplate> RichTextTemplates
		{
			get { return (Collection<DataTemplate>)this.GetValue(RichTextTemplatesProperty); }
			set { this.SetValue(RichTextTemplatesProperty, value); }
		}
		public static readonly DependencyProperty RichTextTemplatesProperty =
			DependencyProperty.Register(nameof(RichTextTemplates), typeof(Collection<DataTemplate>), typeof(RichTextView), new UIPropertyMetadata(new Collection<DataTemplate>(), RichTextTemplatesPropertyChangedCallback));

		private static void RichTextTemplatesPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var source = (RichTextView)d;
			source.UpdateDocument();
		}

		#endregion

		public RichTextView()
		{
			this.Loaded += (sender, e) => this.UpdateDocument();
		}

		private void UpdateDocument()
		{
			if (this.Source != null && this.RichTextTemplates != null && this.RichTextTemplates.Any())
			{
				var paragraph = new Paragraph();

				foreach (var rt in this.Source)
				{
					var template = this.RichTextTemplates.FirstOrDefault(dt => (dt.DataType as Type) == rt.GetType());
					var presenter = template?.LoadContent() as RichTextInlinePresenter;
					var inline = presenter?.Content as Inline;
					if (inline != null)
					{
						inline.DataContext = rt;
						paragraph.Inlines.Add(inline);
					}
				}

				this.Document = new FlowDocument(paragraph)
				{
					TextAlignment = TextAlignment.Left,
				};
			}
		}
	}
}

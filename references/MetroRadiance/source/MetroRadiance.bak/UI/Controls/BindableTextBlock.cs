using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace MetroRadiance.UI.Controls
{
	public class BindableTextBlock : TextBlock
	{
		#region TextTemplates 依存関係プロパティ

		public DataTemplateCollection TextTemplates
		{
			get { return (DataTemplateCollection)this.GetValue(TextTemplatesProperty); }
			set { this.SetValue(TextTemplatesProperty, value); }
		}
		public static readonly DependencyProperty TextTemplatesProperty =
			DependencyProperty.Register("TextTemplates", typeof(DataTemplateCollection), typeof(BindableTextBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnNeedUpdate));

		#endregion

		#region TextSource 依存関係プロパティ

		public IEnumerable<object> TextSource
		{
			get { return (IEnumerable<object>)this.GetValue(TextSourceProperty); }
			set { this.SetValue(TextSourceProperty, value); }
		}
		public static readonly DependencyProperty TextSourceProperty =
			DependencyProperty.Register("TextSource", typeof(IEnumerable<object>), typeof(BindableTextBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnNeedUpdate));

		private static void OnNeedUpdate(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = d as BindableTextBlock;
			instance?.Update();
		}

		#endregion


		public BindableTextBlock()
		{
			this.TextTemplates = new DataTemplateCollection();
			this.Loaded += (sender, e) => this.Update();
		}

		private IEnumerable<InlineHolder> CreateTemplateInstance(IEnumerable<object> textSourcePart)
		{
			return textSourcePart.Select(o =>
			{
				InlineHolder result;

				var template = this.TextTemplates.FirstOrDefault(dt => (Type)dt.DataType == o.GetType());
				if (template == null)
				{
					result = new InlineHolder { Inlines = new InlineSimpleCollection(new Inline[] { new Run(o.ToString()) }) };
				}
				else
				{
					result = (InlineHolder)template.LoadContent();
					result.DataContext = o;
					foreach (var inline in result.Inlines)
					{
						inline.DataContext = o;
					}
				}
				return result;
			});

		}

		private void Update()
		{
			this.Inlines.Clear();

			if (this.TextSource == null) return;

			foreach (var inline in this.CreateTemplateInstance(this.TextSource).SelectMany(inlineHolder => inlineHolder.Inlines))
			{
				this.Inlines.Add(inline);
			}
		}

	}

	[ContentProperty("Inlines")]
	public class InlineHolder : FrameworkElement
	{
		public InlineHolder() {
			this.Inlines = new InlineSimpleCollection(); }

		public InlineSimpleCollection Inlines { get; set; }
	}

	public class InlineSimpleCollection : List<Inline>
	{
		public InlineSimpleCollection()
		{

		}
		public InlineSimpleCollection(IEnumerable<Inline> source)
		{
			this.AddRange(source);
		}
	}
}

#if AVALONIA
using Avalonia.Interactivity;
#elif WPF
using System.Windows;
#endif

#if AVALONIA
namespace CefNet.Avalonia
#elif WPF
namespace CefNet.Wpf
#endif
{
	/// <summary>
	/// Represents find results.
	/// </summary>
	public sealed class TextFoundRoutedEventArgs : RoutedEventArgs, ITextFoundEventArgs
	{
		public TextFoundRoutedEventArgs(int identifier, int count, CefRect selectionRect, int index, bool finalUpdate)
			: base(WebView.TextFoundEvent)
		{
			this.ID = identifier;
			this.Count = count;
			this.SelectionRect = selectionRect;
			this.Index = index;
			this.FinalUpdate = finalUpdate;
		}

		public int ID { get; }

		public int Index { get; }

		public int Count { get; }

		public CefRect SelectionRect { get; }

		public bool FinalUpdate { get; }

	}
}

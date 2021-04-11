using Avalonia;
using Avalonia.Controls;

namespace System.Application.UI.Views.Controls
{
	public class InfoMessage : Label
	{
		public static readonly StyledProperty<int> IconSizeProperty =
			AvaloniaProperty.Register<InfoMessage, int>(nameof(IconSize), 20);

		public int IconSize
		{
			get => GetValue(IconSizeProperty);
			set => SetValue(IconSizeProperty, value);
		}
	}
}
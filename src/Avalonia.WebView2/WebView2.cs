using WebView2BaseType = Avalonia.Controls.Shapes.Rectangle;

namespace Avalonia.Controls;

public partial class WebView2 : WebView2BaseType
{
    public static bool IsSupported { get; }

    public static readonly DirectProperty<WebView2, Uri?> SourceProperty =
            AvaloniaProperty.RegisterDirect<WebView2, Uri?>(
                nameof(Uri),
                x => x.source,
                (x, y) => x.Source = y);

    Uri? source;

    public Uri? Source
    {
        get => source;
        set => SetAndRaise(SourceProperty, ref source, value);
    }
}
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System.Xml;

namespace BD.WTTS.UI;

public static class LogSyntaxHighlighting
{
    static readonly Lazy<IHighlightingDefinition> definition = new(() =>
    {
        using var stream = AssetLoader.Open(new Uri("avares://BD.WTTS.Client.Avalonia/UI/Assets/Log.xshd"));
        using var reader = XmlReader.Create(stream);
        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
    });

    // Both log viewers previously used TextMate's fixed Dark theme, not the app theme.
    public static void Apply(TextEditor editor)
    {
        editor.SyntaxHighlighting = definition.Value;
        editor.Background = new SolidColorBrush(Color.Parse("#1E1E1E"));
        editor.Foreground = new SolidColorBrush(Color.Parse("#D4D4D4"));
    }
}

using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System.Xml;

namespace BD.WTTS.UI;

public static class LogSyntaxHighlighting
{
    static readonly Lazy<IHighlightingDefinition> dark = new(() => Load(lightTheme: false));
    static readonly Lazy<IHighlightingDefinition> light = new(() => Load(lightTheme: true));

    static IHighlightingDefinition Load(bool lightTheme)
    {
        using var stream = AssetLoader.Open(new Uri("avares://BD.WTTS.Client.Avalonia/UI/Assets/Log.xshd"));
        using var reader = XmlReader.Create(stream);
        var syntax = HighlightingLoader.LoadXshd(reader);
        if (lightTheme)
        {
            // Both palettes use the same token rules and emphasis.
            foreach (var color in syntax.Elements.OfType<XshdColor>())
            {
                var foreground = color.Name switch
                {
                    "Debug" or "Constant" => 0xFF215A9A,
                    "Info" => 0xFF17643B,
                    "Warning" => 0xFF8A4B00,
                    "Error" or "Exception" => 0xFFB42329,
                    "Category" => 0xFF455B75,
                    "Separator" => 0xFF647286,
                    "String" => 0xFF784321,
                    _ => 0xFF506078,
                };
                color.Foreground = new SimpleHighlightingBrush(Color.FromUInt32(foreground));
            }
        }
        return HighlightingLoader.Load(syntax, HighlightingManager.Instance);
    }

    public static void Apply(TextEditor editor)
    {
        editor.ActualThemeVariantChanged += static (sender, _) => UpdatePalette((TextEditor)sender!);
        UpdatePalette(editor);
    }

    static void UpdatePalette(TextEditor editor)
    {
        var variant = editor.ActualThemeVariant;
        // Let high contrast use the system text brush for every token.
        editor.SyntaxHighlighting = variant == Styling.CustomTheme.HighContrastTheme ? null
            : variant == ThemeVariant.Dark ? dark.Value : light.Value;
    }
}

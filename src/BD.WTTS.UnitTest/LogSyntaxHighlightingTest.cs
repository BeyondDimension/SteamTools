using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System.Xml;

namespace BD.WTTS.UnitTest;

public sealed class LogSyntaxHighlightingTest
{
    [TestCase("2026-11-15 10:20:30.123 +08:00 [INFO] 中文", "10:20:30.123 +08:00", "Date")]
    [TestCase("[Error] failure", "[Error]", "Error")]
    [TestCase("2026-11-15 10:20:30.123|Warn|Microsoft.Hosting.Lifetime|started", "Warn", "Warning")]
    [TestCase("2026-11-15 10:20:30.123|Warn|Microsoft.Hosting.Lifetime|started", "Microsoft.Hosting.Lifetime", "Category")]
    [TestCase("2026-11-15 10:20:30.123|Warn|Microsoft.Hosting.Lifetime|started", "|", "Separator")]
    [TestCase("value \"ERROR within a quoted string\"", "ERROR", "String")]
    [TestCase("   at Example.Run()", "   at Example.Run()", "Stack")]
    [TestCase("System.IO.IOException", "System.IO.IOException", "Exception")]
    [TestCase("https://example.invalid/path", "https://example.invalid/path", "Constant")]
    public void TokensRetainTheirLogMeaning(string text, string token, string colorName)
    {
        using var stream = typeof(LogSyntaxHighlightingTest).Assembly.GetManifestResourceStream("Log.xshd")!;
        using var reader = XmlReader.Create(stream);
        var definition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        using var highlighter = new DocumentHighlighter(new TextDocument(text), definition);
        var line = highlighter.HighlightLine(1);
        var start = text.IndexOf(token, StringComparison.Ordinal);
        Assert.That(line.Sections.Any(section => section.Offset <= start &&
            section.Offset + section.Length >= start + token.Length && section.Color.Name == colorName), Is.True);
    }
}

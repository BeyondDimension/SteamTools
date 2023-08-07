namespace BD.WTTS.Models;

[MP2Obj(SerializeLayout.Explicit)]
public sealed partial class HyperlinkModel
{
    [MP2Constructor]
    public HyperlinkModel(string text, string url)
    {
        Text = text;
        Url = url;
#if !TOOL_OSL
        Command = ReactiveCommand.CreateFromTask(async () =>
        {
            await Browser2.OpenAsync(url);
        });
#endif
    }

#if !TOOL_OSL
    public HyperlinkModel(string text, ICommand command)
    {
        Text = text;
        Command = command;
    }
#endif

    [MP2Key(0)]
    public string? Text { get; }

    [MP2Key(1)]
    public string? Url { get; }

#if !TOOL_OSL
    [MP2Ignore]
    public ICommand? Command { get; }
#endif
}

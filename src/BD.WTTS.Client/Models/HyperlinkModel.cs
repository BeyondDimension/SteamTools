namespace BD.WTTS.Models;

public sealed class HyperlinkModel
{
    public HyperlinkModel(string text, string url)
    {
        Text = text;
        Url = url;
        Command = ReactiveCommand.CreateFromTask(async () =>
        {
            await Browser2.OpenAsync(url);
        });
    }

    public HyperlinkModel(string text, ICommand command)
    {
        Text = text;
        Command = command;
    }

    public string? Text { get; }

    public string? Url { get; }

    public ICommand Command { get; }
}

using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class DebugPage : PageBase<DebugPageViewModel>
{
    public static IReadOnlyList<string> CommandSuggestions { get; } =
        ["/asm", "/dialog", "/gc", "/messagebox", "/notify", "/plu", "/toast", "/window"];

    public DebugPage()
    {
        InitializeComponent();
        this.SetViewModel<DebugPageViewModel>();
    }

    void Send_OnClick(object? sender, RoutedEventArgs e) => CommandInput.Submit();

    async void CommandInput_CommandSubmitted(object? sender, string text)
    {
        if (ViewModel is not { } model) return;
        var command = text.AsSpan().Trim();
        if (!command.IsEmpty && command[0] == '/') command = command[1..].TrimStart();
        if (command.IsEmpty) return;
        try
        {
            await model.Debug(command.Length == text.Length ? text : command.ToString());
        }
        catch (Exception ex)
        {
            model.ConsoleLogs.Append(ex + Environment.NewLine);
        }
    }
}

namespace BD.WTTS.UI.Views.Controls;

/// <summary>Command suggestions are completed first and sent only by a separate submit action.</summary>
public sealed class LogCommandBox : AutoCompleteBox
{
    bool? sendOnEnterRelease;

    protected override Type StyleKeyOverride => typeof(AutoCompleteBox);

    public event EventHandler<string>? CommandSubmitted;

    public LogCommandBox()
    {
        MinimumPrefixLength = 1;
        MaxDropDownHeight = 260;
        TextFilter = static (input, command) => input.StartsWith('/')
            ? command.AsSpan().TrimStart('/').TrimStart('!').StartsWith(input.AsSpan(1), StringComparison.OrdinalIgnoreCase)
            : input.StartsWith('!') && command.StartsWith(input, StringComparison.OrdinalIgnoreCase);
        AddHandler(KeyDownEvent, OnCommandKeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, OnCommandKeyUp, RoutingStrategies.Tunnel);
        LostFocus += (_, _) => sendOnEnterRelease = null;
    }

    public void Submit()
    {
        var command = Text;
        if (string.IsNullOrWhiteSpace(command)) return;
        SetCurrentValue(IsDropDownOpenProperty, false);
        SetCurrentValue(TextProperty, string.Empty);
        CommandSubmitted?.Invoke(this, command);
    }

    void OnCommandKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        // Commit a suggestion on the first press. Key repeats must not send the result.
        sendOnEnterRelease ??= !IsDropDownOpen;
        e.Handled = sendOnEnterRelease == true || !IsDropDownOpen;
    }

    void OnCommandKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sendOnEnterRelease is not bool send) return;
        sendOnEnterRelease = null;
        e.Handled = true;
        if (send) Submit();
    }
}

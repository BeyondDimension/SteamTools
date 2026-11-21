using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using BD.WTTS.Settings.Abstractions;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.Views.Pages;

public partial class ArchiSteamFarmPage : PageBase<ArchiSteamFarmPlusPageViewModel>
{
    public static IValueConverter RunOrStopIconConverter { get; } = new FuncValueConverter<bool, Symbol>(
        running => running ? Symbol.Pause : Symbol.Play);

    public static IValueConverter RunOrStopTextConverter { get; } = new FuncValueConverter<bool, string>(
        running => running ? AppResources.ASF_Stop : AppResources.ASF_Start);

    public static IValueConverter ConsoleFontSizeConverter { get; } = new FuncValueConverter<int, double>(
        size => Math.Clamp(size, IASFSettings.DefaultMinRangeConsoleFontSize, IASFSettings.DefaultMaxRangeConsoleFontSize));

    public static IValueConverter ConsoleMaxLinesConverter { get; } = new FuncValueConverter<int, int>(
        lines => Math.Clamp(lines, IASFSettings.DefaultMinRangeConsoleMaxLine, IASFSettings.DefaultMaxRangeConsoleMaxLine));

    private readonly ArchiSteamFarmCommandHistory commandHistory = new();

    public ArchiSteamFarmPage()
    {
        InitializeComponent();
        DataContext ??= new ArchiSteamFarmPlusPageViewModel();

        CommandInput.AddHandler(InputElement.KeyDownEvent, CommandInput_KeyDown, RoutingStrategies.Tunnel);
    }

    private void CommandInput_KeyDown(object? sender, KeyEventArgs e)
    {
        // Leave the navigation and cancellation keys to AutoCompleteBox while it shows suggestions.
        if (CommandInput.IsDropDownOpen)
            return;

        switch (e.Key)
        {
            case Key.Up:
            case Key.Down:
                if (commandHistory.Count != 0)
                {
                    if (commandHistory.CurrentIndex == -1)
                        commandHistory.CurrentCommand = CommandInput.Text ?? string.Empty;

                    CommandInput.Text = e.Key == Key.Up ? commandHistory.GetPrevious() : commandHistory.GetNext();
                }
                e.Handled = true;
                break;
            case Key.Escape:
                CommandInput.Text = commandHistory.CurrentCommand;
                ResetHistoryNavigation();
                e.Handled = true;
                break;
        }
    }

    private void SendCommand_Click(object? sender, RoutedEventArgs e) => CommandInput.Submit();

    private void CommandInput_CommandSubmitted(object? sender, string command)
    {
        ASFService.Current.ShellMessageInput(command);
        commandHistory.AddCommandToHistory(command);
        ResetHistoryNavigation();
    }

    private void ResetHistoryNavigation()
    {
        commandHistory.CurrentIndex = -1;
        commandHistory.CurrentCommand = string.Empty;
    }
}

using Avalonia.Controls;
using Avalonia.Input;

namespace BD.WTTS.UI.Views.Pages;

public partial class BorderlessGamePage : PageBase<BorderlessGamePageViewModel>
{
    public BorderlessGamePage()
    {
        InitializeComponent();
        DataContext ??= new BorderlessGamePageViewModel();

        move.PointerPressed += MovePointerPressed;
        move.PointerReleased += MovePointerReleased;
        move.PointerCaptureLost += (_, _) => move.Opacity = 1;
        ButtonGroup.SizeChanged += ButtonGroupSizeChanged;
    }

    void MovePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not BorderlessGamePageViewModel viewModel ||
            viewModel.PickWindowCommand is not { } command || !command.CanExecute(null))
            return;

        e.Pointer.Capture(move);
        move.Opacity = 0;
        try
        {
            command.Execute(null);
        }
        catch
        {
            e.Pointer.Capture(null);
            move.Opacity = 1;
            throw;
        }
    }

    void MovePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        move.Opacity = 1;
    }

    void ButtonGroupSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        const int threeLineButtonWidth = 554;
        const int fourLineButtonWidth = 418;
        var height = e.NewSize.Width < fourLineButtonWidth ? 200 : e.NewSize.Width < threeLineButtonWidth ? 150 : 100;
        if (ButtonGroup.Height != height)
            ButtonGroup.Height = height;
    }
}

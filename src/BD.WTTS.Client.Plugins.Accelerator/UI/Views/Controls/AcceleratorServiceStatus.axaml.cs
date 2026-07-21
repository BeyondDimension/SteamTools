using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class AcceleratorServiceStatus : UserControl
{
    public AcceleratorServiceStatus()
    {
        InitializeComponent();
    }

    private void ConnectTestButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var expander = button.FindAncestorOfType<FASettingsExpander>();
            if (expander != null) expander.IsExpanded = true;
        }
    }
}
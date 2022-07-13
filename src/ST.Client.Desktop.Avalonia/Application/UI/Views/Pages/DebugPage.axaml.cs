using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.Application.UI.Views.Windows;

namespace System.Application.UI.Views.Pages
{
    public class DebugPage : ReactiveUserControl<DebugPageViewModel>
    {
        public DebugPage()
        {
            InitializeComponent();

            var testWindow = this.FindControl<Button>("TestWindow");

            if (testWindow != null)
            {
                testWindow.Tapped += TestWindow_Tapped;
            }
        }

        private void ConsoleShell_CommandSubmit(object? sender, CommandEventArgs e)
        {
            var cmd = e.Command;

            if (ViewModel != null)
            {
                if (string.IsNullOrEmpty(cmd))
                    return;

                var cmds = cmd.ToLowerInvariant().Split(' ');
                ViewModel.DebugString += cmds[0] + Environment.NewLine;

                switch (cmds[0])
                {
                    case "appinfo":
                        ViewModel.DebugString += AboutAppInfoPopup.GetInfoString();
                        break;
                    case "testwindow":
                        new DebugWindow().Show();
                        break;
                    default:
                        ViewModel?.Debug(e.Command);
                        break;
                }
            }
        }

        private void TestWindow_Tapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new DebugWindow().Show();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
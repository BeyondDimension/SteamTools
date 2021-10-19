using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;
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

        private void TestWindow_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            new DebugWindow().Show();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
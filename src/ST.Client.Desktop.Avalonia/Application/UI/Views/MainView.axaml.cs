using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;
using System.Runtime.InteropServices;

namespace System.Application.UI.Views
{
    public class MainView : ReactiveUserControl<MainWindowViewModel>
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var sp = this.FindControl<StackPanel>("titleMenu");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                sp.Margin = new Avalonia.Thickness(0, 0, 140, 0);
            else
                sp.Margin = new Avalonia.Thickness(0, 6, 10, 0);

        }
    }
}
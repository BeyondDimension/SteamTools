using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.Services;
using System.Collections.Generic;
using System.Properties;

namespace System.Application.UI.Views.Windows
{
    public class AddAuthWindow : FluentWindow
    {
        public AddAuthWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var winAuthBtn = this.FindControl<Button>("ImportWinAuthBtn");
            winAuthBtn.Click += WinAuthBtn_Click;
        }

        private void WinAuthBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter> {
                    new FileDialogFilter { Name = "Text Files", Extensions = new List<string> { "txt" } },
                    new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } },
                },
                Title = ThisAssembly.AssemblyTrademark,
                AllowMultiple = false,
            };
            fileDialog.ShowAsync(IDesktopAvaloniaAppService.Instance.MainWindow).ContinueWith(s =>
            {
                if (s != null && s.Result.Length > 0)
                {
                    AuthService.Current.ImportWinAuthenticators(s.Result[0]);
                }
            });

        }
    }
}

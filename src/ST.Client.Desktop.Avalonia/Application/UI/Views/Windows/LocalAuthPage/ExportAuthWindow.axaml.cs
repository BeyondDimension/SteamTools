using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Properties;

namespace System.Application.UI.Views.Windows
{
    public class ExportAuthWindow : FluentWindow<ExportAuthWindowViewModel>
    {
        public ExportAuthWindow() : base()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var selectPathButton = this.FindControl<Button>("SelectPathButton");
            selectPathButton.Click += SelectPathButton_Click;
        }

        private async void SelectPathButton_Click(object? sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog
            {
                Filters = new List<FileDialogFilter> {
                    new FileDialogFilter { Name = "MsgPack Files", Extensions = new List<string> { "mpo" } },
                    new FileDialogFilter { Name = "Data Files", Extensions = new List<string> { "dat" } },
                    new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } },
                },
                Title = ThisAssembly.AssemblyTrademark,
                InitialFileName = ExportAuthWindowViewModel.DefaultExportAuthFileName,
            };
            var result = await fileDialog.ShowAsync(IDesktopAvaloniaAppService.Instance.MainWindow);

            if (this.DataContext is ExportAuthWindowViewModel vm)
            {
                vm.Path = result;
            }
        }
    }
}

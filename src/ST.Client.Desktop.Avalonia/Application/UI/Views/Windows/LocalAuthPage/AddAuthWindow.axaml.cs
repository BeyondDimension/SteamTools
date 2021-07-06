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
    public class AddAuthWindow : FluentWindow<AddAuthWindowViewModel>
    {
        public AddAuthWindow() : base()
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

            var sdaBtn = this.FindControl<Button>("ImportSDABtn");
            sdaBtn.Click += SdaBtn_Click;

            var sppBtn = this.FindControl<Button>("ImportSteamToolsBtn");
            sppBtn.Click += SppBtn_Click;
            var spp2Btn = this.FindControl<Button>("ImportSteamToolsV2Btn");
            spp2Btn.Click += SppV2Btn_Click;
        }

        private void SppV2Btn_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as AddAuthWindowViewModel;
            if (vm == null) return;
            var fileDialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter> {
                    new FileDialogFilter { Name = "MsgPack Files", Extensions = new List<string> { "mpo" } },
                    new FileDialogFilter { Name = "Data Files", Extensions = new List<string> { "dat" } },
                    new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } },
                },
                Title = ThisAssembly.AssemblyTrademark,
                AllowMultiple = false,
            };
            fileDialog.ShowAsync(IDesktopAvaloniaAppService.Instance.MainWindow).ContinueWith(s =>
            {
                if (s != null && s.Result.Length > 0)
                {
                    vm?.ImportSteamPlusPlusV2(s.Result[0]);
                }
            });
        }

        private void SppBtn_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as AddAuthWindowViewModel;
            if (vm == null) return;
            var fileDialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter> {
                    new FileDialogFilter { Name = "Data Files", Extensions = new List<string> { "dat" } },
                    new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } },
                },
                Title = ThisAssembly.AssemblyTrademark,
                AllowMultiple = false,
            };
            fileDialog.ShowAsync(IDesktopAvaloniaAppService.Instance.MainWindow).ContinueWith(s =>
            {
                if (s != null && s.Result.Length > 0)
                {
                    vm.ImportSteamPlusPlusV1(s.Result[0]);
                }
            });
        }

        private void SdaBtn_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as AddAuthWindowViewModel;
            if (vm == null) return;
            var fileDialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter> {
                    new FileDialogFilter { Name = "MaFile Files", Extensions = new List<string> { "maFile" } },
                    new FileDialogFilter { Name = "JSON Files", Extensions = new List<string> { "json" } },
                    new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } },
                },
                Title = ThisAssembly.AssemblyTrademark,
                AllowMultiple = false,
            };
            fileDialog.ShowAsync(IDesktopAvaloniaAppService.Instance.MainWindow).ContinueWith(s =>
            {
                if (s != null && s.Result.Length > 0)
                {
                    vm.ImportSDA(s.Result[0]);
                }
            });
        }

        private void WinAuthBtn_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as AddAuthWindowViewModel;
            if (vm == null) return;
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
                    vm.ImportWinAuth(s.Result[0]);
                }
            });

        }
    }
}

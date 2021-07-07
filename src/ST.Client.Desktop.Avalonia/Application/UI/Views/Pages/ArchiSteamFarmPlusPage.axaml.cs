using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Properties;
using System.Application.Services;
using System.IO;
using System.Application.UI.Views.Controls;

namespace System.Application.UI.Views.Pages
{
    public class ArchiSteamFarmPlusPage : ReactiveUserControl<ArchiSteamFarmPlusPageViewModel>
    {
        private readonly ConsoleShell consoleShell;

        public ArchiSteamFarmPlusPage()
        {
            InitializeComponent();

            consoleShell = this.FindControl<ConsoleShell>("ConsoleLog");

            IArchiSteamFarmService.Instance.GetConsoleWirteFunc = (message) =>
            {
                MainThreadDesktop.BeginInvokeOnMainThread(() =>
                {
                    consoleShell.AppendLogText(message);
                });
            };

            consoleShell.CommandSubmit += ConsoleShell_CommandSubmit;
        }

        private void ConsoleShell_CommandSubmit(object? sender, CommandEventArgs e)
        {
            if (IArchiSteamFarmService.Instance.ReadLineTask is null)
            {
                IArchiSteamFarmService.Instance.ExecuteCommand(e.Command);
            }
            else
            {
                IArchiSteamFarmService.Instance.ReadLineTask.TrySetResult(e.Command);
            }
        }


        private async void SelectAsfPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            //var fileDialog = new OpenFileDialog
            //{
            //    Filters = new List<FileDialogFilter> {
            //        new FileDialogFilter { Name = "Exe Files", Extensions = new List<string> { "exe" } },
            //        new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } },
            //    },
            //    Title = ThisAssembly.AssemblyTrademark,
            //    AllowMultiple = false,
            //};

            //if (IASFService.Instance.IsArchiSteamFarmExists)
            //{
            //    fileDialog.Directory = Path.GetDirectoryName(IASFService.Instance.ArchiSteamFarmExePath);
            //}

            //var result = await fileDialog.ShowAsync(IDesktopAvaloniaAppService.Instance.MainWindow);
            //if (result.Any_Nullable())
            //{
            //    IASFService.Instance.SetArchiSteamFarmExePath(result[0]);
            //}
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
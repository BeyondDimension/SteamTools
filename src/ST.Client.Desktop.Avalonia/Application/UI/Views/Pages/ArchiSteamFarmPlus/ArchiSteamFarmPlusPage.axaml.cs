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


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
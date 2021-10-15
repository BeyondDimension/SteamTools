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
        readonly IArchiSteamFarmService asfService = IArchiSteamFarmService.Instance;
        readonly ConsoleShell consoleShell;

        public ArchiSteamFarmPlusPage()
        {
            InitializeComponent();

            consoleShell = this.FindControl<ConsoleShell>("ConsoleLog");

            consoleShell.CommandSubmit += ConsoleShell_CommandSubmit;
        }

        private void ConsoleShell_CommandSubmit(object? sender, CommandEventArgs e)
        {
            if (asfService.ReadLineTask is null)
            {
                asfService.ExecuteCommand(e.Command!);
            }
            else
            {
                asfService.ReadLineTask.TrySetResult(e.Command!);
            }
        }


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
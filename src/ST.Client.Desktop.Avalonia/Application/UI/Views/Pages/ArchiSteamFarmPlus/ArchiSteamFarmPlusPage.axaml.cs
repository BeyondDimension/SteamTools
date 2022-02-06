using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.Collections.Generic;
using System.IO;
using System.Properties;

namespace System.Application.UI.Views.Pages
{
    public class ArchiSteamFarmPlusPage : ReactiveUserControl<ArchiSteamFarmPlusPageViewModel>
    {
        readonly IArchiSteamFarmService asfService = IArchiSteamFarmService.Instance;

        public ArchiSteamFarmPlusPage()
        {
            InitializeComponent();
        }

        private void ConsoleShell_CommandSubmit(object? sender, CommandEventArgs e)
        {
            asfService.CommandSubmit(e.Command);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
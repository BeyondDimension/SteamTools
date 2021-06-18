using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Properties;
using System.Application.Services;

namespace System.Application.UI.Views.Pages
{
    public class ArchiSteamFarmPlusPage : ReactiveUserControl<ArchiSteamFarmPlusPageViewModel>
    {
        readonly TextBox commandTextbox;

        public ArchiSteamFarmPlusPage()
        {
            InitializeComponent();
            var selectAsfPath = this.FindControl<Button>("selectAsfPath");
            selectAsfPath.Click += SelectAsfPath_Click;
            commandTextbox = this.FindControl<TextBox>("CommandTextbox");
            commandTextbox.KeyUp += CommandTextbox_KeyUp;

            //var console = this.FindControl<TextBox>("console");
            //console.SelectionStart = 999;
        }

        private void CommandTextbox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter && !string.IsNullOrEmpty(commandTextbox.Text))
            {
                IArchiSteamFarmService.Instance.WirteLineCommand(commandTextbox.Text);
                commandTextbox.Text = "";
            }
        }

        private async void SelectAsfPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter> {
                    new FileDialogFilter { Name = "Exe Files", Extensions = new List<string> { "exe" } },
                    new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } },
                },
                Title = ThisAssembly.AssemblyTrademark,
                AllowMultiple = false,
            };
            var result = await fileDialog.ShowAsync(IDesktopAvaloniaAppService.Instance.MainWindow);
            if (result.Any_Nullable())
            {
                IArchiSteamFarmService.Instance.SetArchiSteamFarmExePath(result[0]);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
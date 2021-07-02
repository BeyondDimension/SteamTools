using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Properties;
using System.Application.Services;
using System.IO;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;

namespace System.Application.UI.Views.Pages
{
    public class ArchiSteamFarmPlusPage : ReactiveUserControl<ArchiSteamFarmPlusPageViewModel>
    {
        private readonly TextEditor _textEditor;

        public ArchiSteamFarmPlusPage()
        {
            InitializeComponent();
            //var selectAsfPath = this.FindControl<Button>("selectAsfPath");
            //selectAsfPath.Click += SelectAsfPath_Click;
            //commandTextbox = this.FindControl<TextBox>("CommandTextbox");
            //commandTextbox.KeyUp += CommandTextbox_KeyUp;

            _textEditor = this.FindControl<TextEditor>("Editor");
            //_textEditor.ShowLineNumbers = true;

            IArchiSteamFarmService.Instance.GetConsoleWirteFunc = (message) =>
            {
                _textEditor.AppendText(message += Environment.NewLine);
            };

            //_textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("PowerShell");
            //_textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            //_textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            //_textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy();
        }

        //private void CommandTextbox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        //{
        //    if (e.Key == Avalonia.Input.Key.Enter && !string.IsNullOrEmpty(commandTextbox.Text))
        //    {
        //        IArchiSteamFarmService.Instance.ExecuteCommand(commandTextbox.Text);
        //        commandTextbox.Text = "";
        //    }
        //}

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
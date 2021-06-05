using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Properties;

namespace System.Application.UI.Views.Pages
{
    public class ProxyScriptManagePage : ReactiveUserControl<ProxyScriptManagePageViewModel>
    {
        public ProxyScriptManagePage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var th = this.FindControl<UserControl>("u");
            //var item = this.FindControl<Border>("item");

            //th.GetObservable(UserControl.WidthProperty).Subscribe(v =>
            //{
            //    if (v < 800)
            //        item.Width = 800;
            //    else
            //        item.Width = 450;
            //});
        }

        private void AddNewScriptButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter> {
                    new FileDialogFilter { Name = "JavaScript Files", Extensions = new List<string> { "js" } },
                    new FileDialogFilter { Name = "Text Files", Extensions = new List<string> { "txt" } },
                    new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } },
                },
                Title = ThisAssembly.AssemblyTrademark,
                AllowMultiple = false,
            };
            fileDialog.ShowAsync(IDesktopAvaloniaAppService.Instance.MainWindow).ContinueWith(async (s) =>
            {
                if (s != null && s.Result.Length > 0)
                {
                    await ProxyService.Current.AddNewScript(s.Result[0]);
                }
            });
        }
    }
}
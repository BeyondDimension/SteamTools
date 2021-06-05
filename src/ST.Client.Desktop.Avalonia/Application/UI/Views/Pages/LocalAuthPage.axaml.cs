using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Pages
{
    public class LocalAuthPage : ReactiveUserControl<LocalAuthPageViewModel>
    {
        public LocalAuthPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
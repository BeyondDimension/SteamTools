using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Pages
{
    public class AboutPage : ReactiveUserControl<AboutPageViewModel>
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
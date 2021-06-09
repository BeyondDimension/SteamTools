using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Pages
{
    public class ArchiSteamFarmPlusPage : UserControl
    {
        public ArchiSteamFarmPlusPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
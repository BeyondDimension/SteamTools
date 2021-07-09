using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Pages
{
    public partial class ASF_BotPage : ReactiveUserControl<ArchiSteamFarmPlusPageViewModel>
    {
        public ASF_BotPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

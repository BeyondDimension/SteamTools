using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Pages
{
    public class CommunityProxyPage : ReactiveUserControl<CommunityProxyPageViewModel>
    {
        public CommunityProxyPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

        }
    }
}
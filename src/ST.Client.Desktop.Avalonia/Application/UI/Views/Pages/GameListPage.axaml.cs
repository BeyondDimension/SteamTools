using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;
using System.Reactive.Disposables;

namespace System.Application.UI.Views.Pages
{
    public class GameListPage : ReactiveUserControl<GameListPageViewModel>
    {
        public GameListPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
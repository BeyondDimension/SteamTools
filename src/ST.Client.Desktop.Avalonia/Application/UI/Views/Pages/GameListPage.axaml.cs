using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.Reactive.Disposables;

namespace System.Application.UI.Views.Pages
{
    public class GameListPage : ReactiveUserControl<GameListPageViewModel>
    {
        public GameListPage()
        {
            InitializeComponent();

            var apps = this.FindControl<ItemsRepeater>("Apps");
            apps.PointerPressed += App_PointerPressed;
        }

        private static void App_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.Source is Control c)
            {
                var border = c.FindParentControl<AppCard>("AppCard");
                if (border is not null)
                {
                    var flyout = FlyoutBase.GetAttachedFlyout(border);
                    flyout?.ShowAt(border, true);
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
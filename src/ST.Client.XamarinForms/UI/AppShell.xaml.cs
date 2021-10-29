using System;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace System.Application.UI
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        readonly IPlatformPageRouteService? pageRouteService = IPlatformPageRouteService.Instance;
        public AppShell()
        {
            InitializeComponent();

            if (IViewModelManager.Instance.MainWindow is MainWindowViewModel mainWindow)
            {
                BindingContext = mainWindow;

                foreach (var tab in mainWindow.TabItems.Concat(mainWindow.FooterTabItems))
                {
                    if (pageRouteService?.IsUseNativePage(tab.Id) ?? false)
                    {
                        var tab_item = new MenuItem
                        {
                            IconImageSource = tab.IconKey,
                            BindingContext = tab,
                        };
                        tab_item.Clicked += (_, _) => pageRouteService.GoToNativePage(tab.Id);
                        tab_item.SetBinding(MenuItem.TextProperty, nameof(tab.Name), BindingMode.OneWay);
                        Items.Add(tab_item);
                    }
                    else
                    {
                        var tab_item = new FlyoutItem
                        {
                            Icon = tab.IconKey,
                            BindingContext = tab,
                        };
                        tab_item.SetBinding(BaseShellItem.TitleProperty, nameof(tab.Name), BindingMode.OneWay);
                        tab_item.Items.Add(new ShellContent
                        {
                            Route = tab.IconKey!.TrimEnd("ViewModel"),
                            ContentTemplate = new DataTemplate(typeof(UnderConstructionPage)),
                        });
                        Items.Add(tab_item);
                    }
                }
            }

            //Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            //Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

        private /*async*/ void OnMenuItemClicked(object sender, EventArgs e)
        {
            //await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}

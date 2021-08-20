using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using System.Application.UI.ViewModels;
using System.Runtime.InteropServices;

namespace System.Application.UI.Views
{
    public class MainView : ReactiveUserControl<MainWindowViewModel>
    {
        public MainView()
        {
            InitializeComponent();

            //var sp = this.FindControl<StackPanel>("titleMenu");
            //if (sp != null)
            //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //        sp.Margin = new Avalonia.Thickness(0, 0, 140, 0);
            //    else
            //        sp.Margin = new Avalonia.Thickness(0, 6, 10, 0);

            var avater = this.FindControl<Image>("avater");
            var nav = this.FindControl<NavigationView>("NavigationView");
            var back = this.FindControl<ExperimentalAcrylicBorder>("NavBarBackground");
            if (back != null && nav != null)
            {
                nav.GetObservable(NavigationView.IsPaneOpenProperty)
                    .Subscribe(x => back.Width = x ? 240 : 48);
            }

            if (back != null && nav != null)
            {
                nav.GetObservable(NavigationView.IsPaneOpenProperty)
                  .Subscribe(x =>
                  {
                      if (!x)
                      {
                          avater.Width = 26;
                          avater.Height = 26;
                          avater.Margin = new Thickness(-4, 0, 10, 0);
                      }
                      else
                      {
                          avater.Width = 64;
                          avater.Height = 64;
                          avater.Margin = new Thickness(10, 0);
                      }
                  });
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
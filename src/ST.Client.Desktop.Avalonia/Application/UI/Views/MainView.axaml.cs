using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using System.Application.UI.ViewModels;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
                    .Subscribe(x =>
                    {
                        if (nav.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
                        {
                            back.IsVisible = false;
                            //back.Width = 0;
                        }
                        else
                        {
                            back.Width = x ? 240 : 48;
                        }
                    });
            }

            if (avater != null && nav != null)
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

            var frame = this.FindControl<Frame>("frame");
            if (frame != null && nav != null)
            {
                if (nav.IsBackButtonVisible)
                {
                    nav.BackRequested += (sender, e) =>
                    {
                        frame.GoBack();
                    };
                }
                nav.GetObservable(NavigationView.SelectedItemProperty)
                    .Subscribe(x =>
                    {
                        if (x != null)
                        {
                            var page = frame.DataTemplates.FirstOrDefault(f => f.Match(x));
                            if (page is DataTemplate template)
                            {
                                frame.Navigate(template.Build(null).GetType());
                            }
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Application.UI.Views
{
    public class MainView : ReactiveUserControl<MainWindowViewModel>
    {
        private readonly IBrush? _backgroundTemp;

        public MainView()
        {
            InitializeComponent();

            var avatar = this.FindControl<Image>("avatar");
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

            if (avatar != null && nav != null)
            {
                nav.GetObservable(NavigationView.IsPaneOpenProperty)
                  .Subscribe(x =>
                  {
                      if (!x)
                      {
                          avatar.Width = 26;
                          avatar.Height = 26;
                          avatar.Margin = new Thickness(-4, 0, 10, 0);
                      }
                      else
                      {
                          avatar.Width = 64;
                          avatar.Height = 64;
                          avatar.Margin = new Thickness(10, 0);
                      }


                      if (avatar.Clip is EllipseGeometry ellipse)
                      {
                          ellipse.Rect = new Rect(0, 0, avatar.Width, avatar.Height);
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

            //UISettings.EnableDesktopBackground.Subscribe(x =>
            //{
            //    Background = x ? null : _BackGroundTemp;
            //});

            var title = this.FindControl<TitleBar>("title");
            if (title != null)
            {
                _backgroundTemp = title.Background;
                UISettings.EnableDesktopBackground.Subscribe(x =>
                {
                    title.Background = x ? null : _backgroundTemp;
                });
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
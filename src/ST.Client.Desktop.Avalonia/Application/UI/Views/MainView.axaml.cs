using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using AvaloniaGif;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Application.UI.Views.Pages;
using Avalonia.Layout;

namespace System.Application.UI.Views
{
    public class MainView : ReactiveUserControl<MainWindowViewModel>
    {
        private readonly IBrush? _backgroundTemp;

        private static IReadOnlyDictionary<Type, Type> PageTypes { get; }

        static MainView()
        {
            PageTypes = new Dictionary<Type, Type>
            {
                { typeof(StartPageViewModel), typeof(StartPage) },
                { typeof(CommunityProxyPageViewModel), typeof(CommunityProxyPage) },
                { typeof(ProxyScriptManagePageViewModel), typeof(ProxyScriptManagePage) },
                { typeof(SteamAccountPageViewModel), typeof(SteamAccountPage) },
                { typeof(SettingsPageViewModel), typeof(SettingsPage) },
                { typeof(AboutPageViewModel), typeof(AboutPage) },
                { typeof(GameListPageViewModel), typeof(GameListPage) },
                { typeof(LocalAuthPageViewModel), typeof(LocalAuthPage) },
                { typeof(GameRelatedPageViewModel), typeof(GameRelatedPage) },
                { typeof(ArchiSteamFarmPlusPageViewModel), typeof(ArchiSteamFarmPlusPage) },
                //{ typeof(GameRelated_BorderlessPageViewModel), typeof(GameRelated_BorderlessPage) },
#if DEBUG
                { typeof(DebugPageViewModel), typeof(DebugPage) },
#if WINDOWS
                { typeof(DebugWebViewPageViewModel), typeof(DebugWebViewPage) },
#endif
                { typeof(AccountPageViewModel), typeof(AccountPage) }, 
#endif
            };
        }

        public MainView()
        {
            InitializeComponent();

            var avatar = this.FindControl<Control>("avatar");
            var nav = this.FindControl<NavigationView>("NavigationView");
            var back = this.FindControl<ExperimentalAcrylicBorder>("NavBarBackground");
            //var bg = this.FindControl<Control>("Background");

            //if (!TitleBar.GetIsVisible())
            //{
            //    bg.Margin = new(0, 0, 0, 0);
            //}

            //if (back != null && nav != null)
            //{
            //    nav.GetObservable(NavigationView.IsPaneOpenProperty)
            //        .Subscribe(x =>
            //        {
            //            if (nav.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            //            {
            //                back.IsVisible = false;
            //                //back.Width = 0;
            //            }
            //            else
            //            {
            //                back.Width = x ? 240 : 48;
            //            }
            //        });
            //}

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
                //frame.Navigating += (s, e) =>
                //{
                //    DebugPageViewModel.Instance.DebugString += $"Frame Navigating: {e?.SourcePageType?.Name} {Environment.NewLine}";
                //};
                //frame.Navigated += (s, e) =>
                //{
                //    DebugPageViewModel.Instance.DebugString += $"Frame Navigated: {e?.SourcePageType?.Name} {Environment.NewLine}";
                //};
                //frame.NavigationFailed += (s, e) =>
                //{
                //    DebugPageViewModel.Instance.DebugString += $"TabItem Change Error: {e.Exception} {e.SourcePageType} {Environment.NewLine}";
                //    Log.Error("TabItem Changed", e.Exception, nameof(MainWindowViewModel));
                //};

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
                            //var page = frame.DataTemplates.FirstOrDefault(f => f.Match(x));
                            //if (page is DataTemplate template)
                            //{
                            //    frame.Navigate(template.Build(null).GetType());
                            //}

                            frame.Navigate(PageTypes[x.GetType()]);
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

            if (nav != null)
            {
                this.GetObservable(BoundsProperty)
                        .Subscribe(x =>
                        {
                            switch (x.Width)
                            {
                                case < 1000:
                                    nav.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
                                    nav.IsPaneOpen = false;
                                    break;
                                default:
                                    nav.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                                    break;
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
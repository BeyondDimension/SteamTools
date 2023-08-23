using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using FluentAvalonia.Core;

namespace BD.WTTS.UI.Views.Controls;

public partial class AdControl : UserControl
{
    /// <summary>
    /// Defines the <see cref="AutoScroll"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> SupportCloseProperty =
        AvaloniaProperty.Register<AdControl, bool>(nameof(SupportClose), false);

    /// <summary>
    /// Defines the <see cref="AutoScrollInterval"/> property.
    /// </summary>
    public static readonly StyledProperty<AdvertisementOrientation> StandardProperty =
        AvaloniaProperty.Register<AdControl, AdvertisementOrientation>(nameof(Standard), AdvertisementOrientation.Horizontal);

    /// <summary>
    /// SupportClose
    /// </summary>
    public bool SupportClose
    {
        get => GetValue(SupportCloseProperty);
        set => SetValue(SupportCloseProperty, value);
    }

    /// <summary>
    /// Standard
    /// </summary>
    public AdvertisementOrientation Standard
    {
        get => GetValue(StandardProperty);
        set => SetValue(StandardProperty, value);
    }

    public AdControl()
    {
        InitializeComponent();

        if (CloseAdBtn != null)
        {
            this.GetObservable(SupportCloseProperty)
                .Subscribe(x => CloseAdBtn.IsVisible = x);

            CloseAdBtn.Click += (s, e) =>
            {
                RemoveAd();
            };
        }

        if (AdBanner != null)
        {
            this.GetObservable(StandardProperty)
                .Subscribe(x =>
                {
                    var bind = new Binding()
                    {
                        Source = AdvertiseService.Current,
                        Mode = BindingMode.OneWay,
                    };

                    if (Standard == AdvertisementOrientation.Vertical)
                    {
                        bind.Path = nameof(AdvertiseService.Current.VerticalBannerAdvertisements);
                    }
                    else
                    {
                        bind.Path = nameof(AdvertiseService.Current.HorizontalBannerAdvertisements);
                    }
                    AdBanner.Bind(CarouselBanner.ItemsSourceProperty, bind);
                });

            //AdBanner.ItemsSource.WhenAnyValue(x => x)
            //    .Subscribe(x =>
            //    {
            //        CheckItems(x);
            //    });

            AdBanner.PropertyChanged += AdBanner_PropertyChanged;

            //AdBanner.GetObservable(CarouselBanner.ItemsSourceProperty)
            //    .Subscribe(CheckItems);

            //banner.GetObservable(CarouselBanner.IsVisibleProperty)
            //    .Subscribe(_ => CheckItems(banner.Items));

            //UserService.Current.WhenValueChanged(x => x.User, false)
            //      .Subscribe(_ => Check());

            //AdvertiseService.Current.WhenValueChanged(x => x.IsShowAdvertise, false)
            //      .Subscribe(_ => CheckItems(banner.Items));
        }
    }

    private void AdBanner_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Carousel.ItemsSourceProperty)
        {
            CheckItems(AdBanner.ItemsSource);
        }
    }

    //protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    //{
    //    base.OnApplyTemplate(e);

    //    if (Standard == AdvertisementOrientation.Vertical)
    //    {
    //        AdBanner.ItemsSource = AdvertiseService.Current.VerticalBannerAdvertisements;
    //    }
    //    else
    //    {
    //        AdBanner.ItemsSource = AdvertiseService.Current.HorizontalBannerAdvertisements;
    //    }
    //}

    void CheckItems(IEnumerable? x)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (AdvertiseService.Current.IsInitialized && (x?.Count() is 0 or null))
            {
                this.IsVisible = false;
            }
            else if (AdvertiseService.Current.IsShowAdvertise)
            {
                this.IsVisible = true;
            }
        });
    }

    void RemoveAd()
    {
        Dispatcher.UIThread.Post(() =>
        {
            this.IsVisible = false;
        });
    }
}

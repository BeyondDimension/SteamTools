using System.Collections.Specialized;
using System.ComponentModel;

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

    AdvertiseService? advertisementService;
    ObservableCollection<AdvertisementDTO>? advertisements;
    bool closed;
    bool detachedCleanupQueued;

    public AdControl()
    {
        InitializeComponent();
        CloseAdBtn.IsVisible = SupportClose;
        AdBanner.AutoScroll = false;
        CloseAdBtn.Click += (_, _) =>
        {
            closed = true;
            RefreshAdvertisements();
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        advertisementService = AdvertiseService.Current;
        advertisementService.PropertyChanged += AdvertisementService_PropertyChanged;
        RefreshAdvertisements();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (advertisementService != null)
            advertisementService.PropertyChanged -= AdvertisementService_PropertyChanged;
        advertisementService = null;
        SetAdvertisements(null);
        AdBanner.AutoScroll = false;
        // Ancestors may still be traversing the carousel's generated visual children.
        if (!detachedCleanupQueued)
        {
            detachedCleanupQueued = true;
            Dispatcher.UIThread.Post(ReleaseDetachedAdvertisements, DispatcherPriority.Background);
        }
        base.OnDetachedFromVisualTree(e);
    }

    void ReleaseDetachedAdvertisements()
    {
        detachedCleanupQueued = false;
        // One queued cleanup serves repeated detaches, but must not affect a new attachment.
        if (advertisementService != null) return;
        AdBanner.ItemsSource = null;
        IsVisible = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StandardProperty)
            RefreshAdvertisements();
        else if (change.Property == SupportCloseProperty && CloseAdBtn != null)
            CloseAdBtn.IsVisible = SupportClose;
    }

    void AdvertisementService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) ||
            e.PropertyName is nameof(AdvertiseService.HorizontalBannerAdvertisements) or
                nameof(AdvertiseService.VerticalBannerAdvertisements) or nameof(AdvertiseService.IsShowAdvertise))
            ScheduleRefresh();
    }

    void Advertisements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ScheduleRefresh();

    void ScheduleRefresh()
    {
        if (Dispatcher.UIThread.CheckAccess())
            RefreshAdvertisements();
        else
            Dispatcher.UIThread.Post(RefreshAdvertisements);
    }

    void SetAdvertisements(ObservableCollection<AdvertisementDTO>? value)
    {
        if (ReferenceEquals(advertisements, value)) return;
        if (advertisements != null)
            advertisements.CollectionChanged -= Advertisements_CollectionChanged;
        advertisements = value;
        if (advertisements != null)
            advertisements.CollectionChanged += Advertisements_CollectionChanged;
    }

    void RefreshAdvertisements()
    {
        var service = advertisementService;
        if (service == null) return;
        SetAdvertisements(Standard == AdvertisementOrientation.Vertical
            ? service.VerticalBannerAdvertisements
            : service.HorizontalBannerAdvertisements);
        var visible = !closed && service.IsShowAdvertise && advertisements is { Count: > 0 };
        AdBanner.ItemsSource = visible ? advertisements : null;
        AdBanner.AutoScroll = visible;
        IsVisible = visible;
    }
}

using Avalonia.Controls.Notifications;
using static BD.WTTS.Program;

namespace BD.WTTS.UI.Views.Controls;

/// <summary>
/// An <see cref="INotificationManager"/> that displays notifications in a <see cref="Window"/>.
/// </summary>
[TemplatePart("PART_Items", typeof(Panel))]
[PseudoClasses(":topleft", ":topright", ":bottomleft", ":bottomright")]
public class SnackbarManager : TemplatedControl, IManagedNotificationManager
{
    private IList? _items;

    /// <summary>
    /// Defines the <see cref="Position"/> property.
    /// </summary>
    public static readonly StyledProperty<NotificationPosition> PositionProperty =
      AvaloniaProperty.Register<SnackbarManager, NotificationPosition>(nameof(Position), NotificationPosition.BottomRight);

    /// <summary>
    /// Defines which corner of the screen notifications can be displayed in.
    /// </summary>
    /// <seealso cref="NotificationPosition"/>
    public NotificationPosition Position
    {
        get { return GetValue(PositionProperty); }
        set { SetValue(PositionProperty, value); }
    }

    /// <summary>
    /// Defines the <see cref="MaxItems"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxItemsProperty =
      AvaloniaProperty.Register<SnackbarManager, int>(nameof(MaxItems), 5);

    /// <summary>
    /// Defines the maximum number of notifications visible at once.
    /// </summary>
    public int MaxItems
    {
        get { return GetValue(MaxItemsProperty); }
        set { SetValue(MaxItemsProperty, value); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnackbarManager"/> class.
    /// </summary>
    public SnackbarManager()
    {
        UpdatePseudoClasses(Position);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnackbarManager"/> class.
    /// </summary>
    /// <param name="host">The window that will host the control.</param>
    public SnackbarManager(TopLevel? host)
    {
        if (host != null)
        {
            Install(host);
        }

        UpdatePseudoClasses(Position);
    }

    static SnackbarManager()
    {
        HorizontalAlignmentProperty.OverrideDefaultValue<SnackbarManager>(HorizontalAlignment.Stretch);
        VerticalAlignmentProperty.OverrideDefaultValue<SnackbarManager>(VerticalAlignment.Stretch);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var itemsControl = e.NameScope.Find<Panel>("PART_Items");
        _items = itemsControl?.Children;
    }

    /// <inheritdoc/>
    public void Show(INotification content)
    {
        Show(content as object);
    }

    /// <inheritdoc/>
    public async void Show(object content)
    {
        var notification = content as INotification;

        var infoBarControl = new Snackbar
        {
            IsOpen = true,
            IsClosable = true,
            Severity = InfoBarSeverity.Informational,
        };

        if (notification != null)
        {
            infoBarControl.Title = notification.Title;
            infoBarControl.Message = notification.Message;
            infoBarControl.Severity = notification.Type switch
            {
                Avalonia.Controls.Notifications.NotificationType.Warning => InfoBarSeverity.Warning,
                Avalonia.Controls.Notifications.NotificationType.Success => InfoBarSeverity.Success,
                Avalonia.Controls.Notifications.NotificationType.Error => InfoBarSeverity.Error,
                _ => InfoBarSeverity.Informational,
            };
        }
        else
        {
            infoBarControl.Message = content.ToString();
        }

        infoBarControl.Closed += (sender, args) =>
        {
            notification?.OnClose?.Invoke();
            _items?.Remove(sender);
        };

        //infoBarControl.PointerPressed += (sender, args) =>
        //{
        //    if (notification != null && notification.OnClick != null)
        //    {
        //        notification.OnClick.Invoke();
        //    }

        //    //(sender as Snackbar)?.Close();
        //};

        _items?.Add(infoBarControl);

        if (_items?.OfType<Snackbar>().Count(i => !i.IsClosing) > MaxItems)
        {
            _items.OfType<Snackbar>().First(i => !i.IsClosing).Close();
        }

        if (notification != null && notification.Expiration == TimeSpan.Zero)
        {
            return;
        }

        await Task.Delay(notification?.Expiration ?? TimeSpan.FromSeconds(5));

        infoBarControl.Close();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PositionProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<NotificationPosition>());
        }
    }

    /// <summary>
    /// Installs the <see cref="WindowNotificationManager"/> within the <see cref="AdornerLayer"/>
    /// of the host <see cref="Window"/>.
    /// </summary>
    /// <param name="host">The <see cref="Window"/> that will be the host.</param>
    public void Install(TemplatedControl host)
    {
        var adornerLayer = host.FindDescendantOfType<VisualLayerManager>()?.AdornerLayer;

        if (adornerLayer is not null && !adornerLayer.Children.Contains(this))
        {
            adornerLayer.Children.Add(this);
            AdornerLayer.SetAdornedElement(this, adornerLayer);
        }
    }

    public void UpdatePseudoClasses(NotificationPosition position)
    {
        PseudoClasses.Set(":topleft", position == NotificationPosition.TopLeft);
        PseudoClasses.Set(":topright", position == NotificationPosition.TopRight);
        PseudoClasses.Set(":bottomleft", position == NotificationPosition.BottomLeft);
        PseudoClasses.Set(":bottomright", position == NotificationPosition.BottomRight);
    }
}

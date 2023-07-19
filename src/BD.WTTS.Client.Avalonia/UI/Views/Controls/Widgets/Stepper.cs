using Avalonia.Automation;
using Avalonia.Automation.Peers;
using FluentAvalonia.Core;
using System.CommandLine;
using MouseButton = Avalonia.Input.MouseButton;

namespace BD.WTTS.UI.Views.Controls;

[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter))]
public class Stepper : SelectingItemsControl
{
    private object? _selectedContent;
    private IDataTemplate? _selectedContentTemplate;
    private CompositeDisposable? _selectedItemSubscriptions;
    private Button? _previousButton;
    private Button? _skipButton;
    private Button? _nextButton;

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<Stepper, string>(nameof(Title));

    public static readonly StyledProperty<bool> ShowNavigationButtonsProperty =
        AvaloniaProperty.Register<Stepper, bool>(nameof(ShowNavigationButtons), true);

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<Stepper>();

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<Stepper>();

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<Stepper>();

    public static readonly DirectProperty<Stepper, object?> SelectedContentProperty =
        AvaloniaProperty.RegisterDirect<Stepper, object?>(nameof(SelectedContent), o => o.SelectedContent);

    public static readonly DirectProperty<Stepper, IDataTemplate?> SelectedContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<Stepper, IDataTemplate?>(nameof(SelectedContentTemplate), o => o.SelectedContentTemplate);

    public static readonly StyledProperty<bool> IsFinishProperty =
        AvaloniaProperty.Register<Stepper, bool>(nameof(IsFinish), false);

    public static readonly StyledProperty<string> SkipButtonNameProperty =
        AvaloniaProperty.Register<StepperItem, string>(nameof(SkipButtonName), "跳过");

    public static readonly StyledProperty<string> NextButtonNameProperty =
        AvaloniaProperty.Register<StepperItem, string>(nameof(NextButtonName), "继续");

    public static readonly StyledProperty<string> BackButtonNameProperty =
        AvaloniaProperty.Register<StepperItem, string>(nameof(BackButtonName), "返回");

    /// <summary>
    /// Occurs after the primary button has been tapped.
    /// </summary>
    public event TypedEventHandler<Stepper, CancelEventArgs>? Nexting;

    /// <summary>
    /// Occurs after the secondary button has been tapped.
    /// </summary>
    public event TypedEventHandler<Stepper, CancelEventArgs>? Skiping;

    /// <summary>
    /// Occurs after the close button has been tapped.
    /// </summary>
    public event TypedEventHandler<Stepper, CancelEventArgs>? Backing;

    static Stepper()
    {
        SelectionModeProperty.OverrideDefaultValue<Stepper>(SelectionMode.AlwaysSelected);
        //ItemsPanelProperty.OverrideDefaultValue<Stepper>(DefaultPanel);
        //AffectsMeasure<Stepper>(TabStripPlacementProperty);
        SelectedItemProperty.Changed.AddClassHandler<Stepper>((x, e) => x.UpdateSelectedContent());
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<Stepper>(AutomationControlType.Tab);
    }

    public string SkipButtonName
    {
        get => GetValue(SkipButtonNameProperty);
        set => SetValue(SkipButtonNameProperty, value);
    }

    public string BackButtonName
    {
        get => GetValue(BackButtonNameProperty);
        set => SetValue(BackButtonNameProperty, value);
    }

    public string NextButtonName
    {
        get => GetValue(NextButtonNameProperty);
        set => SetValue(NextButtonNameProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsFinish
    {
        get => GetValue(IsFinishProperty);
        set => SetValue(IsFinishProperty, value);
    }

    public bool ShowNavigationButtons
    {
        get => GetValue(ShowNavigationButtonsProperty);
        set => SetValue(ShowNavigationButtonsProperty, value);
    }

    public IDataTemplate? ContentTemplate
    {
        get { return GetValue(ContentTemplateProperty); }
        set { SetValue(ContentTemplateProperty, value); }
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get { return GetValue(HorizontalContentAlignmentProperty); }
        set { SetValue(HorizontalContentAlignmentProperty, value); }
    }

    public VerticalAlignment VerticalContentAlignment
    {
        get { return GetValue(VerticalContentAlignmentProperty); }
        set { SetValue(VerticalContentAlignmentProperty, value); }
    }

    public object? SelectedContent
    {
        get => _selectedContent;
        internal set => SetAndRaise(SelectedContentProperty, ref _selectedContent, value);
    }

    public IDataTemplate? SelectedContentTemplate
    {
        get => _selectedContentTemplate;
        internal set => SetAndRaise(SelectedContentTemplateProperty, ref _selectedContentTemplate, value);
    }

    internal ItemsPresenter? ItemsPresenterPart { get; private set; }

    internal ContentPresenter? ContentPart { get; private set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_previousButton != null)
            _previousButton.Click -= OnButtonClick;
        if (_skipButton != null)
            _skipButton.Click -= OnButtonClick;
        if (_nextButton != null)
            _nextButton.Click -= OnButtonClick;

        base.OnApplyTemplate(e);
        ItemsPresenterPart = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        ItemsPresenterPart?.ApplyTemplate();

        this.SelectedIndex = 0;

        _previousButton = e.NameScope.Find<Button>("PreviousButton");
        _skipButton = e.NameScope.Find<Button>("SkipButton");
        _nextButton = e.NameScope.Find<Button>("NextButton");
        
        if (_previousButton != null)
        {
            _previousButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, _previousButton));
            _previousButton.Click += OnButtonClick;
        }
        if (_skipButton != null)
        {
            _skipButton.Click += OnButtonClick;
        }
        if (_nextButton != null)
        {
            _nextButton.Click += OnButtonClick;
        }
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new StepperItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<StepperItem>(item, out recycleKey);
    }

    protected virtual bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name == "PART_SelectedContentHost")
        {
            ContentPart = presenter;
            return true;
        }

        return false;
    }

    protected override void PrepareContainerForItemOverride(Control element, object? item, int index)
    {
        base.PrepareContainerForItemOverride(element, item, index);

        if (index == SelectedIndex)
        {
            UpdateSelectedContent(element);
        }
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);

        var selectedIndex = SelectedIndex;

        if (selectedIndex == oldIndex || selectedIndex == newIndex)
            UpdateSelectedContent();
    }

    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);
        UpdateSelectedContent();
    }

    private void UpdateSelectedContent(Control? container = null)
    {
        _selectedItemSubscriptions?.Dispose();
        _selectedItemSubscriptions = null;

        if (SelectedIndex == -1)
        {
            SelectedContent = SelectedContentTemplate = null;
        }
        else
        {
            container ??= ContainerFromIndex(SelectedIndex);
            if (container != null)
            {
                _selectedItemSubscriptions = new CompositeDisposable(
                    container.GetObservable(ContentControl.ContentProperty).Subscribe(v => SelectedContent = v),
                    // Note how we fall back to our own ContentTemplate if the container doesn't specify one
                    container.GetObservable(ContentControl.ContentTemplateProperty).Subscribe(v => SelectedContentTemplate = v ?? ContentTemplate));
            }
        }
    }

    // protected override void OnPointerPressed(PointerPressedEventArgs e)
    // {
    //     base.OnPointerPressed(e);
    //
    //     if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.Pointer.Type == PointerType.Mouse)
    //     {
    //         e.Handled = UpdateSelectionFromEventSource(e.Source);
    //     }
    // }

    // protected override void OnPointerReleased(PointerReleasedEventArgs e)
    // {
    //     if (e.InitialPressMouseButton == MouseButton.Left && e.Pointer.Type != PointerType.Mouse)
    //     {
    //         var container = GetContainerFromEventSource(e.Source);
    //         if (container != null
    //             && container.GetVisualsAt(e.GetPosition(container))
    //                 .Any(c => container == c || container.IsVisualAncestorOf(c)))
    //         {
    //             e.Handled = UpdateSelectionFromEventSource(e.Source);
    //         }
    //     }
    // }

    protected virtual void OnNexting(CancelEventArgs args)
    {
        Nexting?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the secondary button is invoked
    /// </summary>
    protected virtual void OnSkiping(CancelEventArgs args)
    {
        Skiping?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the close button is invoked
    /// </summary>
    protected virtual void OnBacking(CancelEventArgs args)
    {
        Backing?.Invoke(this, args);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
        {
            if (change.NewValue is StepperItem stepperItem)
            {
                if (_skipButton != null)
                    _skipButton.IsVisible = stepperItem.IsSkip;
                if (_previousButton != null)
                    _previousButton.IsVisible = stepperItem.IsBack;
                if (_nextButton != null)
                    _nextButton.IsVisible = stepperItem.IsNext;
            }
        }
        else if (change.Property == ContentTemplateProperty)
        {
            var newTemplate = change.GetNewValue<IDataTemplate?>();
            if (SelectedContentTemplate != newTemplate &&
                ContainerFromIndex(SelectedIndex) is { } container &&
                container.GetValue(ContentControl.ContentTemplateProperty) == null)
            {
                SelectedContentTemplate = newTemplate; // See also UpdateSelectedContent
            }
        }
        else if (change.Property == KeyboardNavigation.TabOnceActiveElementProperty &&
            ItemsPresenterPart?.Panel is { } panel)
        {
            // Forward TabOnceActiveElement to the panel.
            KeyboardNavigation.SetTabOnceActiveElement(
                panel,
                change.GetNewValue<IInputElement?>());
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        var args = new CancelEventArgs();

        var deferral = new Deferral(() =>
        {
            Dispatcher.UIThread.VerifyAccess();

            if (args.Cancel)
                return;

            if (sender == _previousButton)
            {
                if (this.SelectedIndex > 0)
                {
                    this.SelectedIndex--;
                }
                IsFinish = false;
            }
            else if (sender == _skipButton)
            {
                if (this.SelectedIndex < ItemCount - 1)
                {
                    this.SelectedIndex++;
                }
            }
            else if (sender == _nextButton)
            {
                if (this.SelectedIndex < ItemCount - 1)
                {
                    this.SelectedIndex++;
                    if (this.SelectedIndex == ItemCount - 1)
                        _nextButton!.Content = "Finish";
                }
                else if (this.SelectedIndex == ItemCount - 1)
                {
                    if (IsFinish)
                    {
                        IsFinish = false;
                        this.SelectedIndex = 0;
                        _nextButton!.Content = "Next";
                    }
                    else
                    {
                        IsFinish = true;
                        _nextButton!.Content = "Reset";
                    }
                }
            }
        });

        if (sender == _previousButton)
        {
            OnBacking(args);
        }
        else if (sender == _skipButton)
        {
            OnSkiping(args);
        }
        else if (sender == _nextButton)
        {
            OnNexting(args);
        }

        deferral.Complete();
    }
}

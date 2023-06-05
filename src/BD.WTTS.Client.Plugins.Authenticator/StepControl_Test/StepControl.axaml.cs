using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Button = Avalonia.Controls.Button;

namespace BD.WTTS.StepControls_Test;

public class StepControl : TemplatedControl, IStyleable
{
    private Button? _previousButton;
    private Button? _nextButton;
    private ContentPresenter? _contentPresenter;
    private int _currentStepIndex;

    public static readonly StyledProperty<bool> IsFirstStepProperty =
        AvaloniaProperty.Register<StepControl, bool>(nameof(IsFirstStep));

    public static readonly StyledProperty<bool> IsLastStepProperty =
        AvaloniaProperty.Register<StepControl, bool>(nameof(IsLastStep));

    public static readonly StyledProperty<string> PreviousButtonContentProperty =
        AvaloniaProperty.Register<StepControl, string>(nameof(PreviousButtonContent));

    public static readonly StyledProperty<string> NextButtonContentProperty =
        AvaloniaProperty.Register<StepControl, string>(nameof(NextButtonContent));

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<StepControl, string>(nameof(Title));

    public static readonly StyledProperty<bool> ShowNavigationButtonsProperty =
        AvaloniaProperty.Register<StepControl, bool>(nameof(ShowNavigationButtons), true);

    public static readonly StyledProperty<bool> ShowTitleProperty =
        AvaloniaProperty.Register<StepControl, bool>(nameof(ShowTitle), true);

    public static readonly DirectProperty<StepControl, ObservableCollection<StepItem>> StepItemsProperty =
        AvaloniaProperty.RegisterDirect<StepControl, ObservableCollection<StepItem>>(nameof(StepItems), o => o.StepItems);

    private ObservableCollection<StepItem> _stepItems = new();

    public int CurrentStepIndex => _currentStepIndex;

    public bool IsFirstStep
    {
        get => GetValue(IsFirstStepProperty);
        set => SetValue(IsFirstStepProperty, value);
    }

    public bool IsLastStep
    {
        get => GetValue(IsLastStepProperty);
        set => SetValue(IsLastStepProperty, value);
    }

    public string PreviousButtonContent
    {
        get => GetValue(PreviousButtonContentProperty);
        set => SetValue(PreviousButtonContentProperty, value);
    }

    public string NextButtonContent
    {
        get => GetValue(NextButtonContentProperty);
        set => SetValue(NextButtonContentProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowNavigationButtons
    {
        get => GetValue(ShowNavigationButtonsProperty);
        set => SetValue(ShowNavigationButtonsProperty, value);
    }

    public bool ShowTitle
    {
        get => GetValue(ShowTitleProperty);
        set => SetValue(ShowTitleProperty, value);
    }

    public ObservableCollection<StepItem> StepItems
    {
        get => _stepItems;
        set => SetAndRaise(StepItemsProperty, ref _stepItems, value);
    }

    public StepControl()
    {
        _currentStepIndex = 0;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");

        _previousButton.Click += PreviousButton_Click;
        _nextButton.Click += NextButton_Click;

        ShowCurrentStep();
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStepIndex > 0)
        {
            _currentStepIndex--;
            ShowCurrentStep();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStepIndex < _stepItems.Count - 1)
        {
            _currentStepIndex++;
            ShowCurrentStep();
        }
    }

    public void AddStep(string title, Control content)
    {
        var stepItem = new StepItem(title, content);
        _stepItems.Add(stepItem);
    }

    private void ShowCurrentStep()
    {
        if (_contentPresenter != null && _stepItems.Count > 0 && _currentStepIndex >= 0 && _currentStepIndex < _stepItems.Count)
        {
            UpdateStepIndicators();

            var step = _stepItems[_currentStepIndex];
            _contentPresenter.Content = step.Content;

            IsFirstStep = (_currentStepIndex == 0);
            IsLastStep = (_currentStepIndex == _stepItems.Count - 1);

            Title = step.Title;
        }
    }

    private void UpdateStepIndicators()
    {
        foreach (var step in _stepItems)
        {
            step.IsCurrent = false;
        }

        if (_currentStepIndex >= 0 && _currentStepIndex < _stepItems.Count)
        {
            _stepItems[_currentStepIndex].IsCurrent = true;
        }
    }

    Type IStyleable.StyleKey => typeof(StepControl);
}

public class StepItem : INotifyPropertyChanged
{
    public string Title { get; }

    public Control Content { get; }

    // private bool _canNext;
    //
    // public bool CanNext
    // {
    //     get => _canNext;
    //     set
    //     {
    //         if (_canNext != value)
    //         {
    //             _canNext = value;
    //             OnPropertyChanged();
    //         }
    //     }
    // }

    private bool _isCurrent;

    public bool IsCurrent
    {
        get => _isCurrent;
        set
        {
            if (_isCurrent != value)
            {
                _isCurrent = value;
                OnPropertyChanged();
            }
        }
    }

    public StepItem(string title, Control content)
    {
        Title = title;
        Content = content;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

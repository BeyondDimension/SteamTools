namespace BD.WTTS.UI.Views.Controls;

public sealed class TaskDialogEx : TaskDialog
{
    private Button? _closeButton;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_closeButton != null)
        {
            _closeButton.IsVisible = !Buttons.Any_Nullable();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_closeButton != null)
        {
            _closeButton.Click -= CloseButtonClick;
        }

        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("CloseButton");

        if (_closeButton != null)
        {
            _closeButton.Click += CloseButtonClick;
        }
    }

    private void CloseButtonClick(object? sender, RoutedEventArgs e)
    {
        Hide(TaskDialogStandardResult.Close);
    }
}

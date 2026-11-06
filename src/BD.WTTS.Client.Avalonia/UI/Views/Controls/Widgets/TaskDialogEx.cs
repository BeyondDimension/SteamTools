using Avalonia.Input;
using Avalonia.VisualTree;

namespace BD.WTTS.UI.Views.Controls;

public sealed class TaskDialogEx : TaskDialog
{
    private Button? _closeButton;
    private Panel? _layoutRoot;

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

        if (_layoutRoot != null)
        {
            _layoutRoot.PointerPressed -= LayoutRootPointerPressed;
        }

        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("CloseButton");
        _layoutRoot = e.NameScope.Find<Panel>("LayoutRoot");

        if (_layoutRoot != null)
        {
            _layoutRoot.PointerPressed += LayoutRootPointerPressed;
        }

        if (_closeButton != null)
        {
            _closeButton.Click += CloseButtonClick;
        }
    }

    private void LayoutRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Only the modal backdrop can initiate a move; dialog content must retain its input.
        if (e.Source != _layoutRoot ||
            e.GetCurrentPoint(_layoutRoot).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed ||
            TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        foreach (var titleBar in window.GetVisualDescendants().OfType<TitleBar>())
        {
            var region = titleBar.WindowDragRegion;
            if (region.IsEffectivelyVisible && new Rect(region.Bounds.Size).Contains(e.GetPosition(region)))
            {
                window.BeginMoveDrag(e);
                e.Handled = true;
                return;
            }
        }
    }

    private void CloseButtonClick(object? sender, RoutedEventArgs e)
    {
        Hide(TaskDialogStandardResult.Close);
    }
}

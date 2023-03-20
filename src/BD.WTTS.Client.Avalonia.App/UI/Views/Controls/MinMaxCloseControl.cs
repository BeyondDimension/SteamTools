using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.Views.Controls;

public class MinMaxCloseControl : TemplatedControl
{
    private IDisposable? _windowStateObservable;
    private CoreWindow? _owner;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;

    public MinMaxCloseControl()
    {
        KeyboardNavigation.SetIsTabStop(this, false);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_minimizeButton != null)
            _minimizeButton.Click -= OnButtonClick;

        if (_maximizeButton != null)
            _maximizeButton.Click -= OnButtonClick;

        if (_closeButton != null)
            _closeButton.Click -= OnButtonClick;

        base.OnApplyTemplate(e);

        _minimizeButton = e.NameScope.Get<Button>("MinimizeButton");
        _minimizeButton.Click += OnButtonClick;

        _maximizeButton = e.NameScope.Get<Button>("MaxRestoreButton");
        _maximizeButton.Click += OnButtonClick;

        _closeButton = e.NameScope.Get<Button>("CloseButton");
        _closeButton.Click += OnButtonClick;

        _owner = TemplatedParent as CoreWindow;

        if (_owner == null)
        {
            throw new InvalidOperationException("不应该在非 CoreWindow 中使用此控件");
        }

        _owner.Opened += OwnerWindowOpened;

        if (_owner.ShowAsDialog)
        {
            _minimizeButton.IsVisible = false;
            _maximizeButton.IsVisible = false;
        }
    }

    void OwnerWindowOpened(object? sender, EventArgs args)
    {
        _windowStateObservable = new CompositeDisposable(
            _owner!.GetObservable(Window.WindowStateProperty).Subscribe(OnWindowStateChanged),
            _owner!.GetObservable(WindowBase.IsActiveProperty).Subscribe(OnWindowActiveChanged));
    }

    void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (this == null)
            return;

        if (sender == _minimizeButton)
        {
            _owner!.WindowState = WindowState.Minimized;
        }
        else if (sender == _maximizeButton)
        {
            if (_owner!.WindowState == WindowState.Maximized)
            {
                _owner!.WindowState = WindowState.Normal;
            }
            else if (_owner!.WindowState == WindowState.Normal)
            {
                _owner!.WindowState = WindowState.Maximized;
            }
        }
        else if (sender == _closeButton)
        {
            _owner!.Close();
        }
    }

    void OnWindowStateChanged(WindowState state)
    {
        PseudoClasses.Set(":maximized", state == WindowState.Maximized);
        PseudoClasses.Set(":fullscreen", state == WindowState.FullScreen);
    }

    void OnWindowActiveChanged(bool active)
    {
        if (_minimizeButton != null) ((IPseudoClasses)_minimizeButton.Classes).Set(":inactive", !active);
        if (_maximizeButton != null) ((IPseudoClasses)_maximizeButton.Classes).Set(":inactive", !active);
        if (_closeButton != null) ((IPseudoClasses)_closeButton.Classes).Set(":inactive", !active);
    }
}

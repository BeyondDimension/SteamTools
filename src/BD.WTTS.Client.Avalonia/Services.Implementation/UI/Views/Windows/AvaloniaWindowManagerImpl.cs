using BD.WTTS.Client.Resources;
using System.Collections.Immutable;

namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IWindowManager"/>
public sealed class AvaloniaWindowManagerImpl : IWindowManagerImpl
{
    Type? IWindowManagerImpl.WindowType => typeof(Window);

    public static TopLevel? GetWindowTopLevel()
    {
        if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            if (window != null)
            {
                return TopLevel.GetTopLevel(window);
            }
        }
        else if (App.Instance.ApplicationLifetime is ISingleViewApplicationLifetime view)
        {
            var mainView = view.MainView;
            if (mainView != null)
            {
                return TopLevel.GetTopLevel(mainView);
            }
        }
        return null;
    }

    Type GetWindowType(AppEndPoint customWindow)
    {
        IWindowManagerImpl @this = this;
        return @this.GetWindowType(customWindow, typeof(AvaloniaWindowManagerImpl).Assembly);
    }

    Type GetWindowViewModelType(AppEndPoint customWindow)
    {
        IWindowManagerImpl @this = this;
        return @this.GetWindowViewModelType(customWindow, typeof(WindowViewModel).Assembly);
    }

    static bool IsSingletonWindow(AppEndPoint customWindow) => customWindow switch
    {
        AppEndPoint.MessageBox or
        AppEndPoint.TextBox or
        AppEndPoint.AuthTrade or
        AppEndPoint.ShowAuth => false,
        _ => true,
    };

    static bool TryShowSingletonWindow(Type windowType)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.Windows.FirstOrDefault(x => x.GetType() == windowType);
            if (window != null)
            {
                window.Show();
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;
                window.Activate();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 显示一个页内弹窗
    /// </summary>
    /// <typeparam name="TPageViewModel"></typeparam>
    /// <param name="viewModel"></param>
    /// <param name="title"></param>
    /// <param name="subHeader"></param>
    /// <param name="isDialog"></param>
    /// <param name="isFooterExpanded"></param>
    /// <returns></returns>
    public async Task<bool> ShowTaskDialogAsync<TPageViewModel>(
        TPageViewModel? viewModel,
        string title = "",
        string? header = null,
        string? subHeader = null,
        bool isDialog = false,
        bool showProgressBar = false,
        bool isRememberChooseFooter = false,
        bool isOkButton = true,
        bool isCancelButton = false,
        bool isRetryButton = false,
        object? pageContent = null,
        string? okButtonText = null,
        string? retryButtonText = null,
        string? moreInfoText = null,
        Func<bool>? cancelCloseAction = null,
        bool disableScroll = false)
        where TPageViewModel : ViewModelBase
  {
        var td = new TaskDialogEx
        {
            Title = title,
            Header = string.IsNullOrEmpty(header) ? title : header,
            SubHeader = subHeader,
            //DataContext = viewModel,
            //Content = GetPageContent(viewModel),
            //IconSource = new SymbolIconSource { Symbol = Symbol.Accept },
            ShowProgressBar = showProgressBar,
            FooterVisibility = TaskDialogFooterVisibility.Never,
            //IsFooterExpanded = isFooterExpanded,
            //Footer = new CheckBox { Content = Strings.RememberChooseNotToAskAgain, },
            XamlRoot = GetWindowTopLevel(),
        };

        if (td.XamlRoot == null)
        {
            Toast.LogAndShowT(new Exception("在 AppWindow 为 Null 时，无法弹出 Taskdialog."));
            return false;
        }

        if (viewModel != null)
        {
            if (viewModel is IWindowViewModel window)
            {
                window.Close = b => td?.Hide(b ? TaskDialogStandardResult.OK : TaskDialogStandardResult.Close);
            }

            td.DataContext = viewModel;
            td.Content = pageContent ?? INavigationService.Instance.GetViewModelToPageContent(viewModel);
        }

        if (!string.IsNullOrEmpty(moreInfoText))
        {
            td.FooterVisibility = TaskDialogFooterVisibility.Auto;
            td.Footer = new TextBlock { Text = moreInfoText, TextWrapping = TextWrapping.Wrap };
        }
        else if (isRememberChooseFooter)
        {
            td.FooterVisibility = TaskDialogFooterVisibility.Always;
            td.Footer = new CheckBox { Content = Strings.RememberChooseNotToAskAgain, };
        }

        if (isRetryButton)
        {
            td.Buttons.Add(new TaskDialogButton(string.IsNullOrEmpty(retryButtonText) ? Strings.Retry : retryButtonText, TaskDialogStandardResult.Retry));
        }
        if (isOkButton)
        {
            td.Buttons.Add(new TaskDialogButton(string.IsNullOrEmpty(okButtonText) ? Strings.Confirm : okButtonText, TaskDialogStandardResult.OK));
        }
        if (isCancelButton)
        {
            td.Buttons.Add(new TaskDialogButton(Strings.Cancel, TaskDialogStandardResult.Cancel));
        }
        if (disableScroll)
        {
            td.Classes.Add("disableScroll");
        }
        //td.DataTemplates.Add(new FuncDataTemplate<DebugPageViewModel>((x, _) => new DebugPage(), true));

        object? result = null;
        void OnClosing(TaskDialog s, TaskDialogClosingEventArgs args)
        {
            result = args.Result;
            if (result is not TaskDialogStandardResult.Cancel or TaskDialogStandardResult.No)
            {
                args.Cancel = cancelCloseAction?.Invoke() ?? false;
            }
            if (viewModel is IWindowViewModel window)
            {
                var e = new CancelEventArgs(args.Cancel);
                window.OnClosing(window, e);
                args.Cancel = e.Cancel;
            }
        }
        try
        {
            td.Closing += OnClosing;
            result = await MainThread2.InvokeOnMainThreadAsync(async () =>
            {
                var result = await td.ShowAsync(!isDialog);
                return result;
            });
        }
        catch (NullReferenceException)
        {
            // 在插件中调用 TaskDialog.ShowAsync(bool showHosted = false) 会抛出此异常
            // 在 FluentAvalonia.UI.Controls.TaskDialog.<ShowAsync>d__6.MoveNext()
            // 在 AvaloniaWindowManagerImpl.<>c__DisplayClass8_0`1.<<ShowTaskDialogAsync>b__1>d.MoveNext()

            // Visual visual = XamlRoot ?? (base.VisualRoot as Visual);
            // _previousFocus = TopLevel.GetTopLevel(visual)!.FocusManager!.GetFocusedElement(); is null!
            // _previousFocus.Focus(); !!!!!!!!!! NullReferenceException !!!!!!!!!!
        }
        td.Closing -= OnClosing;
        td = null;

        return result is TaskDialogStandardResult.OK;
    }

    Task ShowAsync(Type typeWindowViewModel,
        bool isDialog,
        AppEndPoint customWindow,
        string title,
        WindowViewModel? viewModel,
        ResizeMode resizeMode = default,
        bool isParent = true,
        Action<DialogWindowViewModel>? actionDialogWindowViewModel = null)
        => MainThread2.InvokeOnMainThreadAsync(async () =>
        {
            var windowType = GetWindowType(customWindow);
            if (IsSingletonWindow(customWindow) && TryShowSingletonWindow(windowType))
            {
                return;
            }
            var window = (Window?)Activator.CreateInstance(windowType);
            if (window == null) return;

            if (resizeMode != default)
                window.SetResizeMode(resizeMode);

            if (viewModel == null && typeWindowViewModel != typeof(object))
            {
                viewModel = (WindowViewModel?)Activator.CreateInstance(typeWindowViewModel);
            }
            if (!string.IsNullOrEmpty(title) && viewModel != null)
            {
                viewModel.Title = title;
            }
            if (isParent)
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (typeof(DialogWindowViewModel).IsAssignableFrom(typeWindowViewModel))
            {
                static void BindingDialogWindowViewModel(Window window, DialogWindowViewModel dialogWindowViewModel, Action<DialogWindowViewModel>? actionDialogWindowViewModel)
                {
                    actionDialogWindowViewModel?.Invoke(dialogWindowViewModel);
                    dialogWindowViewModel.OK ??= ReactiveCommand.Create(() =>
                        {
                            dialogWindowViewModel.DialogResult = true;
                            if (dialogWindowViewModel.OnOKClickCanClose())
                            {
                                window.Close();
                            }
                        });
                    dialogWindowViewModel.Cancel ??= ReactiveCommand.Create(() =>
                    {
                        dialogWindowViewModel.DialogResult = false;
                        window.Close();
                    });
                }
                if (viewModel == null)
                {
                    void Window_DataContextChanged(object? _, EventArgs __)
                    {
                        if (window.DataContext is DialogWindowViewModel dialogWindowViewModel)
                        {
                            BindingDialogWindowViewModel(window, dialogWindowViewModel, actionDialogWindowViewModel);
                        }
                    }
                    void Window_Closed(object? _, EventArgs __)
                    {
                        window.DataContextChanged -= Window_DataContextChanged;
                        window.Closed -= Window_Closed;
                    }
                    window.DataContextChanged += Window_DataContextChanged;
                    window.Closed += Window_Closed;
                }
                else if (viewModel is DialogWindowViewModel dialogWindowViewModel)
                {
                    BindingDialogWindowViewModel(window, dialogWindowViewModel, actionDialogWindowViewModel);
                }
            }
            if (viewModel != null) window.DataContext = viewModel;
            try
            {
                if (isDialog)
                {
                    await App.Instance.ShowDialogWindowAsync(window);
                }
                else
                {
                    if (isParent)
                        App.Instance.ShowWindow(window);
                    else
                        App.Instance.ShowWindowNoParent(window);
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(AvaloniaWindowManagerImpl), e,
                    "Show fail, windowType: {0}", window?.GetType().Name);
            }
        });

    public Task ShowAsync<TWindowViewModel>(
        AppEndPoint customWindow,
        TWindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = false,
        bool isParent = true)
        where TWindowViewModel : WindowViewModel, new() => ShowAsync(typeof(TWindowViewModel),
            customWindow, viewModel, title, resizeMode, isDialog, isParent);

    public Task ShowAsync(Type typeWindowViewModel,
        AppEndPoint customWindow,
        WindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = false,
        bool isParent = true) => ShowAsync(typeWindowViewModel, isDialog, customWindow,
            title, viewModel, resizeMode, isParent);

    public Task ShowAsync(AppEndPoint customWindow,
       WindowViewModel? viewModel = null,
       string title = "",
       ResizeMode resizeMode = ResizeMode.NoResize,
       bool isDialog = false,
       bool isParent = true) => ShowAsync(GetWindowViewModelType(customWindow), isDialog, customWindow,
            title, viewModel, resizeMode, isParent);

    public async Task<bool> ShowDialogAsync<TWindowViewModel>(
        AppEndPoint customWindow,
        TWindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = true,
        bool isParent = true)
        where TWindowViewModel : WindowViewModel, new()
    {
        DialogWindowViewModel? dialogWindowViewModel = null;
        await ShowAsync(typeof(TWindowViewModel), isDialog, customWindow,
            title, viewModel, resizeMode, isParent, dwvm =>
            {
                dialogWindowViewModel = dwvm;
            });
        return dialogWindowViewModel?.DialogResult ?? false;
    }

    public Task ShowDialogAsync(Type typeWindowViewModel,
            AppEndPoint customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = true) => ShowAsync(typeWindowViewModel, isDialog, customWindow,
                title, viewModel, resizeMode);

    public Task ShowDialogAsync(AppEndPoint customWindow,
        WindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = true) => ShowAsync(GetWindowViewModelType(customWindow), isDialog, customWindow,
            title, viewModel, resizeMode);

    public void CloseWindow(WindowViewModel vm)
    {
        try
        {
            if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var e = new CancelEventArgs();
                vm.OnClosing(vm, e);
                if (!e.Cancel)
                {
                    var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                    window?.Close();
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(nameof(AvaloniaWindowManagerImpl), e,
                "CloseWindow fail, vmType: {0}", vm.GetType().Name);
        }
    }

    public bool IsVisibleWindow(WindowViewModel vm)
    {
        try
        {
            if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                return window?.IsVisible == true;
            }
            return false;
        }
        catch (Exception e)
        {
            Log.Error(nameof(AvaloniaWindowManagerImpl), e,
                "HideWindow fail, vmType: {0}", vm.GetType().Name);
            return false;
        }
    }

    public void HideWindow(WindowViewModel vm)
    {
        try
        {
            if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                window?.Hide();
            }
        }
        catch (Exception e)
        {
            Log.Error(nameof(AvaloniaWindowManagerImpl), e,
                "HideWindow fail, vmType: {0}", vm.GetType().Name);
        }
    }

    public void ShowWindow(WindowViewModel vm)
    {
        try
        {
            if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                window?.Show();
            }
        }
        catch (Exception e)
        {
            Log.Error(nameof(AvaloniaWindowManagerImpl), e,
                "ShowWindowNoParent fail, vmType: {0}", vm.GetType().Name);
        }
    }
}
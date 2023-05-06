using BD.WTTS.Client.Resources;

namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IWindowManager"/>
sealed class AvaloniaWindowManagerImpl : IWindowManagerImpl
{
    Type? IWindowManagerImpl.WindowType => typeof(Window);

    object? GetPageContent(ViewModelBase viewModel)
    {
        return viewModel switch
        {
            DebugPageViewModel => new DebugPage(),
            TextBoxWindowViewModel => new TextInputDialogPage(),
            MessageBoxWindowViewModel => new MessageBoxPage(),
            _ => null,
        };
    }

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
    public Task<bool> ShowTaskDialogAsync<TPageViewModel>(
        TPageViewModel viewModel,
        string title = "",
        string header = "",
        string subHeader = "",
        bool isDialog = false,
        bool showProgressBar = false,
        bool isRememberChooseFooter = false,
        bool isCancelButton = false,
        bool isRetryButton = false)
        where TPageViewModel : ViewModelBase, new()
        => MainThread2.InvokeOnMainThreadAsync<bool>(async () =>
        {
            var td = new TaskDialog
            {
                Title = title,
                Header = string.IsNullOrEmpty(header) ? title : header,
                SubHeader = subHeader,
                DataContext = viewModel,
                Content = GetPageContent(viewModel),
                //IconSource = new SymbolIconSource { Symbol = Symbol.Accept },
                ShowProgressBar = showProgressBar,
                FooterVisibility = TaskDialogFooterVisibility.Never,
                //IsFooterExpanded = isFooterExpanded,
                //Footer = new CheckBox { Content = Strings.RememberChooseNotToAskAgain, },
                XamlRoot = GetWindowTopLevel(),
                Buttons =
                {
                    new TaskDialogButton(Strings.Confirm, TaskDialogStandardResult.OK),
                }
            };

            if (isRememberChooseFooter)
            {
                td.FooterVisibility = TaskDialogFooterVisibility.Always;
                td.Footer = new CheckBox { Content = Strings.RememberChooseNotToAskAgain, };
            }

            if (isCancelButton)
            {
                td.Buttons.Add(new TaskDialogButton(Strings.Cancel, TaskDialogStandardResult.Cancel));
            }
            if (isRetryButton)
            {
                td.Buttons.Add(new TaskDialogButton(Strings.Retry, TaskDialogStandardResult.Retry));
            }

            //td.DataTemplates.Add(new FuncDataTemplate<DebugPageViewModel>((x, _) => new DebugPage(), true));

            var result = await td.ShowAsync(!isDialog);
            return result is TaskDialogStandardResult.OK;
        });

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
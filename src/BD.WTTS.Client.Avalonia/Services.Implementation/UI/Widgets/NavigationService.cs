using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using System.Xml.Linq;

namespace BD.WTTS.Services.Implementation;

public sealed class NavigationService : INavigationService
{
    public static NavigationService Instance { get; } = (NavigationService)Ioc.Get<INavigationService>();

    //public Type? PreviousPage { get; set; }

    public Type? CurrnetPage => _frame?.CurrentSourcePageType;

    public object? GetViewModelToPageContent(object viewModel, bool isCreateInstance = true)
    {
        string name;

        if (viewModel is Type t)
        {
            name = t.Name;
        }
        else
        {
            name = viewModel.GetType().Name;
        }

        var type = name switch
        {
            nameof(DebugPageViewModel) => typeof(DebugPage),
            nameof(TextBoxWindowViewModel) => typeof(TextInputDialogPage),
            nameof(MessageBoxWindowViewModel) => typeof(MessageBoxPage),
            nameof(LoginOrRegisterWindowViewModel) => typeof(LoginOrRegisterPage),
            _ => null,
        };

        if (isCreateInstance && type != null)
        {
            return Activator.CreateInstance(type);
        }

        return type;
    }

    public void SetFrame(Frame f)
    {
        if (_frame != null)
        {
            _frame.NavigationFailed -= OnNavigationFailed;
            _frame.Navigated -= OnFrameNavigated;
        }
        _frame = f;
        _frame.NavigationFailed += OnNavigationFailed;
        _frame.Navigated += OnFrameNavigated;

        if (savedNavigation is { } state)
        {
            savedNavigation = null;
            if (state.CurrentPage != null)
                _frame.NavigateToType(state.CurrentPage, state.CurrentParameter!, new FrameNavigationOptions
                {
                    IsNavigationStackEnabled = false,
                    TransitionInfoOverride = new SuppressNavigationTransitionInfo(),
                });
            foreach (var item in state.BackStack)
                _frame.BackStack.Add(item.ToEntry());
            foreach (var item in state.ForwardStack)
                _frame.ForwardStack.Add(item.ToEntry());
        }

        if (pendingNavigation is { } pending)
        {
            pendingNavigation = null;
            Navigate(pending.Page, pending.Effect, pending.UseCache);
        }
    }

    void OnFrameNavigated(object? sender, FANavigationEventArgs e)
    {
        currentParameter = e.Parameter;
        currentHasContext = e.Content is Control page && e.SourcePageType != page.GetType();
    }

    internal MainWindowViewState? CaptureFrameState()
    {
        if (_frame == null)
            return new MainWindowViewState();
        if (currentHasContext || !CanSaveParameter(currentParameter) ||
            _frame.BackStack.Any(x => x.Context != null || !CanSaveParameter(x.Parameter)) ||
            _frame.ForwardStack.Any(x => x.Context != null || !CanSaveParameter(x.Parameter)))
            return null;

        return new MainWindowViewState
        {
            CurrentPage = _frame.CurrentSourcePageType,
            CurrentParameter = currentParameter,
            BackStack = _frame.BackStack.Select(x => new MainWindowViewState.NavigationEntry(x.SourcePageType, x.Parameter)).ToArray(),
            ForwardStack = _frame.ForwardStack.Select(x => new MainWindowViewState.NavigationEntry(x.SourcePageType, x.Parameter)).ToArray(),
        };
    }

    static bool CanSaveParameter(object? value) => value == null || value is string or Guid ||
        value.GetType().IsPrimitive || value.GetType().IsEnum;

    internal void SaveFrameState(Frame frame, MainWindowViewState state)
    {
        if (!ReferenceEquals(_frame, frame))
            return;
        savedNavigation = state;
        ClearFrame(frame);
    }

    internal void ClearFrame(Frame frame)
    {
        if (!ReferenceEquals(_frame, frame))
            return;
        _frame.NavigationFailed -= OnNavigationFailed;
        _frame.Navigated -= OnFrameNavigated;
        _frame = null;
        _overlayHost = null;
    }

    static void OnNavigationFailed(object? sender, FluentAvalonia.UI.Navigation.FANavigationFailedEventArgs e)
    {
        Log.Error(nameof(NavigationService), e.Exception, "Page navigation failed.");
#if DEBUG
        Console.Error.WriteLine(e.Exception);
#endif
    }

    public void SetOverlayHost(Panel p)
    {
        _overlayHost = p;
    }

    public void Navigate(Type? t, NavigationTransitionEffect effect = NavigationTransitionEffect.None, bool useCache = true)
    {
        if (_frame == null)
        {
            pendingNavigation = (t, effect, useCache);
            return;
        }

        var navOptions = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = useCache,
            TransitionInfoOverride = GetNavigationTransitionInfo(effect)
        };

        if (t == null)
        {
            navOptions.IsNavigationStackEnabled = false;
            _frame.NavigateToType(typeof(ErrorPage), null, navOptions);
            return;
        }

        if (t.IsSubclassOf(typeof(ViewModelBase)))
        {
            var pageType = GetViewModelToPageContent(t, false);
            t = pageType as Type;
        }

        if (t == null)
        {
            navOptions.IsNavigationStackEnabled = false;
            _frame.NavigateToType(typeof(ErrorPage), null, navOptions);
            return;
        }

        if (_frame.Content?.GetType() != t)
        {
            _frame.NavigateToType(t, null, navOptions);
            //if (!useCache && navOptions.IsNavigationStackEnabled)
            //{
            //    _frame.BackStack.Remove(_frame.BackStack.Last());
            //}
        }
    }

    public void GoBack(Type? t = null)
    {
        if (t == null)
        {
            _frame?.GoBack();
            return;
        }

        if (t.IsSubclassOf(typeof(ViewModelBase)))
        {
            var pageType = GetViewModelToPageContent(t, false);
            t = pageType as Type;
        }

        if (t == null)
        {
            _frame?.GoBack();
            return;
        }

        if (_frame?.CurrentSourcePageType == t)
        {
            _frame?.GoBack();
        }
    }

    public void NavigateFromContext(object dataContext, NavigationTransitionEffect effect = NavigationTransitionEffect.None)
    {
        if ((_frame?.Content as Control)?.DataContext != dataContext)
        {
            _frame?.NavigateFromObject(dataContext,
            new FluentAvalonia.UI.Navigation.FAFrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = GetNavigationTransitionInfo(effect)
            });
        }
    }

    public void ShowControlDefinitionOverlay(Type targetType)
    {
        if (_overlayHost != null)
        {

        }
    }

    public void ClearOverlay()
    {
        _overlayHost?.Children.Clear();
    }

    private static NavigationTransitionInfo GetNavigationTransitionInfo(NavigationTransitionEffect effect)
    {
        NavigationTransitionInfo transitionInfo = effect switch
        {
            NavigationTransitionEffect.FromLeft => new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromLeft,
            },
            NavigationTransitionEffect.FromRight => new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromRight,
            },
            NavigationTransitionEffect.FromTop => new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromTop,
            },
            NavigationTransitionEffect.FromBottom => new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromBottom,
            },
            NavigationTransitionEffect.DrillIn => new DrillInNavigationTransitionInfo(),
            NavigationTransitionEffect.Entrance => new EntranceNavigationTransitionInfo(),
            _ => new SuppressNavigationTransitionInfo(),
        };

        return transitionInfo;
    }

    private Frame? _frame;
    private Panel? _overlayHost;
    private MainWindowViewState? savedNavigation;
    private (Type? Page, NavigationTransitionEffect Effect, bool UseCache)? pendingNavigation;
    private object? currentParameter;
    private bool currentHasContext;
}



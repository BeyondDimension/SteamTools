using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace BD.WTTS.Services.Implementation;

public sealed class NavigationService : INavigationService
{
    public static NavigationService Instance { get; } = (NavigationService)Ioc.Get<INavigationService>();

    //public Type? PreviousPage { get; set; }

    public Type? CurrnetPage => _frame?.CurrentSourcePageType;

    public object? GetViewModelToPageContent(object viewModel, bool isCreateInstance = true)
    {
        var pageType = ResolvePageType(viewModel as Type ?? viewModel.GetType());
        return isCreateInstance && pageType is not null ? Activator.CreateInstance(pageType) : pageType;
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    private static Type? ResolvePageType(Type viewModelType)
    {
        if (viewModelType == typeof(DebugPageViewModel)) return typeof(DebugPage);
        if (viewModelType == typeof(TextBoxWindowViewModel)) return typeof(TextInputDialogPage);
        if (viewModelType == typeof(MessageBoxWindowViewModel)) return typeof(MessageBoxPage);
        if (viewModelType == typeof(LoginOrRegisterWindowViewModel)) return typeof(LoginOrRegisterPage);
        return null;
    }

    public void SetFrame(Frame f)
    {
        if (_frame != null)
        {
            _frame.NavigationFailed -= OnNavigationFailed;
        }
        _frame = f;
        _frame.NavigationFailed += OnNavigationFailed;

        if (pendingNavigation is { } pending)
        {
            pendingNavigation = null;
            Navigate(pending.Page, pending.Effect, pending.UseCache);
        }
    }

    internal void ResetFrame()
    {
        if (_frame != null)
            _frame.NavigationFailed -= OnNavigationFailed;
        _frame = null;
        // Detach may be traversing the host's children; release ownership without changing its tree.
        _overlayHost = null;
        pendingNavigation = null;
    }

    internal void DetachFrame(Frame frame)
    {
        if (ReferenceEquals(_frame, frame))
            ResetFrame();
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

    public void Navigate([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type? t, NavigationTransitionEffect effect = NavigationTransitionEffect.None, bool useCache = true)
    {
        if (_frame == null)
        {
            pendingNavigation = new PendingNavigation(t, effect, useCache);
            return;
        }

        var navOptions = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = useCache,
            TransitionInfoOverride = GetNavigationTransitionInfo(effect)
        };

        if (t?.IsSubclassOf(typeof(ViewModelBase)) == true)
            t = ResolvePageType(t);

        if (t == null)
        {
            navOptions.IsNavigationStackEnabled = false;
            _frame.NavigateToType(typeof(ErrorPage), null, navOptions);
            return;
        }

        if (_frame.Content?.GetType() != t)
        {
            _frame.NavigateToType(t, null, navOptions);
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
            t = ResolvePageType(t);

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
            _ => FrameEffect.NoTransition,
        };

        return transitionInfo;
    }

    private Frame? _frame;
    private Panel? _overlayHost;
    private PendingNavigation? pendingNavigation;

    private readonly record struct PendingNavigation(
        [param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type? Page,
        NavigationTransitionEffect Effect,
        bool UseCache);
}



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
        _frame = f;
    }

    public void SetOverlayHost(Panel p)
    {
        _overlayHost = p;
    }

    public void Navigate(Type? t, NavigationTransitionEffect effect = NavigationTransitionEffect.None, bool useCache = true)
    {
        if (_frame == null)
            return;

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
            new FluentAvalonia.UI.Navigation.FrameNavigationOptions
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
}



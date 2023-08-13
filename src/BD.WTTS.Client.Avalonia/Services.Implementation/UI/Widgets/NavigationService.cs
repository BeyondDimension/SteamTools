using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.Services.Implementation;

public sealed class NavigationService : INavigationService
{
    public static NavigationService Instance { get; } = (NavigationService)Ioc.Get<INavigationService>();

    public Control? PreviousPage { get; set; }

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

    public void Navigate(Type t, NavigationTransitionEffect effect = NavigationTransitionEffect.None)
    {
        if (t.IsSubclassOf(typeof(ViewModelBase)))
        {
            var pageType = GetViewModelToPageContent(t, false);
            if (pageType == null) return;
            t = (Type)pageType;
        }

        if (_frame?.Content?.GetType() != t)
        {
            _frame?.Navigate(t, null, GetNavigationTransitionInfo(effect));
        }
    }

    public void GoBack()
    {
        _frame?.GoBack();
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



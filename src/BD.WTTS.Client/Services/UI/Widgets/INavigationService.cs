namespace BD.WTTS.Services;

public interface INavigationService
{
    static INavigationService Instance { get; } = Ioc.Get<INavigationService>();

    object? GetViewModelToPageContent(object viewModel, bool isCreateInstance = true);

    void Navigate(Type? t, NavigationTransitionEffect effect = NavigationTransitionEffect.None, bool useCache = true);

    void GoBack(Type? t = null);

    void NavigateFromContext(object dataContext, NavigationTransitionEffect transitionInfo = NavigationTransitionEffect.None);

    void ShowControlDefinitionOverlay(Type targetType);

    void ClearOverlay();
}

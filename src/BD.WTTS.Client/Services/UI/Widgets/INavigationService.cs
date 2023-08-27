namespace BD.WTTS.Services;

public interface INavigationService
{
    public static INavigationService Instance { get; } = Ioc.Get<INavigationService>();

    public object? GetViewModelToPageContent(object viewModel, bool isCreateInstance = true);

    public void Navigate(Type? t, NavigationTransitionEffect effect = NavigationTransitionEffect.None);

    public void GoBack();

    public void NavigateFromContext(object dataContext, NavigationTransitionEffect transitionInfo = NavigationTransitionEffect.None);

    public void ShowControlDefinitionOverlay(Type targetType);

    public void ClearOverlay();
}

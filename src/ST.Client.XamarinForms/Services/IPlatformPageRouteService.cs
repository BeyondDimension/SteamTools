using static System.Application.UI.ViewModels.TabItemViewModel;

namespace System.Application.Services;

public interface IPlatformPageRouteService
{
    static IPlatformPageRouteService? Instance => DI.Get_Nullable<IPlatformPageRouteService>();

    bool IsUseNativePage(TabItemId tabItemId);

    void GoToNativePage(TabItemId tabItemId);
}

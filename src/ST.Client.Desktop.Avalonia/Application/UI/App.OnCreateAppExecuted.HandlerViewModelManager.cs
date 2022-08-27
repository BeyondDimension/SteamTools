using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
#if !MAUI
using System.Application.UI.Views.Windows;
#endif

namespace System.Application.UI;

partial class App
{
    void HandlerViewModelManager(IViewModelManager vmService)
    {
        switch (vmService.MainWindow)
        {
            case CloudArchiveWindowViewModel:
                ProgramHost.IsMinimize = false;
#if !MAUI
                MainWindow = new CloudArchiveWindow();
#endif
                break;

            case AchievementWindowViewModel:
                ProgramHost.IsMinimize = false;
#if !MAUI
                MainWindow = new AchievementWindow();
#endif
                break;

            default:
                #region 主窗口启动时加载的资源
#if !UI_DEMO
                compositeDisposable.Add(SettingsHost.Save);
                compositeDisposable.Add(ProxyService.Current.Exit);
                if (IApplication.IsDesktopPlatform)
                {
                    compositeDisposable.Add(SteamConnectService.Current.Dispose);
                    if (GeneralSettings.IsStartupAppMinimized.Value)
                        ProgramHost.IsMinimize = true;
                }
                compositeDisposable.Add(ASFService.Current.StopASF);
#endif
                #endregion
#if !MAUI
                MainWindow = new MainWindow();
#endif
                break;
        }
#if !MAUI
        MainWindow.DataContext = vmService.MainWindow;
#else
        var mainWindowVM = (MainWindowViewModel?)vmService.MainWindow;
        MainPage = new AppShell(mainWindowVM.ThrowIsNull());
#endif
    }
}

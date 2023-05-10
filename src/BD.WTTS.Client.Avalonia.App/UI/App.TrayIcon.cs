namespace BD.WTTS.UI;

partial class App
{
    readonly Dictionary<string, ICommand> trayIconMenus = new();

    public IReadOnlyDictionary<string, ICommand> TrayIconMenus => trayIconMenus;

    void InitTrayIcon()
    {
        //        if (IViewModelManager.Instance.MainWindow == null ||
        //            ApplicationLifetime is not
        //            IClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime)
        //            return;

        //        var s = Startup.Instance;
        //        if (s.IsMainProcess)
        //        {
        //            s.HasTrayIcon = GeneralSettings.IsEnableTrayIcon.Value;

        //            if (s.HasTrayIcon)
        //            {
        //                IViewModelManager.Instance.InitTaskBarWindowViewModel();

        //                NotifyIconHelper.Init(this,
        //                    notifyIconClick: (_, _) => RestoreMainWindow());
        //            }
        //            else
        //            {
        //                NotifyIconHelper.Dispoe();
        //                IViewModelManager.Instance.DispoeTaskBarWindowViewModel();
        //            }

        //            classicDesktopStyleApplicationLifetime.ShutdownMode =
        //#if UI_DEMO
        //                ShutdownMode.OnMainWindowClose;
        //#else
        //                s.HasTrayIcon ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnMainWindowClose;
        //#endif

        //        }
    }
}
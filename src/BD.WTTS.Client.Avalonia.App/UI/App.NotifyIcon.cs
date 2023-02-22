namespace BD.WTTS.UI;

partial class App
{
    readonly Dictionary<string, ICommand> mNotifyIconMenus = new();

    public IReadOnlyDictionary<string, ICommand> NotifyIconMenus => mNotifyIconMenus;

    void InitTrayIcon(IClassicDesktopStyleApplicationLifetime? desktop)
    {
        if (desktop == null || IViewModelManager.Instance.MainWindow == null)
            return;
        if (Program.Host.Instance.IsMainProcess)
        {
            StartupOptions.Value.HasNotifyIcon = GeneralSettings.IsEnableTrayIcon.Value;

            if (StartupOptions.Value.HasNotifyIcon)
            {
                IViewModelManager.Instance.InitTaskBarWindowViewModel();

                NotifyIconHelper.Init(this, NotifyIcon_Click);
            }
            else
            {
                NotifyIconHelper.Dispoe();
                IViewModelManager.Instance.DispoeTaskBarWindowViewModel();
            }

            desktop.ShutdownMode =
#if UI_DEMO
                    ShutdownMode.OnMainWindowClose;
#else
            StartupOptions.Value.HasNotifyIcon ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnMainWindowClose;
#endif

        }
    }

    void NotifyIcon_Click(object? sender, EventArgs e)
    {
        RestoreMainWindow();
    }
}
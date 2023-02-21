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

                NotifyIconHelper.Init(this, () =>
                {
                    RestoreMainWindow();
                });
                //                        if (!OperatingSystem2.IsLinux())
                //                        {
                //                            (var notifyIcon, var menuItemDisposable) = NotifyIconHelper.Init(NotifyIconHelper.GetIconByCurrentAvaloniaLocator);
                //                            notifyIcon.Click += NotifyIcon_Click;
                //                            notifyIcon.DoubleClick += NotifyIcon_Click;
                //                            if (menuItemDisposable != null) menuItemDisposable.AddTo(this);
                //                            notifyIcon.AddTo(this);
                //                        }
                //                        else
                //                        {
                //#if LINUX || DEBUG
                //                            NotifyIconHelper.StartPipeServer();
                //#endif
                //                        }
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
}
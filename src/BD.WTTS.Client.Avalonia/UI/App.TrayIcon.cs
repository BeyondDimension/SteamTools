using BD.WTTS.Client.Resources;
using static BD.WTTS.Services.INotificationService;

namespace BD.WTTS.UI;

partial class App
{
    public TrayIcons TrayIcons { get; init; } = new TrayIcons();

    readonly Dictionary<string, ICommand> trayIconMenus = new();

    public IReadOnlyDictionary<string, ICommand> TrayIconMenus => trayIconMenus;

    void InitTrayIcon()
    {
        if (ApplicationLifetime is not
            IClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime)
            return;

        var s = Startup.Instance;
        if (s.IsMainProcess)
        {
            s.HasTrayIcon = GeneralSettings.IsEnableTrayIcon.Value;
            if (s.HasTrayIcon)
            {
                TrayIcon.SetIcons(this, TrayIcons);
                TrayIcons.Add(new TrayIcon
                {
                    Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://BD.WTTS.Client.Avalonia/UI/Assets/ApplicationIcon.ico"))),
                    ToolTipText = AssemblyInfo.Trademark,
                    Command = ReactiveCommand.Create(RestoreMainWindow),
                    //[!TrayIcon.IsVisibleProperty] = new Binding { Path = "Value", Source = GeneralSettings.IsEnableTrayIcon, Mode = BindingMode.OneWay },
                    Menu = new NativeMenu
                    {
                        new NativeMenuItem
                        {
                            Header = "Test",
                        },
                        new NativeMenuItemSeparator(),
                        new NativeMenuItem
                        {
                            [!NativeMenuItem.HeaderProperty] = new Binding { Path = "Res.Exit", Source = ResourceService.Current, Mode = BindingMode.OneWay },
                            Command = ReactiveCommand.Create(() => { Shutdown(); })
                        }
                    }
                });

                //IViewModelManager.Instance.InitTaskBarWindowViewModel();
                //NotifyIconHelper.Init(this,
                //    notifyIconClick: (_, _) => RestoreMainWindow());
            }
            else
            {
                foreach (var trayIcon in TrayIcons)
                {
                    trayIcon.Dispose();
                }
                TrayIcons.Clear();
                TrayIcon.SetIcons(this, null);
                //NotifyIconHelper.Dispoe();
                //IViewModelManager.Instance.DispoeTaskBarWindowViewModel();
            }

            classicDesktopStyleApplicationLifetime.ShutdownMode =
#if UI_DEMO
                        ShutdownMode.OnMainWindowClose;
#else
                s.HasTrayIcon ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnMainWindowClose;
#endif

        }
    }
}
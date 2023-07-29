//#if WINDOWS
//using System.Windows;
//#endif
//using AppResources = BD.WTTS.Client.Resources.Strings;

//// ReSharper disable once CheckNamespace
//namespace BD.WTTS;

///// <inheritdoc cref="INotificationService.NotifyIconHelper"/>
//sealed class NotifyIconHelper : INotificationService.NotifyIconHelper
//{
//    NotifyIconHelper() => throw new NotSupportedException();

//    static object? Tray = null;

//    static Stream? GetIcon(IAssetLoader assets)
//    {
//        string iconPath;
//        if (OperatingSystem.IsMacOS())
//        {
//            iconPath = "avares://BD.WTTS.Client.Avalonia.App/Application/UI/Assets/ApplicationIcon_16.png";
//        }
//        else
//        {
//            iconPath = "avares://BD.WTTS.Client.Avalonia/Application/UI/Assets/ApplicationIcon.ico";
//        }

//        try
//        {
//            return assets.Open(new(iconPath));
//        }
//        catch
//        {
//            return null;
//        }
//    }

//    static Stream? GetIconByCurrentAvaloniaLocator()
//    {
//        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
//        return GetIcon(assets);
//    }

//    //    public static void Init(App app, EventHandler notifyIconClick)
//    //    {
//    //        if (IsInitialized) return;
//    //        IDisposable? menuItemDisposable = null;
//    //        var icon = GetIconByCurrentAvaloniaLocator();
//    //        var text = TaskBarWindowViewModel.TitleString;
//    //#if WINDOWS
//    //        if (OperatingSystem.IsWindows())
//    //        {
//    //            var notifyIcon = Ioc.Get<NotifyIcon>();
//    //            notifyIcon.Text = text;
//    //            notifyIcon.Icon = icon;
//    //            notifyIcon.Visible = true;
//    //#if WINDOWS
//    //            notifyIcon.RightClick += (_, e) =>
//    //            {
//    //                IViewModelManager.Instance.ShowTaskBarWindow(e.X, e.Y);
//    //            };
//    //#else
//    //            menuItemDisposable = InitMenuItems(notifyIcon);
//    //#endif
//    //            notifyIcon.Click += notifyIconClick;
//    //            notifyIcon.DoubleClick += notifyIconClick;
//    //            notifyIcon.AddTo(app);
//    //            Tray = notifyIcon;
//    //        }
//    //        else
//    //#endif
//    //        {
//    //            NativeMenu menu = new();
//    //            menuItemDisposable = InitMenuItems(menu);
//    //            TrayIcon trayIcon = new()
//    //            {
//    //                Icon = icon == default ? default : new(icon),
//    //                ToolTipText = text,
//    //                IsVisible = true,
//    //                Menu = menu,
//    //            };
//    //            trayIcon.Clicked += notifyIconClick;
//    //            TrayIcon.SetIcons(app, new()
//    //                {
//    //                    trayIcon,
//    //                });
//    //            Tray = trayIcon;
//    //            if (OperatingSystem.IsMacOS())
//    //            {
//    //                NativeMenu.SetMenu(app, menu);
//    //            }
//    //        }
//    //        menuItemDisposable?.AddTo(app);
//    //        IsInitialized = true;
//    //        return;
//    //    }

//    public static Stream? GetIcon()
//    {
//        var assets = new AssetLoader(typeof(NotifyIconHelper).Assembly);
//        return GetIcon(assets);
//    }

//    public static void Dispoe()
//    {
//#if WINDOWS
//        if (Tray is NotifyIcon tray1)
//        {
//            tray1.Visible = false;
//            tray1.Dispose();
//        }
//        else
//#endif
//        if (Tray is TrayIcon tray)
//        {
//            tray.IsVisible = false;
//            tray.Dispose();
//        }

//        IsInitialized = false;
//    }

//    #region 仅在非 Windows 上使用平台原生托盘菜单

//    static string Exit => AppResources.Exit;

//    static void OnMenuClick(string command)
//    {
//        //TaskBarWindowViewModel.OnMenuClick(command);
//    }

//    //static void OnMenuClick(TabItemViewModel.TabItemId tabItemId)
//    //{
//    //    TaskBarWindowViewModel.OnMenuClick(tabItemId);
//    //}

//    static NativeMenuItem? exitMenuItem;
//    //static Dictionary<TabItemViewModel.TabItemId, NativeMenuItem>? tabItems;

//    //static IDisposable? InitMenuItems(NativeMenu menu)
//    //{
//    //    if (IViewModelManager.Instance.MainWindow is not MainWindowViewModel main)
//    //        return null;

//    //    tabItems = new();
//    //    foreach (var item in main.AllTabIdItems)
//    //    {
//    //        var menuItem = new NativeMenuItem
//    //        {
//    //            Header = TabItemViewModel.GetName(item),
//    //            Command = ReactiveCommand.Create(() =>
//    //            {
//    //                OnMenuClick(item);
//    //            }),
//    //        };
//    //        tabItems.Add(item, menuItem);
//    //        menu.Add(menuItem);
//    //    }
//    //    exitMenuItem = new NativeMenuItem
//    //    {
//    //        Header = Exit,
//    //        Command = ReactiveCommand.Create(() =>
//    //        {
//    //            OnMenuClick(TaskBarWindowViewModel.CommandExit);
//    //        }),
//    //    };
//    //    menu.Add(exitMenuItem);

//    //    return ResourceService.Subscribe(() =>
//    //    {
//    //        if (exitMenuItem != null)
//    //        {
//    //            exitMenuItem.Header = Exit;
//    //        }
//    //        foreach (var item in tabItems)
//    //        {
//    //            item.Value.Header = TabItemViewModel.GetName(item.Key);
//    //        }
//    //    });
//    //}

//    #endregion
//}

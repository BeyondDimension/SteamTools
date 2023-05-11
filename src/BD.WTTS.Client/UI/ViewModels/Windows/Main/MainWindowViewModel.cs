// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public sealed partial class MainWindowViewModel : WindowViewModel
{
    #region 更改通知

    ViewModelBase? selectedItem;

    public ViewModelBase? SelectedItem
    {
        get => selectedItem;
        set
        {
            if (value != null)
            {
                if (MenuTabItemToPages.TryGetValue(value.GetType(), out var pageVMType))
                {
                    value = IViewModelManager.Instance.Get(pageVMType);
                }
            }
            this.RaiseAndSetIfChanged(ref selectedItem, value);
        }
    }

    [Reactive]
    public bool IsOpenUserMenu { get; set; }

    #endregion

    public ImmutableArray<TabItemViewModel> TabItems { get; }

    public ImmutableArray<TabItemViewModel> FooterTabItems { get; }

    public Dictionary<Type, Type> MenuTabItemToPages { get; }

    public MainWindowViewModel()
    {
        if (IApplication.IsDesktop())
        {
            var platformService = IPlatformService.Instance;
            var adminTag = platformService.IsAdministrator ? (OperatingSystem.IsWindows() ? " (Administrator)" : " (Root)") : string.Empty;
            var title = $"{AssemblyInfo.Trademark} {RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()} v{AssemblyInfo.Version} for {DeviceInfo2.OSName()}{adminTag}";
#if DEBUG
            title = $"[Debug] {title}";
#endif
            Title = title;

            //IUserManager.Instance.OnSignOut += () =>
            //{
            //    IsOpenUserMenu = false;
            //};

            //OpenUserMenu = ReactiveCommand.Create(() =>
            //{
            //    IsOpenUserMenu = UserService.Current.IsAuthenticated;
            //    if (!IsOpenUserMenu)
            //    {
            //        UserService.Current.ShowWindow(CustomWindow.LoginOrRegister);
            //    }
            //});
        }

        IEnumerable<TabItemViewModel> tabItems = new TabItemViewModel[] {
            new WelcomeMenuTabItemViewModel(),
        };
        IEnumerable<KeyValuePair<Type, Type>> menuTabItemToPages = new KeyValuePair<Type, Type>[]
        {
            //new KeyValuePair<Type, Type>(typeof(WelcomeMenuTabItemViewModel), typeof()),
            new KeyValuePair<Type, Type>(typeof(DebugMenuTabItemViewModel), typeof(DebugPageViewModel)),
            new KeyValuePair<Type, Type>(typeof(SettingsMenuTabItemViewModel), typeof(SettingsPageViewModel)),
            new KeyValuePair<Type, Type>(typeof(AboutMenuTabItemViewModel), typeof(AboutPageViewModel)),
        };

        if (Startup.Instance.TryGetPlugins(out var plugins))
        {
            tabItems = tabItems.Concat(plugins
                .Select(static x =>
                   {
                       try
                       {
                           return x.GetMenuTabItems();
                       }
                       catch (Exception ex)
                       {
                           Log.Error(nameof(MainWindowViewModel), ex,
                               $"({x.Name}) Plugin.GetMenuTabItems fail.");
                           return null;
                       }
                   })
                .Where(static x => x != null)
                .SelectMany(static x => x!));

            menuTabItemToPages = menuTabItemToPages.Concat(plugins
                .Select(static x =>
                {
                    try
                    {
                        return x.GetMenuTabItemToPages();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(MainWindowViewModel), ex,
                            $"({x.Name}) Plugin.GetMenuTabItemToPages fail.");
                        return null;
                    }
                })
                .Where(static x => x != null)
                .SelectMany(static x => x!));
        }

        TabItems = tabItems
#if DEBUG
            .Concat(new[] { new DebugMenuTabItemViewModel(), })
#endif
            .ToImmutableArray();
        MenuTabItemToPages = new(menuTabItemToPages);
        SelectedItem = TabItems.FirstOrDefault();

        FooterTabItems = ImmutableArray.Create<TabItemViewModel>(
            new SettingsMenuTabItemViewModel()/*,*/
            /*new AboutMenuTabItemViewModel()*/);
    }

    public override Task Initialize()
    {
        var task = Task.Run(() =>
         {
             Thread.CurrentThread.IsBackground = true;
             if (!IsInitialized)
             {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
                 Task.Run(async () =>
                 {
                     if (Startup.Instance.TryGetPlugins(out var plugins))
                     {
                         foreach (var plugin in plugins)
                         {
                             try
                             {
                                 await plugin.OnInitializeAsync();
                             }
                             catch (Exception ex)
                             {
                                 Log.Error(nameof(MainWindowViewModel), ex,
                                     $"({plugin.Name}) Plugin.OnInitializeAsync fail.");
                             }
                         }
                     }
                     //if (ASFSettings.AutoRunArchiSteamFarm.Value)
                     //{
                     //    if (platformService.UsePlatformForegroundService)
                     //    {
                     //        await platformService.StartOrStopForegroundServiceAsync(nameof(ASFService), true);
                     //    }
                     //    else
                     //    {
                     //        await ASFService.Current.InitASF();
                     //    }
                     //}
                 });
#endif

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
                 {
                     //SteamConnectService.Current.Initialize();
                     //SteamConnectService.Current.RefreshSteamUsers();
                 }
#endif

                 Parallel.ForEach(TabItems, item =>
                 {
                     item.Initialize();
                     //Task.Run(item.Initialize).ForgetAndDispose();
                 });
                 IsInitialized = true;
             }
         });

        task.ForgetAndDispose();
        return task;
    }

    //public async override void Activation()
    //{
    //    if (IsFirstActivation)
    //    {
    //        if (UISettings.DoNotShowMessageBoxs.Value?.Contains(MessageBox.DontPromptType.Donate) == false)
    //        {
    //            //INotificationService.Instance.Notify("如果你觉得 Watt Toolkit 好用，你可以考虑给我们一些捐助以支持我们继续开发，谢谢！", NotificationType.Message);
    //            await MessageBox.ShowAsync("如果你觉得 Watt Toolkit 好用，你可以考虑给我们一些捐助以支持我们继续开发，谢谢！", button: MessageBox.Button.OK,
    //                rememberChooseKey: MessageBox.DontPromptType.Donate);
    //        }
    //    }
    //    base.Activation();
    //}
}
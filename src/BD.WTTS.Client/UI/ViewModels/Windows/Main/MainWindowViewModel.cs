// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public sealed partial class MainWindowViewModel : WindowViewModel
{
    #region 更改通知
    [Reactive]
    public TabItemViewModel? SelectedItem { get; set; }

    [Reactive]
    public bool IsOpenUserMenu { get; set; }

    #endregion

    public List<TabItemViewModel> TabItems { get; }

    public ImmutableArray<TabItemViewModel> FooterTabItems { get; }

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
            new HomeMenuTabItemViewModel(),
        };

        if (Startup.Instance.TryGetPlugins(out var plugins))
        {
            var tabs = plugins.Select(static x =>
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
                .SelectMany(static x => x!);

            tabItems = tabItems.Concat(tabs);
        }

        TabItems = tabItems.ToList();
        SelectedItem = TabItems.FirstOrDefault();

        FooterTabItems = ImmutableArray.Create<TabItemViewModel>(
#if DEBUG
            new DebugMenuTabItemViewModel(),
#endif
            new SettingsMenuTabItemViewModel()
        );
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
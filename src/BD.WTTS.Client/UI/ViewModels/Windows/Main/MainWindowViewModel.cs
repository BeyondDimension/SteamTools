// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public sealed partial class MainWindowViewModel : WindowViewModel
{
    #region 更改通知
    //[Reactive]
    //public TabItemViewModel? SelectedItem { get; set; }

    [Reactive]
    public int PluginCount { get; set; }

    #endregion

    public List<TabItemViewModel> TabItems { get; }

    public ImmutableArray<TabItemViewModel> FooterTabItems { get; }

    public MainWindowViewModel(IEnumerable<TabItemViewModel> tabItems, ImmutableArray<TabItemViewModel> footerTabItems)
    {
        if (IApplication.IsDesktop())
        {
            var platformService = IPlatformService.Instance;
            var adminTag = platformService.IsAdministrator ? (OperatingSystem.IsWindows() ? " (Administrator)" : " (Root)") : string.Empty;
            var title = $"{AssemblyInfo.Trademark} {RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()} v{AssemblyInfo.InformationalVersion} for {AboutPageViewModel.GetOSName()}{adminTag}";
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
                             $"({x.UniqueEnglishName}) Plugin.GetMenuTabItems fail.");
                         return null;
                     }
                 })
                .Where(static x => x != null)
                .SelectMany(static x => x!);
            var sortTabSettings = UISettings.SortMenuTabs.Value;
            int OrderBy(MenuTabItemViewModel m)
            {
                if (sortTabSettings != null)
                {
                    var i = 0;
                    foreach (var item in sortTabSettings)
                    {
                        if (item == m.Id)
                            return i;
                        i++;
                    }
                }
                return int.MaxValue;
            }
            var comparer = ComparerBuilder.For<MenuTabItemViewModel>()
                .OrderBy(OrderBy)
                .ThenBy(x => x.Id);
            var sortTabs = new SortedSet<MenuTabItemViewModel>(comparer);
            foreach (var item in tabs)
                sortTabs.Add(item);
            tabItems = tabItems.Concat(sortTabs);
        }

        TabItems = tabItems.ToList();
        //SelectedItem = TabItems.FirstOrDefault();

        FooterTabItems = footerTabItems;
    }

    public override async Task Initialize()
    {
        if (!IsInitialized)
        {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            if (OperatingSystem.IsWindows())
            {
                // 等待 Ipc 管理员权限服务初始化完毕
                await IPlatformService.IPCRoot.Instance;
            }
            if (Startup.Instance.TryGetPlugins(out var plugins))
            {
                PluginCount = plugins.Count;
                foreach (var plugin in plugins)
                {
                    try
                    {
                        await plugin.OnInitializeAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(MainWindowViewModel), ex,
                            $"({plugin.UniqueEnglishName}) Plugin.OnInitializeAsync fail.");
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
#endif

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            {
                SteamConnectService.Current.Initialize();
                //SteamConnectService.Current.RefreshSteamUsers();
            }
#endif

            //Parallel.ForEach(TabItems, item =>
            //{
            //    item.Initialize();
            //    //Task.Run(item.Initialize).ForgetAndDispose();
            //});
            IsInitialized = true;
        }
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
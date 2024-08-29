using dotnetCampus.Ipc.CompilerServices.Attributes;

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

    public ObservableCollection<TabItemViewModel> TabItems { get; }

    public ImmutableArray<TabItemViewModel> FooterTabItems { get; }

    public MainWindowViewModel(IEnumerable<TabItemViewModel> tabItems, ImmutableArray<TabItemViewModel> footerTabItems)
    {
        if (IApplication.IsDesktop())
        {
            //var platformService = IPlatformService.Instance;
            //var adminTag = platformService.IsAdministrator ? (OperatingSystem.IsWindows() ? " (Administrator)" : " (Root)") : string.Empty;
            //var title = $"{AssemblyInfo.Trademark} {RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()} v{AssemblyInfo.InformationalVersion} for {AboutPageViewModel.GetOSName()}{adminTag}";
            var title = AssemblyInfo.Trademark;
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
            IReadOnlyList<MenuTabItemViewModel> tabItems2 =
                tabItems.OfType<MenuTabItemViewModel>().ToArray();
            var sortTabSettings = UISettings.SortMenuTabs.Value;
            int OrderBy(KeyValuePair<int, MenuTabItemViewModel> m)
            {
                if (sortTabSettings != null)
                {
                    int i = 0;
                    foreach (var item in sortTabSettings)
                    {
                        i++;
                        if (item == m.Value.Id)
                            return i;
                    }
                }
                return m.Key;
            }
            var comparer = ComparerBuilder.For<KeyValuePair<int, MenuTabItemViewModel>>()
                .OrderBy(OrderBy)
                .ThenBy(x => x.Value.Id);
            var sortTabs = new SortedSet<KeyValuePair<int, MenuTabItemViewModel>>(comparer);
            int i = byte.MaxValue;
            foreach (var item in tabItems2)
            {
                sortTabs.Add(new(i, item));
                i++;
            }
            foreach (var item in tabs)
            {
                sortTabs.Add(new(i, item));
                i++;
            }

            tabItems = sortTabs.Select(x => x.Value).ToArray();
        }

        TabItems = new ObservableCollection<TabItemViewModel>(tabItems);
        //SelectedItem = TabItems.FirstOrDefault();

        FooterTabItems = footerTabItems;
    }

    public override async Task Initialize()
    {
        if (!IsInitialized)
        {
            var startup = Startup.Instance;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            if (OperatingSystem.IsWindows())
            {
                // 等待 Ipc 管理员权限服务初始化完毕
                await IPlatformService.IPCRoot.Instance;
            }
            if (startup.TryGetPlugins(out var plugins))
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

            #region 提示设置项配置文件，如果存在无效的文件时

            if (startup.InvalidConfiguration)
            {
                MainThread2.BeginInvokeOnMainThread(() =>
                {
                    StringBuilder b = new(Strings.Settings_InvalidConfigurationFile);
                    b.AppendLine(Environment.NewLine);
                    foreach (var item in startup.InvalidConfigurationFileNames)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;
                        b.Append(item);
                        b.AppendLine(FileEx.JSON);
                    }
                    var errorMsg = b.ToString();
                    MessageBox.Show(errorMsg, Strings.Error, icon: MessageBox.Image.Error);
                });
            }

            #endregion

            IsInitialized = true;
        }

        await Task.CompletedTask;
    }

    //public async override void Activation()
    //{
    //    //if (IsFirstActivation)
    //    //{
    //    //    if (UISettings.DoNotShowMessageBoxs.Value?.Contains(MessageBox.DontPromptType.Donate) == false)
    //    //    {
    //    //        //INotificationService.Instance.Notify("如果你觉得 Watt Toolkit 好用，你可以考虑给我们一些捐助以支持我们继续开发，谢谢！", NotificationType.Message);
    //    //        await MessageBox.ShowAsync("如果你觉得 Watt Toolkit 好用，你可以考虑给我们一些捐助以支持我们继续开发，谢谢！", button: MessageBox.Button.OK,
    //    //            rememberChooseKey: MessageBox.DontPromptType.Donate);
    //    //    }
    //    //}

    //    base.Activation();
    //}

    public override void Deactivation()
    {
        SaveTabItemsSort();
        base.Deactivation();
    }

    public void SaveTabItemsSort()
    {
        UISettings.SortMenuTabs.Value = TabItems.OfType<MenuTabItemViewModel>().Select(x => x.Id).ToHashSet();
    }
}
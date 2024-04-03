// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed partial class ProxyService
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    : ReactiveObject, IProxyService
#endif
{
    static readonly Lazy<ProxyService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ProxyService Current => mCurrent.Value;

    readonly IReverseProxyService reverseProxyService = IReverseProxyService.Constants.Instance;
    readonly IScriptManager scriptManager = IScriptManager.Instance;
    readonly IHostsFileService hostsFileService = IHostsFileService.Constants.Instance;
    readonly IPlatformService platformService = IPlatformService.Instance;

    ProxyService()
    {
        ProxyDomains = new SourceCache<AccelerateProjectGroupDTO, string>(s => s.Name);
        ProxyScripts = new SourceList<ScriptDTO>();

        ProxyDomains
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Sort(SortExpressionComparer<AccelerateProjectGroupDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
            .Bind(out _ProxyDomainsList)
            .Subscribe(_ => SelectGroup = ProxyDomains.Items.FirstOrDefault());

        this.WhenValueChanged(x => x.ProxyStatus, false)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async proxyStatusLeft =>
            {
                bool proxyStatusRight;
                if (proxyStatusLeft)
                {
                    var reuslt = await StartProxyServiceAsync();
                    proxyStatusRight = reuslt.OnStartedShowToastReturnProxyStatus();
                }
                else
                {
                    var reuslt = await StopProxyServiceAsync();
                    proxyStatusRight = reuslt.OnStopedShowToastReturnProxyStatus();
                }
                if (proxyStatusLeft != proxyStatusRight)
                {
                    ProxyStatus = proxyStatusRight;

                    //UpdateProxyTrayMenuItems();
                    //if (Steamworks.SteamClient.IsValid)
                    //{
                    //    if (ProxyStatus)
                    //        Steamworks.SteamFriends.SetRichPresence("steam_display", "#Status_Accelerator");
                    //    else
                    //        Steamworks.SteamFriends.ClearRichPresence();
                    //}
                }
            });

        this.WhenAnyValue(v => v.ProxyDomainsList)
              .ObserveOn(RxApp.MainThreadScheduler)
              .Subscribe(domain => domain?
              .ToObservableChangeSet()
              .AutoRefresh(x => x.ObservableItems)
              .TransformMany(t => t.ObservableItems ?? new ObservableCollection<AccelerateProjectDTO>())
              .AutoRefresh(x => x.Checked)
              .WhenPropertyChanged(x => x.Checked, false)
              .Subscribe(_ =>
              {
                  IsChangeSupportProxyServicesStatus = true;
                  ProxySettings.SupportProxyServicesStatus.Value = EnableProxyDomains?.Select(k => k.Id.ToString()).ToImmutableHashSet();
              }));

        this.WhenAnyValue(v => v.ProxyScripts)
              .ObserveOn(RxApp.MainThreadScheduler)
              .Subscribe(script => script?
              .Connect()
              .AutoRefresh(x => x.Disable)
              .WhenPropertyChanged(x => x.Disable, false)
              .Subscribe(async item =>
              {
                  await scriptManager.SaveEnableScriptAsync(item.Sender);
                  //ProxySettings.ScriptsStatus.Value = EnableProxyScripts?.Where(w => w?.LocalId > 0).Select(k => k.LocalId).ToImmutableHashSet();
                  //ProxySettings.ScriptsStatus.Value = ProxyScripts.Items.Where(x => x?.LocalId > 0).Select(k => k.LocalId).ToImmutableHashSet();
                  if (reverseProxyService.ProxyRunning)
                  {
                      //await EnableProxyScripts.ContinueWith(e =>
                      //{
                      //    reverseProxyService.Scripts = e.Result?.ToImmutableArray();
                      //});
                      this.RaisePropertyChanged(nameof(EnableProxyScripts));
                  }
              }));
    }

    public SourceCache<AccelerateProjectGroupDTO, string> ProxyDomains { get; }

    private readonly ReadOnlyObservableCollection<AccelerateProjectGroupDTO> _ProxyDomainsList;

    public ReadOnlyObservableCollection<AccelerateProjectGroupDTO> ProxyDomainsList => _ProxyDomainsList;

    private AccelerateProjectGroupDTO? _SelectGroup;

    public AccelerateProjectGroupDTO? SelectGroup
    {
        get => _SelectGroup;
        set => this.RaiseAndSetIfChanged(ref _SelectGroup, value);
    }

    public SourceList<ScriptDTO> ProxyScripts { get; }

    public IEnumerable<AccelerateProjectDTO>? GetEnableProxyDomains()
    {
        if (!ProxyDomains.Items.Any_Nullable())
            return null;
        var data = ProxyDomains.Items
            .Where(x => x.Items != null)
            .SelectMany(s => s.Items!.Where(w => w.ThreeStateEnable != false));
        //return data.Concat(data.SelectMany(s => GetProxyDomainsItems(s)));
        return data;
    }

    public IReadOnlyCollection<AccelerateProjectDTO>? EnableProxyDomains => GetEnableProxyDomains()?.ToImmutableArray();

    //static IEnumerable<AccelerateProjectDTO>? GetProxyDomainsItems(AccelerateProjectDTO accelerates)
    //{
    //    return accelerates.Items.Where(w => w.Enable).SelectMany(GetProxyDomainsItems);
    //}

    static void EnableProxyDomainsItems(AccelerateProjectDTO accelerates)
    {
        if (accelerates.Items != null)
        {
            foreach (var item in accelerates.Items)
            {
                item.Checked = accelerates.Checked;
                EnableProxyDomainsItems(item);
            }
        }
    }

    public IEnumerable<ScriptDTO>? GetEnableProxyScripts()
    {
        //if (!IsEnableScript)
        //return null;
        if (!ProxyScripts.Items.Any_Nullable())
            return null;
        return ProxyScripts.Items!.Where(w => !w.Disable).OrderBy(x => x.Order);
    }

    public Task<IEnumerable<ScriptDTO>?> EnableProxyScripts => scriptManager.LoadingScriptContentAsync(GetEnableProxyScripts());

    private DateTimeOffset _StartAccelerateTime;

    [Reactive]
    public string? IPv6AddresString { get; set; }

    [Reactive]
    public TimeSpan AccelerateTime { get; set; }

    public bool IsEnableScript
    {
        get => ProxySettings.IsEnableScript.Value;
        set
        {
            ProxySettings.IsEnableScript.Value = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(EnableProxyScripts));
        }
    }

    public bool IsOnlyWorkSteamBrowser
    {
        get
        {
            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()) return false;
            return ProxySettings.IsOnlyWorkSteamBrowser.Value;
        }

        set
        {
            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()) return;
            if (ProxySettings.IsOnlyWorkSteamBrowser.Value != value)
            {
                ProxySettings.IsOnlyWorkSteamBrowser.Value = value;
                this.RaisePropertyChanged();
            }
        }
    }

    #region HOSTS_PROXY_RUNNING_STATUS

    //const string KEY_HOSTS_PROXY_RUNNING_STATUS = "KEY_HOSTS_PROXY_RUNNING_STATUS";
    //static async void SaveHostsProxyStatus(bool value)
    //{
    //    await ISecureStorage.Instance.SetAsync<bool>(KEY_HOSTS_PROXY_RUNNING_STATUS, value);
    //}

    //public static async Task<bool> GetHostsProxyStatusAsync()
    //{
    //    var r = await ISecureStorage.Instance.GetAsync<bool>(KEY_HOSTS_PROXY_RUNNING_STATUS);
    //    return r;
    //}

    #endregion HOSTS_PROXY_RUNNING_STATUS

    #region 代理状态启动退出

    private bool _ProxyStatus;

    public bool ProxyStatus
    {
        get { return _ProxyStatus; }
        set => this.RaiseAndSetIfChanged(ref _ProxyStatus, value);
    }

    #endregion 代理状态启动退出

    static bool IsProgramStartupRunProxy()
    {
        var s = Startup.Instance;

        if (s.IsProxyService &&
            (s.ProxyServiceStatus == OnOffToggle.On ||
                s.ProxyServiceStatus == OnOffToggle.Toggle))
            return true;

        return ProxySettings.ProgramStartupRunProxy.Value;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await InitializeAccelerateAsync();
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, nameof(ProxyService),
                msg: "Accelerate init fail.");
            return; // 加速项目初始化失败时，中止初始化
        }

        try
        {
            await InitializeScriptAsync();
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, nameof(ProxyService),
                msg: "Script init fail.");
            return; // 脚本数据初始化失败时，中止初始化
        }

        try
        {
            await RefreshIpv6Support();
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, nameof(ProxyService),
                msg: "IPv6 refresh fail.");
            // Ipv6 支持刷新失败时，可忽略
        }

        try
        {
            if (IsProgramStartupRunProxy())
            {
                if (platformService.UsePlatformForegroundService)
                {
                    await platformService.StartOrStopForegroundServiceAsync(nameof(ProxyService), true);
                }
                else
                {
                    ProxyStatus = true;
                }
            }
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, nameof(ProxyService),
                msg: "Program startup run proxy fail.");
            // 程序启动时启动加速服务失败，可忽略
        }

        UpdateProxyTrayMenuItems();
    }

    private void UpdateProxyTrayMenuItems()
    {
        try
        {
            IApplication.Instance.UpdateMenuItems(Plugin.Instance.UniqueEnglishName, new TrayMenuItem
            {
                Name = Plugin.Instance.Name,
                Items = new List<TrayMenuItem>
                {
                    new TrayMenuItem
                    {
                        Name = "启动/停止",
                        Command = ReactiveCommand.Create(() =>
                        {
                            ProxyStatus = !ProxyStatus;
                        }),
                    },
                },
            });
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            //托盘菜单添加异常
        }
    }

    public async Task InitializeAccelerateAsync()
    {
        ProxyDomains.Clear();
        // 加载代理服务数据
        var client = IMicroServiceClient.Instance.Accelerate;
#if DEBUG
        var stopwatch = Stopwatch.StartNew();
#endif
        var result = await client.All();
#if DEBUG
        stopwatch.Stop();
        Toast.Show(ToastIcon.Info, Strings.Info_LoadingAgentTakesTime____.Format(stopwatch.ElapsedMilliseconds, result.IsSuccess, result.Code, result.Content?.Count));
#endif
        if (result.IsSuccess)
        {
            ProxyDomains.AddOrUpdate(result.Content!);
        }

        LoadOrSaveLocalAccelerate();

        if (ProxySettings.SupportProxyServicesStatus.Value.Any_Nullable() && ProxyDomains.Items.Any_Nullable())
        {
            var items = ProxyDomains.Items!.SelectMany(s => s.Items!);
            var enableItems = ProxySettings.SupportProxyServicesStatus.Value;
            foreach (var item in items)
            {
                if (enableItems.Contains(item.Id.ToString()))
                {
                    item.ThreeStateEnable = true;
                }
            }
        }

        //if (IsLoadImage && ProxyDomains.Items.Any_Nullable())
        //{
        //    foreach (var item in ProxyDomains.Items)
        //    {
        //        item.ImageStream = ImageChannelType.AccelerateGroup.GetImageAsync(ImageUrlHelper.GetImageApiUrlById(item.ImageId));
        //    }
        //}
    }

    public static bool IsChangeSupportProxyServicesStatus { get; set; }

    private void LoadOrSaveLocalAccelerate()
    {
        var localAccelerateFilePath = Path.Combine(Plugin.Instance.AppDataDirectory,
            "LOCAL_ACCELERATE.json");
        if (ProxyDomains.Items.Any_Nullable())
        {
            if (IOPath.TryOpen(localAccelerateFilePath,
                FileMode.Create, FileAccess.Write, FileShare.Read,
                out var fileStream, out var _))
            {
                using var stream = fileStream;
                MessagePackSerializer.Serialize(stream, ProxyDomains.Items, options: Serializable.lz4Options);
            }
        }
        else
        {
            if (File.Exists(localAccelerateFilePath) &&
                IOPath.TryOpenRead(localAccelerateFilePath,
                out var fileStream, out var _))
            {
                using var stream = fileStream;
                ProxyDomains.Clear();
                List<AccelerateProjectGroupDTO>? accelerates = null;
                try
                {
                    accelerates = MessagePackSerializer.Deserialize<List<AccelerateProjectGroupDTO>>(stream, options: Serializable.lz4Options);
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(ProxyService), ex, nameof(LoadOrSaveLocalAccelerate));
                }
                if (accelerates.Any_Nullable())
                {
                    ProxyDomains.AddOrUpdate(accelerates!);
                }
            }
        }
    }

    Timer? timer;

    public void StartTimer()
    {
        timer ??= new Timer(_ => AccelerateTime = DateTimeOffset.Now - _StartAccelerateTime,
            nameof(AccelerateTime), 0, 1000);
    }

    public void StopTimer()
    {
        if (timer != null)
        {
            timer.Dispose();
            timer = null;
        }
    }

    public static async ValueTask OnExitRestoreHosts()
    {
        var s = Ioc.Get_Nullable<IHostsFileService>();
        if (s != null)
        {
            var needClear = s.ContainsHostsByTag();
            if (needClear)
            {
                await s.OnExitRestoreHosts();
            }
        }
    }

    #region 脚本相关

    /// <summary>
    /// 初始化脚本数据
    /// </summary>
    /// <returns></returns>
    public async Task InitializeScriptAsync()
    {
        // 加载脚本数据

        var scriptList = await scriptManager.GetAllScriptAsync();
        var scriptList2 = await scriptManager.CheckFiles(scriptList);
        ProxyScripts.AddRange(scriptList2);

        //拉取 GM.js
        await BasicsInfoAsync();
        //初始化后检查脚本更新
        CheckScriptUpdate();
    }

    /// <summary>
    /// 找不到 GM.js 下载，有多个删除全部重新下载
    /// </summary>
    public async Task BasicsInfoAsync()
    {
        var basicsItems = ProxyScripts.Items.Where(x => x.Id == Guid.Parse("00000000-0000-0000-0000-000000000001")).ToArray();
        var count = basicsItems.Length;
        if (count == 1)
            return;
        else if (count > 1)
        {
            foreach (var item in basicsItems)
            {
                await scriptManager.DeleteScriptAsync(item);
                ProxyScripts.Remove(item);
            }
        }
        var basicsInfo = await IMicroServiceClient.Instance.Script.GM(Strings.Script_UpdateError);
        if (basicsInfo.Code == ApiRspCode.OK && basicsInfo.Content != null)
        {
            var jspath = await scriptManager.DownloadScriptAsync(basicsInfo.Content.UpdateLink);
            if (jspath.IsSuccess)
            {
                var build = await scriptManager.AddScriptAsync(jspath.Content!, basicsInfo.Content, isCompile: false, order: 1, deleteFile: true, pid: basicsInfo.Content.Id, ignoreCache: true);
                if (build.IsSuccess)
                {
                    if (build.Content != null)
                    {
                        build.Content.IsBasics = true;
                        ProxyScripts.Insert(0, build.Content);
                    }
                }
                else
                    Toast.Show(ToastIcon.Error, build.Message);
            }
            else
                Toast.Show(ToastIcon.Error, jspath.GetMessageByFormat(Strings.Download_ScriptError_));
        }
    }

    public async Task AddNewScriptAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Exists)
        {
            var info = await scriptManager.ReadScriptAsync(filePath);
            //ScriptDTO.TryParse(filename, out ScriptDTO? info);
            if (info != null)
            {
                var scriptItem = ProxyScripts.Items.FirstOrDefault(x => x.Name == info.Name);
                if (scriptItem != null)
                {
                    var result = MessageBox.ShowAsync(Strings.Script_ReplaceTips, button: MessageBox.Button.OKCancel).ContinueWith(async (s) =>
                    {
                        if (s.Result == MessageBox.Result.OK)
                        {
                            await AddNewScriptAsync(fileInfo, info, scriptItem);
                        }
                    });
                }
                else
                {
                    await AddNewScriptAsync(fileInfo, info);
                }
            }
            else
            {
                await AddNewScriptAsync(fileInfo, info);
            }
        }
        else
        {
            // $"文件不存在:{filePath}";
            Toast.Show(ToastIcon.Error, Strings.Script_FileError_.Format(filePath));
        }
    }

    public async Task AddNewScriptAsync(FileInfo fileInfo, ScriptDTO? info, ScriptDTO? oldInfo = null)
    {
        bool isCompile = true;
        long order = 10;
        if (oldInfo != null)
        {
            isCompile = oldInfo.IsCompile;
            order = oldInfo.Order;
        }
        var item = await scriptManager.SaveScriptAsync(fileInfo, info, oldInfo, isCompile: isCompile, order: order);
        if (item.IsSuccess)
        {
            if (item.Content != null)
            {
                if (oldInfo == null)
                {
                    ProxyScripts.Add(item.Content);
                }
                else
                {
                    ProxyScripts.Replace(oldInfo, item.Content);
                }
            }
        }
        Toast.Show(item.IsSuccess ? ToastIcon.Success : ToastIcon.Error, item.Message);
        await RefreshScript();
    }

    /// <summary>
    /// 刷新脚本列表
    /// </summary>
    public async Task RefreshScript()
    {
        var scriptList = await scriptManager.GetAllScriptAsync();
        ProxyScripts.Clear();
        ProxyScripts.AddRange(scriptList);

        CheckScriptUpdate();
    }

    /// <summary>
    /// 下载或更新JS 直接替换或新增进列表不需要刷新
    /// </summary>
    /// <param name="model"></param>
    public async void DownloadScript(ScriptDTO model)
    {
        model.IsLoading = true;
        var jspath = await scriptManager.DownloadScriptAsync(model.UpdateLink);
        if (jspath.IsSuccess)
        {
            var build = await scriptManager.AddScriptAsync(jspath.Content!, model, isCompile: model.IsCompile, order: model.Order, deleteFile: true, pid: model.Id);
            if (build.IsSuccess)
            {
                if (build.Content != null)
                {
                    model.IsUpdate = false;
                    model.IsExist = true;
                    build.Content.IsUpdate = false;
                    build.Content.IsExist = true;
                    var basicsItem = Current.ProxyScripts.Items.FirstOrDefault(x => x.Id == model.Id);
                    if (basicsItem != null)
                    {
                        ProxyScripts.Replace(basicsItem, build.Content);
                    }
                    else
                    {
                        ProxyScripts.Add(build.Content);
                    }
                    Toast.Show(ToastIcon.Success, Strings.Download_ScriptOk);
                }
            }
            else
            {
                Toast.Show(ToastIcon.Error, build.Message);
            }
        }
        else
        {
            Toast.Show(ToastIcon.Error, jspath.GetMessageByFormat(Strings.Download_ScriptError_));
        }
        model.IsLoading = false;
    }

    /// <summary>
    /// 检查 JS 更新
    /// </summary>
    public async void CheckScriptUpdate()
    {
        var items = ProxyScripts.Items.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        var client = IMicroServiceClient.Instance.Script;
        var response = await client.GetInfoByIds(Strings.Script_UpdateError, items);
        if (response.Code == ApiRspCode.OK && response.Content != null)
        {
            foreach (var item in ProxyScripts.Items)
            {
                var newItem = response.Content.FirstOrDefault(x => x.Id == item.Id);
                if (newItem != null && item.Version != newItem.Version)
                {
                    item.NewVersion = newItem.Version;
                    item.UpdateLink = newItem.UpdateLink;
                    item.IsUpdate = true;
                    ProxyScripts.Replace(item, item);
                }
            }
        }
    }

    #endregion 脚本相关

    public async void FixNetwork()
    {
        await OnExitRestoreHosts();

#if WINDOWS
        {
            await platformService.SetAsSystemProxyAsync(false);
            await platformService.SetAsSystemPACProxyAsync(false);
            await reverseProxyService.StopProxyAsync();
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    Arguments = "netsh winsock reset",
                });
            }
            catch
            {
            }
        }
#endif

        Toast.Show(ToastIcon.Success, Strings.FixNetworkComplete);
    }

    public async Task<bool> RefreshIpv6Support()
    {
        var result = await IMicroServiceClient.Instance.Accelerate.GetMyIP(ipV6: true);
        if (!result.IsSuccess) return false;
        IPv6AddresString = result.Content;
        return !string.IsNullOrEmpty(IPv6AddresString);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCoreAsync().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    async void Dispose(bool disposing)
    {
        if (disposing)
        {
            await ExitAsync().ConfigureAwait(false);
        }
    }

    async ValueTask DisposeAsyncCoreAsync()
    {
        await ExitAsync().ConfigureAwait(false);
    }

    public async ValueTask ExitAsync()
    {
        if (ProxyStatus)
        {
            await StopProxyServiceAsync(isExit: true);
        }
        reverseProxyService.Dispose();
    }
}
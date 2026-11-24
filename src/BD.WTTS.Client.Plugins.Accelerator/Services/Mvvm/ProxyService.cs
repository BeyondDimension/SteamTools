// ReSharper disable once CheckNamespace
using BD.WTTS.Helpers;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Linq;
using dotnetCampus.Ipc.Exceptions;

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
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Sort(SortExpressionComparer<AccelerateProjectGroupDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
            .Bind(out _ProxyDomainsList)
            .Subscribe(_ => SelectGroup = ProxyDomains.Items.FirstOrDefault());

        //this.WhenValueChanged(x => x.ProxyStatus, false)
        //    .ObserveOn(RxSchedulers.MainThreadScheduler)
        //    .Subscribe(async proxyStatusLeft =>
        //    {
        //        bool proxyStatusRight;
        //        if (proxyStatusLeft)
        //        {
        //            var reuslt = await StartProxyServiceAsync();
        //            proxyStatusRight = reuslt.OnStartedShowToastReturnProxyStatus();
        //        }
        //        else
        //        {
        //            var reuslt = await StopProxyServiceAsync();
        //            proxyStatusRight = reuslt.OnStopedShowToastReturnProxyStatus();
        //        }
        //        if (proxyStatusLeft != proxyStatusRight)
        //        {
        //            ProxyStatus = proxyStatusRight;

        //            //UpdateProxyTrayMenuItems();
        //            //if (Steamworks.SteamClient.IsValid)
        //            //{
        //            //    if (ProxyStatus)
        //            //        Steamworks.SteamFriends.SetRichPresence("steam_display", "#Status_Accelerator");
        //            //    else
        //            //        Steamworks.SteamFriends.ClearRichPresence();
        //            //}
        //        }
        //    });

        var domainChanges = this.ObserveValue(nameof(ProxyDomainsList), static x => x.ProxyDomainsList)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Select(groups => groups is null ? Observable.Empty<bool?>() : groups.ToObservableChangeSet()
                .AutoRefreshOnObservable(static group => group.ObserveValue(nameof(group.ObservableItems), static x => x.ObservableItems).Skip(1))
                .TransformMany(static group => group.ObservableItems ?? [])
                .MergeMany(static item => item.ObserveValue(nameof(item.ThreeStateEnable), static x => x.ThreeStateEnable).Skip(1)));
        Observable.Switch(domainChanges)
            .Subscribe(_ =>
            {
                IsChangeSupportProxyServicesStatus = true;
                ProxySettings.SupportProxyServicesStatus.Value = GetAccelerateEnableAllIds(EnableProxyDomains).ToImmutableHashSet();
            });

        var scriptChanges = this.ObserveValue(nameof(ProxyScripts), static x => x.ProxyScripts)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Select(scripts => scripts is null ? Observable.Empty<ScriptDTO>() : scripts.Connect()
                .MergeMany(static item => item.ObserveValue(nameof(item.Disable), static x => x.Disable).Skip(1).Select(_ => item)));
        Observable.Switch(scriptChanges)
            .Subscribe(async item =>
            {
                try
                {
                    await scriptManager.SaveEnableScriptAsync(item);
                    if (reverseProxyService.ProxyRunning)
                        this.RaisePropertyChanged(nameof(EnableProxyScripts));
                }
                catch (Exception ex)
                {
                    ex.LogAndShowT();
                }
            });
    }

    public SourceCache<AccelerateProjectGroupDTO, string> ProxyDomains { get; }

    private readonly ReadOnlyObservableCollection<AccelerateProjectGroupDTO> _ProxyDomainsList;

    public ReadOnlyObservableCollection<AccelerateProjectGroupDTO> ProxyDomainsList => _ProxyDomainsList;

    [Reactive]
    public partial bool IsLoadingAccelerate { get; set; } = true;

    readonly SemaphoreSlim accelerateLoadLock = new(1, 1);

    private AccelerateProjectGroupDTO? _SelectGroup;

    public AccelerateProjectGroupDTO? SelectGroup
    {
        get => _SelectGroup;
        set => this.RaiseAndSetIfChanged(ref _SelectGroup, value);
    }

    public SourceList<ScriptDTO> ProxyScripts { get; }

    public Task StartOrStopProxyService(bool startOrStop) => StartOrStopProxyService(startOrStop, null);

    internal async Task StartOrStopProxyService(bool startOrStop, Func<Task<bool>>? beforeOperation)
    {
        if (startOrStop == ProxyStatus)
        {
            return;
        }
        if (!ProxyStarting)
        {
            ProxyStopping = !startOrStop;
            ProxyStarting = true;
            try
            {
                if (beforeOperation != null && !await beforeOperation())
                    return;

                bool proxyStatusRight;
                if (startOrStop)
                {
                    var result = await StartProxyServiceAsync();
                    proxyStatusRight = await result.OnStartedShowToastReturnProxyStatus();
                }
                else
                {
                    var result = await StopProxyServiceAsync();
                    proxyStatusRight = result.OnStopedShowToastReturnProxyStatus();
                }

                if (startOrStop != proxyStatusRight)
                    startOrStop = proxyStatusRight;

                ProxyStatus = startOrStop;
                IsAnyProxyScripts = ProxyStatus && IsEnableScript;
            }
            finally
            {
                ProxyStarting = false;
                ProxyStopping = false;
            }
        }
    }

    public IEnumerable<AccelerateProjectDTO> GetEnableProxyDomains()
    {
        if (!ProxyDomains.Items.Any_Nullable())
            return [];
        var data = ProxyDomains.Items
            .Where(x => x.Items != null)
            .SelectMany(s => s.Items!.Where(w => w.ThreeStateEnable != false))
            .Select(item =>
            {
                //过滤部分选中的子项
                if (item.Items.Any_Nullable())
                {
                    return new AccelerateProjectDTO
                    {
                        Name = item.Name,
                        Port = item.Port,
                        MatchDomainNames = item.MatchDomainNames,
                        ForwardDomainNames = item.ForwardDomainNames,
                        IgnoreSSLCertVerification = item.IgnoreSSLCertVerification,
                        FakeServerName = item.FakeServerName,
                        ProxyType = item.ProxyType,
                        ListenDomainNames = item.ListenDomainNames,
                        Checked = item.Checked,
                        Id = item.Id,
                        Order = item.Order,
                        FakeUserAgent = item.FakeUserAgent,
                        Version = item.Version,
                        ThreeStateEnable = item.ThreeStateEnable,
                        // 递归过滤子项
                        Items = item.Items.Where(x => x.ThreeStateEnable != false).ToList()
                    };
                }
                return item;
            });

        return data;
    }

    public IReadOnlyCollection<AccelerateProjectDTO>? EnableProxyDomains => GetEnableProxyDomains().ToImmutableArray();

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

    TrayAccelerationSession? traySession;
    int readingTrayTraffic;

    [Reactive]
    public partial string? IPv6AddresString { get; set; }

    [Reactive]
    public partial TimeSpan AccelerateTime { get; set; }

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

    /// <summary>
    /// 代理启动或停止操作进行中
    /// </summary>
    [Reactive]
    public partial bool ProxyStarting { get; set; }

    /// <summary>
    /// 当前操作是否为停止代理。
    /// </summary>
    [Reactive]
    public partial bool ProxyStopping { get; private set; }

    [Reactive]
    public partial bool ProxyStatus { get; set; }

    [Reactive]
    public partial bool IsAnyProxyScripts { get; set; }

    private bool CheckProxyScriptsEnable()
    {
        if (!ProxyScripts.Items.Any_Nullable())
            return false;
        return ProxyScripts.Items!.Any(w => !w.Disable);
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

    /// <summary>
    /// Whether accelerator data and scripts finished initializing, independently of IPC readiness.
    /// </summary>
    public bool IsInitialized { get; private set; }

    public Exception? InitializationException { get; private set; }

    public async Task InitializeAsync()
    {
        IsInitialized = false;
        InitializationException = null;
        this.RaisePropertyChanged(nameof(IsInitialized));
        RegisterProxyTraySwitch();

        try
        {
            await InitializeAccelerateAsync();
        }
        catch (Exception ex)
        {
            InitializationException = ex;
            this.RaisePropertyChanged(nameof(InitializationException));
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
            InitializationException = ex;
            this.RaisePropertyChanged(nameof(InitializationException));
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
                    //ProxyStatus = true;
                    await StartOrStopProxyService(true);
                }
            }
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, nameof(ProxyService),
                msg: "Program startup run proxy fail.");
            // 程序启动时启动加速服务失败，可忽略
        }

        IsInitialized = true;
        this.RaisePropertyChanged(nameof(IsInitialized));
    }

    bool hasTraySwitch;

    void RegisterProxyTraySwitch()
    {
        if (hasTraySwitch)
            return;

        var state = Observable.Defer(() => Changed
            .Where(static change => change.PropertyName is nameof(ProxyStatus) or nameof(ProxyStarting)
                or nameof(IsInitialized) or nameof(InitializationException))
            .Select(_ => GetTrayState())
            .StartWith(GetTrayState())
            .DistinctUntilChanged());

        IApplication.Instance.UpdateMenuItems(Plugin.Instance.UniqueEnglishName, new TrayMenuItem
        {
            Name = Strings.CommunityFix,
            NameResourceKey = nameof(Strings.CommunityFix),
            ToggleState = state,
            GetAccelerationSession = () => traySession,
            Command = ReactiveCommand.CreateFromTask(ToggleProxyFromTrayAsync,
                state.Select(static value => value is TrayToggleState.Off or TrayToggleState.On).DistinctUntilChanged()),
        });
        hasTraySwitch = true;
    }

    TrayToggleState GetTrayState()
    {
        if (ProxyStarting)
            return ProxyStopping ? TrayToggleState.Stopping : TrayToggleState.Starting;
        if (ProxyStatus)
            return TrayToggleState.On;
        if (IsInitialized)
            return TrayToggleState.Off;
        return InitializationException == null ? TrayToggleState.Preparing : TrayToggleState.Unavailable;
    }

    TrayTrafficSample? ReadTrayTraffic()
    {
        // A closing panel may still have a read in flight when another panel opens.
        if (Interlocked.CompareExchange(ref readingTrayTraffic, 1, 0) != 0)
            return null;
        try
        {
            var flow = reverseProxyService.GetFlowStatistics();
            // The listener reads uploads from clients and writes downloads back to them.
            return flow == null ? null : new(flow.TotalRead, flow.TotalWrite, flow.ReadRate, flow.WriteRate);
        }
        catch (IpcException)
        {
            return null;
        }
        finally
        {
            Volatile.Write(ref readingTrayTraffic, 0);
        }
    }

    async Task ToggleProxyFromTrayAsync()
    {
        try
        {
            if (GetTrayState() is TrayToggleState.Off or TrayToggleState.On)
                await StartOrStopProxyService(!ProxyStatus);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(nameof(ProxyService));
        }
    }

    public async Task InitializeAccelerateAsync()
    {
        await accelerateLoadLock.WaitAsync();
        IsLoadingAccelerate = true;
        try
        {
            ProxyDomains.Clear();
            try
            {
                // 加载代理服务数据；网络不可用时继续尝试本地缓存。
                var client = IMicroServiceClient.Instance.Accelerate;
#if DEBUG
                var stopwatch = Stopwatch.StartNew();
#endif
                var result = await client.All().WaitAsync(TimeSpan.FromSeconds(20));
#if DEBUG
                stopwatch.Stop();
                Toast.Show(ToastIcon.Info, Strings.Info_LoadingAgentTakesTime____.Format(stopwatch.ElapsedMilliseconds, result.IsSuccess, result.Code, result.Content?.Count));
#endif
                if (result.IsSuccess && result.Content != null)
                    ProxyDomains.AddOrUpdate(result.Content);
            }
            catch (Exception ex)
            {
                Log.Error(nameof(ProxyService), ex, "Could not load accelerator projects from the server.");
            }

            LoadOrSaveLocalAccelerate();

            if (ProxySettings.SupportProxyServicesStatus.Value.Any_Nullable() && ProxyDomains.Items.Any_Nullable())
            {
                var items = ProxyDomains.Items!.SelectMany(s => s.Items!);
                var enableItems = ProxySettings.SupportProxyServicesStatus.Value;
                RestoreAccelerateEnableAllIds(items, enableItems);
            }
        }
        finally
        {
            IsLoadingAccelerate = false;
            accelerateLoadLock.Release();
        }
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
                Serialization.AccelerateCacheSerializer.Serialize(stream, ProxyDomains.Items);
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
                    accelerates = Serialization.AccelerateCacheSerializer.Deserialize(stream);
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

    private IEnumerable<string> GetAccelerateEnableAllIds(IEnumerable<AccelerateProjectDTO>? nodes)
    {
        if (nodes == null)
            return Enumerable.Empty<string>();
        return nodes.Where(s => s.ThreeStateEnable == true).SelectMany(node => new[] { node.Id.ToString() }.Concat(GetAccelerateEnableAllIds(node.Items)));
    }

    private void RestoreAccelerateEnableAllIds(IEnumerable<AccelerateProjectDTO> nodes, IReadOnlyCollection<string> enableItems)
    {
        foreach (var node in nodes)
        {
            if (node.Items.Any_Nullable())
            {
                RestoreAccelerateEnableAllIds(node.Items, enableItems);
                continue;
            }
            if (enableItems.Contains(node.Id.ToString()))
            {
                node.ThreeStateEnable = true;
            }
        }
    }

    Timer? timer;

    public void StartTimer()
    {
        if (traySession is { } session)
            timer ??= new Timer(_ => AccelerateTime = Stopwatch.GetElapsedTime(session.StartedTimestamp),
                null, 0, 1000);
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
        ProxyScripts.Clear();
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

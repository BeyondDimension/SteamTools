using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Properties;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Properties;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Models;
// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    public sealed class ProxyService : ReactiveObject, IDisposable
    {
        static ProxyService? mCurrent;
        public static ProxyService Current => mCurrent ?? new();

        readonly IHttpProxyService httpProxyService = IHttpProxyService.Instance;
        readonly IScriptManager scriptManager = IScriptManager.Instance;
        readonly IHostsFileService hostsFileService = IHostsFileService.Instance;
        readonly IPlatformService platformService = IPlatformService.Instance;

        public ProxyService()
        {
            mCurrent = this;

            ProxyDomains = new SourceList<AccelerateProjectGroupDTO>();
            ProxyScripts = new SourceList<ScriptDTO>();

            ProxyDomains
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<AccelerateProjectGroupDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
                .Bind(out _ProxyDomainsList)
                .Subscribe(_ => SelectGroup = ProxyDomains.Items.FirstOrDefault());


            this.WhenValueChanged(x => x.ProxyStatus, false)
                .Subscribe(async x =>
                {
                    if (x)
                    {
                        httpProxyService.ProxyDomains = EnableProxyDomains;
                        httpProxyService.IsEnableScript = ProxySettings.IsEnableScript.Value;
                        httpProxyService.Scripts = EnableProxyScripts;
                        httpProxyService.IsOnlyWorkSteamBrowser = ProxySettings.IsOnlyWorkSteamBrowser.Value;
                        httpProxyService.IsSystemProxy = ProxySettings.EnableWindowsProxy.Value;
                        httpProxyService.IsProxyGOG = ProxySettings.IsProxyGOG.Value;

                        // macOS 上目前因权限问题仅支持 0.0.0.0(IPAddress.Any)
                        httpProxyService.ProxyIp = (!OperatingSystem2.IsMacOS && IPAddress2.TryParse(ProxySettings.SystemProxyIp.Value, out var ip)) ? ip : IPAddress.Any;

                        httpProxyService.Socks5ProxyEnable = ProxySettings.Socks5ProxyEnable.Value;
                        httpProxyService.Socks5ProxyPortId = ProxySettings.Socks5ProxyPortId.Value;

                        //httpProxyService.HostProxyPortId = ProxySettings.HostProxyPortId;
                        httpProxyService.TwoLevelAgentEnable = ProxySettings.TwoLevelAgentEnable.Value;

                        httpProxyService.TwoLevelAgentProxyType = (ExternalProxyType)ProxySettings.TwoLevelAgentProxyType.Value;
                        if (!httpProxyService.TwoLevelAgentProxyType.IsDefined()) httpProxyService.TwoLevelAgentProxyType = IHttpProxyService.DefaultTwoLevelAgentProxyType;

                        httpProxyService.TwoLevelAgentIp = IPAddress2.TryParse(ProxySettings.TwoLevelAgentIp.Value, out var ip_t) ? ip_t.ToString() : IPAddress.Loopback.ToString();
                        httpProxyService.TwoLevelAgentPortId = ProxySettings.TwoLevelAgentPortId.Value;
                        httpProxyService.TwoLevelAgentUserName = ProxySettings.TwoLevelAgentUserName.Value;
                        httpProxyService.TwoLevelAgentPassword = ProxySettings.TwoLevelAgentPassword.Value;

                        if (IPAddress.TryParse(ProxySettings.ProxyMasterDns.Value, out var dns))
                            httpProxyService.ProxyDNS = dns;

                        this.RaisePropertyChanged(nameof(EnableProxyDomains));
                        this.RaisePropertyChanged(nameof(EnableProxyScripts));

                        if (!httpProxyService.IsSystemProxy)
                        {
                            const ushort httpsPort = 443;
                            var inUse = httpProxyService.PortInUse(httpsPort);
                            if (inUse)
                            {
                                string? error_CommunityFix_StartProxyFaild443 = null;
                                if (OperatingSystem2.IsWindows)
                                {
                                    var p = SocketHelper.GetProcessByTcpPort(httpsPort);
                                    if (p != null)
                                    {
                                        error_CommunityFix_StartProxyFaild443 = AppResources.CommunityFix_StartProxyFaild443___.Format(httpsPort, p.ProcessName, p.Id);
                                    }
                                }
                                error_CommunityFix_StartProxyFaild443 ??= AppResources.CommunityFix_StartProxyFaild443_.Format(httpsPort);
                                Toast.Show(error_CommunityFix_StartProxyFaild443);
                                return;
                            }
                        }

                        var isRun = await httpProxyService.StartProxy();

                        if (isRun)
                        {
                            if (!httpProxyService.IsSystemProxy)
                            {
                                if (httpProxyService.ProxyDomains.Any_Nullable())
                                {
                                    var localhost = IPAddress.Any.Equals(httpProxyService.ProxyIp) ? IPAddress.Loopback.ToString() : httpProxyService.ProxyIp.ToString();

                                    var hosts = httpProxyService.ProxyDomains!.SelectMany(s =>
                                    {
                                        if (s == null) return default!;

                                        return s.HostsArray.Select(host =>
                                        {
                                            if (host.Contains(' '))
                                            {
                                                var h = host.Split(' ');
                                                return KeyValuePair.Create(h[1], h[0]);
                                            }
                                            return KeyValuePair.Create(host, localhost);
                                        });
                                    }).ToDictionaryIgnoreRepeat(x => x.Key, y => y.Value);

                                    if (httpProxyService.IsEnableScript)
                                    {
                                        hosts.TryAdd(IHttpProxyService.LocalDomain, localhost);
                                    }

                                    var r = hostsFileService.UpdateHosts(hosts);

                                    if (r.ResultType != OperationResultType.Success)
                                    {
                                        if (OperatingSystem2.IsMacOS)
                                        {
                                            Browser2.Open(UrlConstants.OfficialWebsite_UnixHostAccess);
                                            //platformService.RunShell($" \\cp \"{Path.Combine(IOPath.CacheDirectory, "hosts")}\" \"{platformService.HostsFilePath}\"");
                                        }
                                        Toast.Show(AppResources.OperationHostsError_.Format(r.Message));
                                        httpProxyService.StopProxy();
                                        return;
                                    }
                                }
                            }
                            _StartAccelerateTime = DateTimeOffset.Now;
                            StartTimer();
                            Toast.Show(AppResources.CommunityFix_StartProxySuccess);
                        }
                        else
                        {
                            MessageBox.Show(AppResources.CommunityFix_StartProxyFaild);
                        }
                    }
                    else
                    {
                        httpProxyService.StopProxy();
                        StopTimer();
                        void OnStopRemoveHostsByTag()
                        {
                            var needClear = hostsFileService.ContainsHostsByTag();
                            if (needClear)
                            {
                                var r = hostsFileService.RemoveHostsByTag();

                                if (r.ResultType != OperationResultType.Success)
                                {
                                    Toast.Show(AppResources.OperationHostsError_.Format(r.Message));

                                    if (OperatingSystem2.IsMacOS || (OperatingSystem2.IsLinux && !platformService.IsAdministrator))
                                    {
                                        Browser2.Open(UrlConstants.OfficialWebsite_UnixHostAccess);
                                    }
                                    //return;
                                    //if (OperatingSystem2.IsMacOS && !ProxySettings.EnableWindowsProxy.Value)
                                    //{
                                    //    //platformService.RunShell($" \\cp \"{Path.Combine(IOPath.CacheDirectory, "hosts")}\" \"{platformService.HostsFilePath}\"", true);
                                    //}
                                }
                            }
                        }
                        OnStopRemoveHostsByTag();
                        //Toast.Show(SteamTools.Properties.Resources.ProxyStop);
                    }
                });
        }

        public SourceList<AccelerateProjectGroupDTO> ProxyDomains { get; }

        private readonly ReadOnlyObservableCollection<AccelerateProjectGroupDTO>? _ProxyDomainsList;
        public ReadOnlyObservableCollection<AccelerateProjectGroupDTO>? ProxyDomainsList => _ProxyDomainsList;

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        private AccelerateProjectGroupDTO? _SelectGroup;
        public AccelerateProjectGroupDTO? SelectGroup
        {
            get => _SelectGroup;
            set => this.RaiseAndSetIfChanged(ref _SelectGroup, value);
        }

        public SourceList<ScriptDTO> ProxyScripts { get; }

        public IReadOnlyCollection<AccelerateProjectDTO>? EnableProxyDomains
        {
            get
            {
                if (!ProxyDomains.Items.Any_Nullable())
                    return null;
                return ProxyDomains.Items
                    .Where(x => x.Items != null)
                    .SelectMany(s => s.Items!.Where(w => w.Enable))
                    .ToArray();
            }
        }

        public IReadOnlyCollection<ScriptDTO>? EnableProxyScripts
        {
            get
            {
                //if (!IsEnableScript)
                //return null;
                if (!ProxyScripts.Items.Any_Nullable())
                    return null;
                return ProxyScripts.Items!.Where(w => w.Enable).OrderBy(x=>x.Order).ToArray();
            }
        }

        private DateTimeOffset _StartAccelerateTime;

        private TimeSpan _AccelerateTime;
        public TimeSpan AccelerateTime
        {
            get => _AccelerateTime;
            set => this.RaiseAndSetIfChanged(ref _AccelerateTime, value);
        }

        public bool IsEnableScript
        {
            get => ProxySettings.IsEnableScript.Value;
            set
            {
                ProxySettings.IsEnableScript.Value = value;
                httpProxyService.IsEnableScript = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(EnableProxyScripts));
            }
        }

        public bool IsOnlyWorkSteamBrowser
        {
            get => ProxySettings.IsOnlyWorkSteamBrowser.Value;
            set
            {
                if (ProxySettings.IsOnlyWorkSteamBrowser.Value != value)
                {
                    ProxySettings.IsOnlyWorkSteamBrowser.Value = value;
                    httpProxyService.IsOnlyWorkSteamBrowser = value;
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

        #endregion

        #region 代理状态启动退出
        private bool _ProxyStatus;
        public bool ProxyStatus
        {
            get { return _ProxyStatus; }
            set => this.RaiseAndSetIfChanged(ref _ProxyStatus, value);
        }
        #endregion

        public async Task Initialize()
        {
            //httpProxyService.StopProxy();
            await InitializeAccelerate();
            await InitializeScript();

            if (ProxySettings.ProgramStartupRunProxy.Value)
            {
                ProxyStatus = ProxySettings.ProgramStartupRunProxy.Value;
            }
        }

        public async Task InitializeAccelerate()
        {
            #region 加载代理服务数据
            var client = ICloudServiceClient.Instance.Accelerate;
#if DEBUG
            var stopwatch = Stopwatch.StartNew();
#endif
            var result = await client.All();
#if DEBUG
            stopwatch.Stop();
            Toast.Show($"加载代理服务数据耗时：{stopwatch.ElapsedMilliseconds}ms，IsSuccess：{result.IsSuccess}，Count：{result.Content?.Count}");
#endif
            if (result.IsSuccess)
            {
                if (ProxySettings.SupportProxyServicesStatus.Value.Any_Nullable() && result.Content.Any_Nullable())
                {
                    var items = result.Content!.SelectMany(s => s.Items);
                    foreach (var item in items)
                    {
                        if (ProxySettings.SupportProxyServicesStatus.Value!.Contains(item.Id.ToString()))
                        {
                            item.Enable = true;
                        }
                    }
                }

                ProxyDomains.Clear();
                ProxyDomains.AddRange(result.Content!);
            }

            LoadOrSaveLocalAccelerate();

            if (ProxyDomains.Items.Any_Nullable())
            {
                foreach (var item in ProxyDomains.Items)
                {
                    item.ImageStream = IHttpService.Instance.GetImageAsync(ImageUrlHelper.GetImageApiUrlById(item.ImageId), ImageChannelType.AccelerateGroup);
                }
            }

            this.WhenAnyValue(v => v.ProxyDomainsList)
                  .Subscribe(domain => domain?
                  .ToObservableChangeSet()
                  .AutoRefresh(x => x.ObservableItems)
                  .TransformMany(t => t.ObservableItems ?? new ObservableCollection<AccelerateProjectDTO>())
                  .AutoRefresh(x => x.Enable)
                  .WhenPropertyChanged(x => x.Enable, false)
                  .Subscribe(_ =>
                  {
                      if (EnableProxyDomains != null)
                      {
                          ProxySettings.SupportProxyServicesStatus.Value = EnableProxyDomains.Select(k => k.Id.ToString()).ToList();
                      }
                  }));
            #endregion
        }

        public async Task InitializeScript()
        {
            #region 加载脚本数据

            //var response =// await client.Scripts();
            //if (!response.IsSuccess)
            //{
            //    return;
            //}
            //new ObservableCollection<ScriptDTO>(response.Content);
            var scriptList = await scriptManager.GetAllScriptAsync();
            if (ProxySettings.ScriptsStatus.Value.Any_Nullable() && scriptList.Any())
            {
                foreach (var item in scriptList)
                {
                    if (item.LocalId > 0 && ProxySettings.ScriptsStatus.Value!.Contains(item.LocalId))
                    {
                        item.Enable = true;
                    }
                }
            }

            ProxyScripts.AddRange(scriptList);
            BasicsInfo();
            httpProxyService.IsEnableScript = IsEnableScript;

            this.WhenAnyValue(v => v.ProxyScripts)
                  .Subscribe(script => script?
                  .Connect()
                  .AutoRefresh(x => x.Enable)
                  .WhenPropertyChanged(x => x.Enable, false)
                  .Subscribe(_ =>
                  {
                      //ProxySettings.ScriptsStatus.Value = EnableProxyScripts?.Where(w => w?.LocalId > 0).Select(k => k.LocalId).ToList();
                      ProxySettings.ScriptsStatus.Value = ProxyScripts.Items.Where(x => x?.LocalId > 0).Select(k => k.LocalId).ToList();
                      httpProxyService.Scripts = EnableProxyScripts;
                      this.RaisePropertyChanged(nameof(EnableProxyScripts));
                  }));
            #endregion
        }

        private void LoadOrSaveLocalAccelerate()
        {
            var filepath = Path.Combine(IOPath.AppDataDirectory, "LOCAL_ACCELERATE.json");
            if (ProxyDomains.Items.Any_Nullable())
            {
                if (IOPath.TryOpen(filepath, FileMode.Create, FileAccess.Write, FileShare.Read, out var fileStream, out var _))
                {
                    using var stream = fileStream;
                    using var writer = new StreamWriter(stream, Encoding.UTF8);
                    var content = Serializable.SMPB64U(ProxyDomains.Items);
                    writer.Write(content);
                    writer.Flush();
                }
            }
            else
            {
                if (File.Exists(filepath) && IOPath.TryOpenRead(filepath, out var fileStream, out var _))
                {
                    using var stream = fileStream;
                    ProxyDomains.Clear();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    var content = reader.ReadToEnd();
                    List<AccelerateProjectGroupDTO>? accelerates = null;
                    try
                    {
                        accelerates = Serializable.DMPB64U<List<AccelerateProjectGroupDTO>>(content);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(ProxyService), ex, nameof(LoadOrSaveLocalAccelerate));
                    }
                    if (accelerates.Any_Nullable())
                        ProxyDomains.AddRange(accelerates!);
                }
            }
        }

        public async void BasicsInfo()
        {
            var basicsItem = ProxyScripts.Items.FirstOrDefault(x => x.Id == Guid.Parse("00000000-0000-0000-0000-000000000001"));
            if (basicsItem == null)
            {
                var basicsInfo = await ICloudServiceClient.Instance.Script.Basics(AppResources.Script_UpdateError);
                if (basicsInfo.Code == ApiResponseCode.OK && basicsInfo.Content != null)
                {
                    var jspath = await scriptManager.DownloadScriptAsync(basicsInfo.Content.UpdateLink);
                    if (jspath.IsSuccess)
                    {
                        var build = await scriptManager.AddScriptAsync(jspath.Content!, build: false, order: 1, deleteFile: true, pid: basicsInfo.Content.Id, ignoreCache: true);
                        if (build.IsSuccess)
                        {
                            if (build.Content != null)
                            {
                                build.Content.IsBasics = true;
                                ProxyScripts.Insert(0, build.Content);
                            }
                        }
                    }
                }
            }
        }

        private Timer? timer;

        public void StartTimer()
        {
            if (timer == null)
            {
                timer = new Timer((state) =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    AccelerateTime = DateTimeOffset.Now - _StartAccelerateTime;
                }, nameof(AccelerateTime), 1000, 1000);
            }
        }

        public void StopTimer()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        public static void OnExitRestoreHosts()
        {
            var s = DI.Get_Nullable<IHostsFileService>();
            if (s != null)
            {
                var needClear = s.ContainsHostsByTag();
                if (needClear)
                {
                    s.OnExitRestoreHosts();
                }
            }
        }

        public async Task AddNewScript(string filename)
        {
            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                ScriptDTO.TryParse(filename, out ScriptDTO? info);
                if (info != null)
                {
                    var scriptItem = ProxyScripts.Items.FirstOrDefault(x => x.Name == info.Name);
                    if (scriptItem != null)
                    {
                        var result = MessageBox.ShowAsync(AppResources.Script_ReplaceTips, button: MessageBox.Button.OKCancel).ContinueWith(async (s) =>
                         {
                             if (s.Result == MessageBox.Result.OK)
                             {
                                 await AddNewScript(fileInfo, info, scriptItem);
                             }
                         });
                    }
                    else
                    {
                        await AddNewScript(fileInfo, info);
                    }
                }
                else
                {
                    await AddNewScript(fileInfo, info);
                }
            }
            else
            {
                var msg = AppResources.Script_FileError.Format(filename);// $"文件不存在:{filePath}";
                Toast.Show(msg);
            }
        }

        public async Task AddNewScript(FileInfo fileInfo, ScriptDTO? info, ScriptDTO? oldInfo = null)
        {
            IsLoading = true;
            bool isbuild = true;
            int order = 10;
            if (oldInfo != null)
            {
                isbuild = oldInfo.IsBuild;
                order = oldInfo.Order;
            }
            var item = await scriptManager.AddScriptAsync(fileInfo, info, oldInfo, build: isbuild, order: order);
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
            IsLoading = false;
            Toast.Show(item.Message);
        }

        public async void RefreshScript()
        {
            var scriptList = await scriptManager.GetAllScriptAsync();
            ProxyScripts.Clear();
            if (ProxySettings.ScriptsStatus.Value.Any_Nullable() && scriptList.Any())
            {
                foreach (var item in scriptList)
                {
                    if (ProxySettings.ScriptsStatus.Value!.Contains(item.LocalId))
                    {
                        item.Enable = true;
                    }
                }
            }
            ProxyScripts.AddRange(scriptList);

            CheckUpdate();
        }

        public async void DownloadScript(ScriptDTO model)
        {
            model.IsLoading = true;
            var jspath = await scriptManager.DownloadScriptAsync(model.UpdateLink);
            if (jspath.IsSuccess)
            {
                var build = await scriptManager.AddScriptAsync(jspath.Content!, model, build: model.IsBuild, order: model.Order, deleteFile: true, pid: model.Id);
                if (build.IsSuccess)
                {
                    if (build.Content != null)
                    {
                        var basicsItem = Current.ProxyScripts.Items.FirstOrDefault(x => x.Id == model.Id);
                        if (basicsItem != null)
                        {
                            var index = Current.ProxyScripts.Items.IndexOf(basicsItem);
                            Current.ProxyScripts.ReplaceAt(index, basicsItem);
                        }
                        else
                        {
                            Current.ProxyScripts.Add(build.Content);
                        }
                        model.IsUpdate = false;
                        model.IsExist = true;
                        model.UpdateLink = build.Content.UpdateLink;
                        model.FilePath = build.Content.FilePath;
                        model.Version = build.Content.Version;
                        model.Name = build.Content.Name;
                        RefreshScript();
                        Toast.Show(AppResources.Download_ScriptOk);
                    }
                }
                else
                {
                    Toast.Show(build.Message);
                }
            }
            else
            {
                Toast.Show(jspath.GetMessageByFormat(AppResources.Download_ScriptError_));
            }
            model.IsLoading = false;
        }

        public async void CheckUpdate()
        {
            var items = Current.ProxyScripts.Items.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
            var client = ICloudServiceClient.Instance.Script;
            var response = await client.ScriptUpdateInfo(items, AppResources.Script_UpdateError);
            if (response.Code == ApiResponseCode.OK && response.Content != null)
            {
                foreach (var item in Current.ProxyScripts.Items)//response.Content)
                {
                    var newItem = response.Content.FirstOrDefault(x => x.Id == item.Id);
                    if (newItem != null && item.Version != newItem.Version)
                    {
                        item.NewVersion = newItem.Version;
                        item.UpdateLink = newItem.UpdateLink;
                        item.IsUpdate = true;
                        Current.ProxyScripts.Replace(item, item);
                    }
                }
            }
        }

        public void FixNetwork()
        {
            OnExitRestoreHosts();

            if (OperatingSystem2.IsWindows)
            {
                httpProxyService.StopProxy();
                Process.Start("cmd.exe", "netsh winsock reset");
            }

            Toast.Show(AppResources.FixNetworkComplete);
        }

        public void Dispose()
        {
            httpProxyService.StopProxy();
            OnExitRestoreHosts();
            httpProxyService.Dispose();
        }
    }
}
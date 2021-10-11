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
using System.Properties;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Models;
using Interface = System.Application.Services.ProxyService;
using BaseClass = System.Application.Services.Abstractions.ProxyService;
using static System.Application.Services.ProxyService;

namespace System.Application.Services
{
#pragma warning disable IDE1006 // 命名样式
    internal interface ProxyService
#pragma warning restore IDE1006 // 命名样式
    {
        static BaseClass Current => mCurrent is BaseClass i ? i : throw new NullReferenceException("ProxyService is null.");

        static BaseClass? mCurrent;
    }
}

namespace System.Application.Services.Abstractions
{
    public abstract class ProxyService : ReactiveObject, Interface
    {
        protected readonly IHttpProxyService httpProxyService = DI.Get<IHttpProxyService>();
        protected readonly IScriptManager scriptManager = DI.Get<IScriptManager>();

        public ProxyService()
        {
            ProxyDomains = new SourceList<AccelerateProjectGroupDTO>();
            ProxyScripts = new SourceList<ScriptDTO>();

            this.ProxyDomains
                     .Connect()
                     //.Filter(scriptFilter)
                     .ObserveOn(RxApp.MainThreadScheduler)
                     .Sort(SortExpressionComparer<AccelerateProjectGroupDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
                     .Bind(out _ProxyDomainsList)
                     .Subscribe(_ => SelectGroup = ProxyDomains.Items.FirstOrDefault());
        }

        public SourceList<AccelerateProjectGroupDTO> ProxyDomains { get; }

        private ReadOnlyObservableCollection<AccelerateProjectGroupDTO>? _ProxyDomainsList;
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
                if (!IsEnableScript)
                    return null;
                if (!ProxyScripts.Items.Any_Nullable())
                    return null;
                return ProxyScripts.Items!.Where(w => w.Enable).ToArray();
            }
        }

        private DateTimeOffset _StartAccelerateTime;

        private DateTimeOffset _AccelerateTime;
        public DateTimeOffset AccelerateTime
        {
            get => _AccelerateTime;
            set
            {
                if (_AccelerateTime != value)
                {
                    _AccelerateTime = value;
                    this.RaisePropertyChanged();
                }
            }
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

        #region HOSTS_PROXY_RUNNING_STATUS

        //const string KEY_HOSTS_PROXY_RUNNING_STATUS = "KEY_HOSTS_PROXY_RUNNING_STATUS";
        //static async void SaveHostsProxyStatus(bool value)
        //{
        //    await IStorage.Instance.SetAsync<bool>(KEY_HOSTS_PROXY_RUNNING_STATUS, value);
        //}

        //public static async Task<bool> GetHostsProxyStatusAsync()
        //{
        //    var r = await IStorage.Instance.GetAsync<bool>(KEY_HOSTS_PROXY_RUNNING_STATUS);
        //    return r;
        //}

        #endregion

        protected virtual void ConfigProxyService()
        {

        }

        protected virtual bool EnableSystemProxy => true;

        protected virtual void OnHostProxy()
        {

        }

        protected virtual void OnRunHostProxy(IEnumerable<(string, string)> hosts)
        {

        }

        protected virtual void OnStopRemoveHostsByTag()
        {

        }

        #region 代理状态启动退出
        public bool ProxyStatus
        {
            get { return httpProxyService.ProxyRunning; }
            set
            {
                if (value != httpProxyService.ProxyRunning)
                {
                    if (value)
                    {
                        //if (EnableProxyDomains.Any_Nullable())
                        //{
                        //Toast.Show(AppResources.CommunityFix_NoSelectAcceleration);
                        //return;
                        //httpProxyService.ProxyDomains = EnableProxyDomains;
                        //}
                        httpProxyService.ProxyDomains = EnableProxyDomains;
                        httpProxyService.Scripts = EnableProxyScripts;
                        httpProxyService.IsEnableScript = ProxySettings.IsEnableScript.Value;
                        httpProxyService.IsSystemProxy = EnableSystemProxy;

                        // macOS 上目前因权限问题仅支持 0.0.0.0(IPAddress.Any)
                        httpProxyService.ProxyIp = (!OperatingSystem2.IsMacOS && IPAddress2.TryParse(ProxySettings.SystemProxyIp.Value, out var ip)) ? ip : IPAddress.Any;

                        httpProxyService.Socks5ProxyEnable = ProxySettings.Socks5ProxyEnable.Value;
                        httpProxyService.Socks5ProxyPortId = ProxySettings.Socks5ProxyPortId.Value;

                        httpProxyService.TwoLevelAgentEnable = ProxySettings.TwoLevelAgentEnable.Value;

                        httpProxyService.TwoLevelAgentProxyType = (ExternalProxyType)ProxySettings.TwoLevelAgentProxyType.Value;
                        if (!httpProxyService.TwoLevelAgentProxyType.IsDefined()) httpProxyService.TwoLevelAgentProxyType = IHttpProxyService.DefaultTwoLevelAgentProxyType;

                        httpProxyService.TwoLevelAgentIp = IPAddress2.TryParse(ProxySettings.TwoLevelAgentIp.Value, out var ip_t) ? ip_t.ToString() : IPAddress.Loopback.ToString();
                        httpProxyService.TwoLevelAgentPortId = ProxySettings.TwoLevelAgentPortId.Value;
                        httpProxyService.TwoLevelAgentUserName = ProxySettings.TwoLevelAgentUserName.Value;
                        httpProxyService.TwoLevelAgentPassword = ProxySettings.TwoLevelAgentPassword.Value;

                        ConfigProxyService();

                        this.RaisePropertyChanged(nameof(EnableProxyDomains));
                        this.RaisePropertyChanged(nameof(EnableProxyScripts));

                        if (!httpProxyService.IsSystemProxy)
                        {
                            OnHostProxy();
                        }

                        var isRun = httpProxyService.StartProxy();

                        if (isRun)
                        {
                            if (!httpProxyService.IsSystemProxy)
                            {
                                if (httpProxyService.ProxyDomains.Any_Nullable())
                                {
                                    var hosts = httpProxyService.ProxyDomains!.SelectMany(s =>
                                    {
                                        if (s == null) return default!;
                                        return s.HostsArray.Select(host =>
                                        {
                                            if (host.Contains(' '))
                                            {
                                                var h = host.Split(' ');
                                                return (h[0], h[1]);
                                            }
                                            return (IPAddress.Loopback.ToString(), host);
                                        });
                                    }).Where(w => !string.IsNullOrEmpty(w.Item1));

                                    OnRunHostProxy(hosts);
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
                        OnStopRemoveHostsByTag();
                        //Toast.Show(SteamTools.Properties.Resources.ProxyStop);
                    }
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        public async void Initialize()
        {
            //httpProxyService.StopProxy();

            await InitializeAccelerate();
            await InitializeScript();
            if (ProxySettings.ProgramStartupRunProxy.Value)
            {
                ProxyStatus = true;
            }
        }

        public async Task InitializeAccelerate()
        {
            #region 加载代理服务数据
            var client = ICloudServiceClient.Instance.Accelerate;
            var result = await client.All();
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
                      ProxySettings.ScriptsStatus.Value = EnableProxyScripts?.Where(w => w?.LocalId > 0).Select(k => k.LocalId).ToList();
                      httpProxyService.Scripts = EnableProxyScripts;
                      this.RaisePropertyChanged(nameof(EnableProxyScripts));
                  }));
            #endregion
        }

        private void LoadOrSaveLocalAccelerate()
        {
            // https://appcenter.ms/orgs/BeyondDimension/apps/Steam/crashes/errors/1815188879u/overview
            // FileStreamHelpers.ValidateFileHandle (SafeFileHandle fileHandle, String path, Boolean useAsyncIO)
            // System.Private.CoreLib.dll:token 0x6005b56+0x, line 27
            // System.IO.IOException: IO_SharingViolation_File, \AppData\LOCAL_ACCELERATE.json
            var filepath = Path.Combine(IOPath.AppDataDirectory, "LOCAL_ACCELERATE.json");
            if (ProxyDomains.Items.Any_Nullable())
            {
                if (IOPath.TryOpen(filepath, FileMode.Create, FileAccess.Write, FileShare.Read, out var fileStream, out var _))
                {
                    using var stream = fileStream;
                    using var writer = new StreamWriter(stream, Encoding.UTF8);
                    var content = Serializable.SJSON_Original(ProxyDomains.Items);
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
                        accelerates = Serializable.DJSON_Original<List<AccelerateProjectGroupDTO>>(content);
                    }
                    catch
                    {
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
            AccelerateTime = new DateTimeOffset().Add((DateTimeOffset.Now - _StartAccelerateTime));

            if (timer == null)
            {
                timer = new Timer((state) =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    AccelerateTime = AccelerateTime.AddSeconds(1);
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

        protected virtual void OnExitRestoreHostsCore()
        {

        }

        public static void OnExitRestoreHosts() => Current.OnExitRestoreHostsCore();

        public void Dispose()
        {
            httpProxyService.StopProxy();
            OnExitRestoreHostsCore();
            httpProxyService.Dispose();
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
                        var result = MessageBox.ShowAsync(@AppResources.Script_ReplaceTips, ThisAssembly.AssemblyTrademark, MessageBox.Button.OKCancel).ContinueWith(async (s) =>
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
    }

    public abstract class ProxyService<T> : BaseClass, Interface where T : ProxyService<T>, new()
    {
        public static T Current
        {
            get => ((T)Interface.Current)!;
            protected set => Interface.mCurrent = value;
        }

        static ProxyService()
        {
            Current = new();
        }
    }
}
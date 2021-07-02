using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Properties;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Properties;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services
{
    public class ProxyService : ReactiveObject
    {
        public static ProxyService Current { get; } = new ProxyService();
        readonly IHttpProxyService httpProxyService = DI.Get<IHttpProxyService>();

        public ProxyService()
        {
            ProxyDomains = new SourceList<AccelerateProjectGroupDTO>();
            ProxyScripts = new SourceList<ScriptDTO>();
            httpProxyService.CertificateEngine = Titanium.Web.Proxy.Network.CertificateEngine.BouncyCastle;

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
                return ProxyDomains.Items.SelectMany(s =>
                {
                    return s.Items.Where(w => w.Enable);
                }).ToArray();
            }
        }

        public IReadOnlyCollection<ScriptDTO>? EnableProxyScripts
        {
            get
            {
                if (!IsEnableScript)
                    return null;
                if (!ProxyScripts.Items.Any())
                    return null;
                return ProxyScripts.Items.Where(w => w.Enable).ToArray();
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
        //    await IStorage.Instance.SetAsync<bool>(KEY_HOSTS_PROXY_RUNNING_STATUS, value);
        //}

        //public static async Task<bool> GetHostsProxyStatusAsync()
        //{
        //    var r = await IStorage.Instance.GetAsync<bool>(KEY_HOSTS_PROXY_RUNNING_STATUS);
        //    return r;
        //}

        #endregion

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
                        httpProxyService.IsOnlyWorkSteamBrowser = ProxySettings.IsOnlyWorkSteamBrowser.Value;
                        this.RaisePropertyChanged(nameof(EnableProxyDomains));
                        this.RaisePropertyChanged(nameof(EnableProxyScripts));

                        if (!ProxySettings.EnableWindowsProxy.Value)
                        {
                            //if (DI.Platform == Platform.Windows)
                            //{
                            //    var inUse = httpProxyService.PortInUse(443);
                            //    if (inUse)
                            //    {
                            //        var p = DI.Get<IDesktopPlatformService>().GetProcessByPortOccupy(443, true);
                            //        if (p != null)
                            //        {
                            //            Toast.Show(string.Format(AppResources.CommunityFix_StartProxyFaild443, p.ProcessName));
                            //            return;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            var inUse = httpProxyService.PortInUse(443);
                            if (inUse)
                            {
                                Toast.Show(string.Format(AppResources.CommunityFix_StartProxyFaild443, ""));
                                return;
                            }
                            //}
                        }

                        var isRun = httpProxyService.StartProxy(ProxySettings.EnableWindowsProxy.Value, ProxySettings.IsProxyGOG.Value);

                        if (isRun)
                        {
                            if (!ProxySettings.EnableWindowsProxy.Value)
                            {
                                if (httpProxyService.ProxyDomains.Any_Nullable())
                                {
                                    var hosts = httpProxyService.ProxyDomains!.SelectMany(s =>
                                    {
                                        if (s == null) return default!;
                                        return s.HostsArray.Select(host =>
                                        {
                                            if (host.Contains(" "))
                                            {
                                                var h = host.Split(' ');
                                                return (h[0], h[1]);
                                            }
                                            return (IPAddress.Loopback.ToString(), host);
                                        });
                                    }).Where(w => !string.IsNullOrEmpty(w.Item1));
                                    if (DI.Platform == Platform.Windows)
                                    {
                                        var r = IHostsFileService.Instance.UpdateHosts(hosts);
                                        if (r.ResultType != OperationResultType.Success)
                                        {
                                            Toast.Show(SR.OperationHostsError_.Format(r.Message));
                                            httpProxyService.StopProxy();
                                            return;
                                        }
                                    }
                                }
                            }
                            _StartAccelerateTime = DateTimeOffset.Now;
                            StartTimer();
                            Toast.Show(AppResources.CommunityFix_StartProxySuccess);
                        }
                        else
                        {
                            MessageBoxCompat.Show(AppResources.CommunityFix_StartProxyFaild);
                        }
                    }
                    else
                    {
                        httpProxyService.StopProxy();
                        StopTimer();
                        void OnStopRemoveHostsByTag()
                        {
                            var needClear = IHostsFileService.Instance.ContainsHostsByTag();
                            if (needClear)
                            {
                                var r = IHostsFileService.Instance.RemoveHostsByTag();
                                if (r.ResultType != OperationResultType.Success)
                                {
                                    Toast.Show(SR.OperationHostsError_.Format(r.Message));
                                    //return;
                                }
                            }
                        }
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
            await InitializeAccelerate();
            await InitializeScript();
            if (ProxySettings.ProgramStartupRunProxy.Value)
            {
                ProxyService.Current.ProxyStatus = true;
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
                ProxyDomains.AddRange(result.Content);
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
            var scriptList = await DI.Get<IScriptManagerService>().GetAllScript();
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
                    var jspath = await DI.Get<IScriptManagerService>().DownloadScript(basicsInfo.Content.UpdateLink);
                    if (jspath.state)
                    {
                        var build = await DI.Get<IScriptManagerService>().AddScriptAsync(jspath.path, build: false, order: 1, deleteFile: true, pid: basicsInfo.Content.Id, ignoreCache: true);
                        if (build.state)
                        {
                            if (build.model != null)
                            {
                                build.model.IsBasics = true;
                                ProxyScripts.Insert(0, build.model);
                            }
                        }
                    }
                }
            }
        }

        private Timer timer;

        public void StartTimer()
        {
            AccelerateTime = new DateTimeOffset().Add((DateTimeOffset.Now - _StartAccelerateTime));
            timer = new Timer((state) =>
            {
                Thread.CurrentThread.IsBackground = true;
                AccelerateTime = AccelerateTime.AddSeconds(1);
            }, nameof(AccelerateTime), 1000, 1000);
        }

        public void StopTimer()
        {
            timer.Dispose();
        }

        public static void OnExitRestoreHosts()
        {
            var needClear = IHostsFileService.Instance.ContainsHostsByTag();
            if (needClear)
            {
                IHostsFileService.OnExitRestoreHosts();
            }
        }

        public void Dispose()
        {
            httpProxyService.StopProxy();
            OnExitRestoreHosts();
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
                        var result = MessageBoxCompat.ShowAsync(@AppResources.Script_ReplaceTips, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
                        {
                            if (s.Result == MessageBoxResultCompat.OK)
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
            var item = await DI.Get<IScriptManagerService>().AddScriptAsync(fileInfo, info, oldInfo, build: isbuild, order: order);
            if (item.state)
            {
                if (item.model != null)
                    if (oldInfo == null)
                        ProxyScripts.Add(item.model);
                    else
                        ProxyScripts.Replace(oldInfo, item.model);
            }
            IsLoading = false;
            Toast.Show(item.msg);
        }

        public async void RefreshScript()
        {
            var scriptList = await DI.Get<IScriptManagerService>().GetAllScript();
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
            var jspath = await DI.Get<IScriptManagerService>().DownloadScript(model.UpdateLink);
            if (jspath.state)
            {
                var build = await DI.Get<IScriptManagerService>().AddScriptAsync(jspath.path, model, build: model.IsBuild, order: model.Order, deleteFile: true, pid: model.Id);
                if (build.state)
                {
                    if (build.model != null)
                    {
                        var basicsItem = Current.ProxyScripts.Items.FirstOrDefault(x => x.Id == model.Id);
                        if (basicsItem != null)
                        {
                            var index = Current.ProxyScripts.Items.IndexOf(basicsItem);
                            Current.ProxyScripts.ReplaceAt(index, basicsItem);
                        }
                        else
                        {
                            Current.ProxyScripts.Add(build.model);
                        }
                        model.IsUpdate = false;
                        model.IsExist = true;
                        model.UpdateLink = build.model.UpdateLink;
                        model.FilePath = build.model.FilePath;
                        model.Version = build.model.Version;
                        model.Name = build.model.Name;
                        RefreshScript();
                        Toast.Show(AppResources.Download_ScriptOk);
                    }
                }
                else
                    Toast.Show(build.msg);
            }
            else
                Toast.Show(AppResources.Download_ScriptError);
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
            ProxyService.OnExitRestoreHosts();

            if (DI.Platform == Platform.Windows)
            {
                httpProxyService.StopProxy();
                System.Diagnostics.Process.Start("cmd.exe", "netsh winsock reset");
            }

            Toast.Show("修复网络完成");
        }
    }
}
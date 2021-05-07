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
            ProxyScripts = new SourceList<ScriptDTO>();
            httpProxyService.CertificateEngine = Titanium.Web.Proxy.Network.CertificateEngine.BouncyCastle;
        }

        private ReadOnlyObservableCollection<AccelerateProjectGroupDTO>? _ProxyDomains;
        public ReadOnlyObservableCollection<AccelerateProjectGroupDTO>? ProxyDomains
        {
            get => _ProxyDomains;
            set
            {
                if (_ProxyDomains != value)
                {
                    _ProxyDomains = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        bool _IsLoading = false;
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
                if (!ProxyDomains.Any_Nullable())
                    return null;
                return ProxyDomains!.SelectMany(s =>
                {
                    return s.Items.Where(w => w.Enable);
                }).ToArray();
            }
        }

        public IReadOnlyCollection<ScriptDTO>? EnableProxyScripts
        {
            get
            {
                if (!ProxyScripts.Items.Any())
                    return null;
                return ProxyScripts.Items.Where(w => w.Enable).ToArray();
            }
        }

        private DateTime _AccelerateTime = new();
        public DateTime AccelerateTime
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
                        if (EnableProxyDomains.Any_Nullable())
                        {
                            //Toast.Show(AppResources.CommunityFix_NoSelectAcceleration);
                            //return;
                            httpProxyService.ProxyDomains = EnableProxyDomains;
                            this.RaisePropertyChanged(nameof(EnableProxyDomains));
                        }
                        httpProxyService.ProxyDomains = EnableProxyDomains;
                        httpProxyService.Scripts = EnableProxyScripts;
                        httpProxyService.IsEnableScript = ProxySettings.IsEnableScript.Value;
                        httpProxyService.IsOnlyWorkSteamBrowser = ProxySettings.IsOnlyWorkSteamBrowser.Value;
                        this.RaisePropertyChanged(nameof(EnableProxyScripts));

                        if (!ProxySettings.EnableWindowsProxy.Value)
                        {
                            if (httpProxyService.PortInUse(443))
                            {
                                Toast.Show(AppResources.CommunityFix_StartProxyFaild443);
                                return;
                            }
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

                                    var r = IHostsFileService.Instance.UpdateHosts(hosts);
                                    if (r.ResultType != OperationResultType.Success)
                                    {
                                        Toast.Show(SR.OperationHostsError_.Format(r.Message));
                                        httpProxyService.StopProxy();
                                        return;
                                    }
                                }
                            }
                            StartTiming();
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
                        var r = IHostsFileService.Instance.RemoveHostsByTag();
                        if (r.ResultType != OperationResultType.Success)
                        {
                            Toast.Show(SR.OperationHostsError_.Format(r.Message));
                            //return;
                        }
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

                ProxyDomains = new ReadOnlyObservableCollection<AccelerateProjectGroupDTO>(new ObservableCollection<AccelerateProjectGroupDTO>(result.Content!));

                foreach (var item in ProxyDomains)
                {
                    item.ImageStream = IHttpService.Instance.GetImageAsync(ImageUrlHelper.GetImageApiUrlById(item.ImageId), ImageChannelType.AccelerateGroup);
                }

                SelectGroup = ProxyDomains.FirstOrDefault();

                this.WhenAnyValue(v => v.ProxyDomains)
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
            }
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

        public void StartTiming()
        {
            AccelerateTime = new();
            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (ProxyStatus)
                {
                    AccelerateTime = AccelerateTime.AddSeconds(1);
                    Thread.Sleep(1000);
                }
            });
        }

        public void Dispose()
        {
            httpProxyService.StopProxy();
            IHostsFileService.OnExitRestoreHosts();
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
    }
}
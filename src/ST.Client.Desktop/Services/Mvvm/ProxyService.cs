using ReactiveUI;
using DynamicData;
using DynamicData.Binding;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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

        private ObservableCollection<AccelerateProjectGroupDTO>? _ProxyDomains;
        public ObservableCollection<AccelerateProjectGroupDTO>? ProxyDomains
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

        private AccelerateProjectGroupDTO? _SelectGroup;
        public AccelerateProjectGroupDTO? SelectGroup
        {
            get => _SelectGroup;
            set => this.RaiseAndSetIfChanged(ref _SelectGroup, value);
        }

        private ICollection<ScriptDTO>? _ProxyScripts;
        public ICollection<ScriptDTO>? ProxyScripts
        {
            get => _ProxyScripts;
            set
            {
                if (_ProxyScripts != value)
                {
                    _ProxyScripts = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public IReadOnlyCollection<AccelerateProjectDTO>? EnableProxyDomains
        {
            get
            {
                if (!ProxyDomains.Any_Nullable())
                    return null;
                return ProxyDomains.SelectMany(s =>
                {
                    return s.Items.Where(w => w.Enable);
                }).ToArray();
            }
        }

        public IReadOnlyCollection<ScriptDTO>? EnableProxyScripts
        {
            get
            {
                if (!ProxyScripts.Any_Nullable())
                    return null;
                return ProxyScripts.Where(w => w.Enable).ToArray();
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
                if (ProxySettings.IsEnableScript.Value != value)
                {
                    ProxySettings.IsEnableScript.Value = value;
                    httpProxyService.IsEnableScript = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(EnableProxyScripts));
                }
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
                        if (!EnableProxyDomains.Any_Nullable())
                        {
                            Toast.Show(AppResources.CommunityFix_NoSelectAcceleration);
                            return;
                        }
                        httpProxyService.ProxyDomains = EnableProxyDomains;
                        httpProxyService.Scripts = EnableProxyScripts;
                        this.RaisePropertyChanged(nameof(EnableProxyDomains));
                        var hosts = httpProxyService.ProxyDomains.SelectMany(s =>
                        {
                            return s?.HostsArray.Select(host =>
                            {
                                if (host.Contains(" "))
                                {
                                    var h = host.Split(' ');
                                    return (h[0], h[1]);
                                }
                                return (IPAddress.Loopback.ToString(), host);
                            });
                        }).Where(w => !string.IsNullOrEmpty(w.Item1));

                        var isRun = httpProxyService.StartProxy(ProxySettings.EnableWindowsProxy.Value, ProxySettings.IsProxyGOG.Value);

                        if (isRun)
                        {
                            StartTiming();
                            IHostsFileService.Instance.UpdateHosts(hosts);
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
                        IHostsFileService.Instance.RemoveHostsByTag();
                        //Toast.Show(SteamTools.Properties.Resources.ProxyStop);
                    }
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        public async void Initialize()
        {
            #region 加载代理服务数据
            var client = ICloudServiceClient.Instance.Accelerate;
            var result = await client.All();
            if (!result.IsSuccess)
            {
                return;
            }
            ProxyDomains = new ObservableCollection<AccelerateProjectGroupDTO>(result.Content);
            SelectGroup = ProxyDomains.FirstOrDefault();

            if (ProxySettings.SupportProxyServicesStatus.Value.Any_Nullable() && ProxyDomains.Any_Nullable())
            {
                var items = ProxyDomains.SelectMany(s => s.Items);
                foreach (var item in items)
                {
                    if (ProxySettings.SupportProxyServicesStatus.Value.Contains(item.Id.ToString()))
                    {
                        item.Enable = true;
                    }
                }
            }

            this.WhenValueChanged(v => v.ProxyDomains, false)
                  .DistinctUntilChanged()
                  .Subscribe(domain => domain?
                        .ToObservableChangeSet()
                        .AutoRefresh(x => x.ObservableItems)
                        .TransformMany(t => t.ObservableItems ?? new ObservableCollection<AccelerateProjectDTO>())
                        .AutoRefresh(x => x.Enable)
                        .Subscribe(_ =>
                        {
                            if (EnableProxyDomains != null)
                            {
                                ProxySettings.SupportProxyServicesStatus.Value = EnableProxyDomains.Where(w => w?.Id != null).Select(k => k.Id.ToString()).ToList();
                            }
                        }));
            #endregion


            #region 加载脚本数据
            var response = await client.Scripts();
            if (!response.IsSuccess)
            {
                return;
            }
            ProxyScripts = new ObservableCollection<ScriptDTO>(response.Content);

            #endregion
        }

        public void StartTiming()
        {
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
            IHostsFileService.OnExitRestoreHosts();
            httpProxyService.Dispose();
        }

        public void AddNewScript(string filename)
        {

        }
    }
}

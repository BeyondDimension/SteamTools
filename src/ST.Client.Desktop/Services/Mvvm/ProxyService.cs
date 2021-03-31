using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services
{
    public class ProxyService : ReactiveObject
    {
        public static ProxyService Current { get; } = new ProxyService();
        readonly IHttpProxyService httpProxyService = DI.Get<IHttpProxyService>();

        private IReadOnlyCollection<AccelerateProjectGroupDTO>? _ProxyDomains;
        public IReadOnlyCollection<AccelerateProjectGroupDTO>? ProxyDomains
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

        private IList<ScriptDTO>? _ProxyScripts;
        public IList<ScriptDTO>? ProxyScripts
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

        public IReadOnlyCollection<AccelerateProjectDTO?>? EnableProxyDomains
        {
            get 
            {
                if (!ProxyDomains.Any_Nullable())
                    return null;
                return ProxyDomains.Select(s =>
                {
                    foreach (var item in s.Items)
                    {
                        if (item.Enable)
                            return item;
                    }
                    return null;
                }).Where(w => w != null).ToArray();
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

        #region 代理状态变更通知
        public bool ProxyStatus
        {
            get { return httpProxyService.ProxyRunning; }
            set
            {
                if (value != httpProxyService.ProxyRunning)
                {
                    if (value)
                    {
                        httpProxyService.ProxyDomains = EnableProxyDomains;
                        //var hosts = httpProxyService.ProxyDomains.Select();
                        var isRun = httpProxyService.StartProxy(ProxySettings.IsProxyGOG.Value, ProxySettings.EnableWindowsProxy.Value);
                        if (isRun)
                        {
                            StartTiming();
                            //IHostsFileService.Instance.UpdateHosts();
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
            if (!result.IsSuccess || !result.Content.Any_Nullable())
            {
                return;
            }
            ProxyDomains = new List<AccelerateProjectGroupDTO>(result.Content);
            SelectGroup = ProxyDomains.FirstOrDefault();
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
            IHostsFileService.Instance.RemoveHostsByTag();
            httpProxyService.Dispose();
        }
    }
}

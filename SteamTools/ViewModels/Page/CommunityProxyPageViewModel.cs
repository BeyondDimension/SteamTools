using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTool.Proxy;
using SteamTool.Core;
using SteamTools.Services;
using MetroTrilithon.Mvvm;

namespace SteamTools.ViewModels
{
    public class CommunityProxyPageViewModel : TabItemViewModel
    {
        private readonly HostsService hostsService = SteamToolCore.Instance.Get<HostsService>();

        public override string Name
        {
            get { return Properties.Resources.Steam302; }
            protected set { throw new NotImplementedException(); }
        }

        private List<ProxyDomainModel> _SupportedServices;
        public List<ProxyDomainModel> SupportedServices
        {
            get { return _SupportedServices; }
            set
            {
                if (this._SupportedServices != value)
                {
                    this._SupportedServices = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #region 代理状态变更通知
        private bool _ProxyStatus;
        public bool ProxyStatus
        {
            get { return _ProxyStatus; }
            set
            {
                if (this._ProxyStatus != value)
                {
                    this._ProxyStatus = value;
                    if (value)
                    {
                        ProxyService.Current.Proxy.StartProxy();
                        StatusService.Current.Notify("代理已启动");
                    }
                    else
                    {
                        ProxyService.Current.Proxy.StopProxy();
                        StatusService.Current.Notify("代理已关闭");
                    }
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region 脚本启用状态
        private bool _ScriptServiceEnable;
        public bool ScriptServiceEnable
        {
            get { return _ScriptServiceEnable; }
            set
            {
                if (this._ScriptServiceEnable != value)
                {
                    this._ScriptServiceEnable = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        public void Update()
        {
            SupportedServices = ProxyService.ProxyDomains;
        }

        internal override void Initialize()
        {
            //HttpProxy.Current.SetupCertificate();
            ProxyService.Current.Subscribe(nameof(ProxyService.ProxyDomains), this.Update).AddTo(this);
            this.Update();
        }

        public void SetupCertificate_OnClick()
        {
            ProxyService.Current.Proxy.SetupCertificate();
        }

        public void DeleteCertificate_OnClick()
        {
            ProxyService.Current.Proxy.DeleteCertificate();
        }

        public void EditHostsFile_OnClick()
        {
            hostsService.StartNotepadEditHosts();
        }
    }
}

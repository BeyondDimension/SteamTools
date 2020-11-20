using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTool.Proxy;
using SteamTool.Core;
using SteamTools.Services;
using MetroTrilithon.Mvvm;
using SteamTools.Properties;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using SteamTool.Model;

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
                        ProxyStatus = ProxyService.Current.Proxy.StartProxy();
                        StatusService.Current.Notify(Resources.ProxyRun);
                    }
                    else
                    {
                        ProxyService.Current.Proxy.StopProxy();
                        StatusService.Current.Notify(Resources.ProxyStop);
                    }
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion


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

        public void OpenSelectFileDialog_Click()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "脚本文件|*.js",
                Title = Resources.ImportScript,
                //AddExtension = true,
                Multiselect = true
            };
            if (fileDialog.ShowDialog().Value && fileDialog.FileNames.Length > 0)
            {
                foreach (var file in fileDialog.FileNames)
                {
                    File.Copy(file, $@"{Const.SCRIPT_DIR}\{Path.GetFileName(file)}", true);
                }
                ProxyService.Current.InitJsScript();
            }
        }

        public void OpenScriptFileDir_Click()
        {
            Process.Start(Const.SCRIPT_DIR);
        }

        public void DeleteScript_OnClick(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                ProxyService.Current.InitJsScript();
            }
        }
    }
}

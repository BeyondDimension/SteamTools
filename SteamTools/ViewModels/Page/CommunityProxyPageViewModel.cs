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
using SteamTools.Models.Settings;

namespace SteamTools.ViewModels
{
    public class CommunityProxyPageViewModel : TabItemViewModel
    {
        private readonly HostsService hostsService = SteamToolCore.Instance.Get<HostsService>();

        public override string Name
        {
            get { return Properties.Resources.CommunityFix; }
            protected set { throw new NotImplementedException(); }
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

        public void OpenSelectFileDialog_Click()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "JavaScript Files (*.js)|*.js",
                Title = Resources.ImportScript,
                //AddExtension = true,
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == true && fileDialog.FileNames.Length > 0)
            {
                foreach (var file in fileDialog.FileNames)
                {
                    File.Copy(file, $@"{Path.Combine(AppContext.BaseDirectory, Const.SCRIPT_DIR)}\{Path.GetFileName(file)}", true);
                }
                ProxyService.Current.InitJsScript();
            }
        }

        public void OpenScriptFileDir_Click()
        {
            Process.Start(Path.Combine(AppContext.BaseDirectory, Const.SCRIPT_DIR));
        }

        public void DeleteScript_OnClick(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                ProxyService.Current.InitJsScript();
            }
        }

        public void ProxyServices_Checked(ProxyDomainModel proxyDomain)
        {
            var dic = ProxySettings.SupportProxyServicesStatus.Value as Dictionary<int, bool>;
            if (ProxySettings.SupportProxyServicesStatus.Value.ContainsKey(proxyDomain.Index))
            {
                dic.Remove(proxyDomain.Index);
                if (proxyDomain.IsEnable)
                    dic.Add(proxyDomain.Index, proxyDomain.IsEnable);
            }
            else
            {
                dic.Add(proxyDomain.Index, proxyDomain.IsEnable);
            }
            ProxySettings.SupportProxyServicesStatus.Value = dic;
        }
    }
}

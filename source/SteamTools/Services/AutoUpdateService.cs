using SteamTool.Core;
using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamTool.Model.ToolModel;
using SteamTools.Models;
using SteamTool.Core.Common;
using System.Net;
using System.IO;
using Livet;
using SteamTools.Properties;
using System.Diagnostics;

namespace SteamTools.Services
{
    public class AutoUpdateService : NotificationObject
    {
        #region static members
        public static AutoUpdateService Current { get; } = new AutoUpdateService();
        #endregion

        private readonly HttpServices httpServices = SteamToolCore.Instance.Get<HttpServices>();

        #region 更新进度变更通知
        private double _ProgressValue;
        public double ProgressValue
        {
            get => this._ProgressValue;
            set
            {
                if (this._ProgressValue != value)
                {
                    this._ProgressValue = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region 是否有更新
        private bool _IsExistUpdate;
        public bool IsExistUpdate
        {
            get => this._IsExistUpdate;
            set
            {
                if (this._IsExistUpdate != value)
                {
                    this._IsExistUpdate = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        private GithubReleaseModel _UpdateInfo;
        public GithubReleaseModel UpdateInfo
        {
            get => this._UpdateInfo;
            set
            {
                if (this._UpdateInfo != value)
                {
                    this._UpdateInfo = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public async void CheckUpdate()
        {
            try
            {
                StatusService.Current.Notify("正在从Github检查更新...");
                var result = await httpServices.Get(Const.GITHUB_LATEST_RELEASEAPI_URL);
                UpdateInfo = JsonConvert.DeserializeObject<GithubReleaseModel>(result);
                // if (!(ProductInfo.Version > UpdateInfo.version))  Debug时反向检查更新来测试
                if (!(ProductInfo.Version < UpdateInfo.version))
                {
                    IsExistUpdate = false;
                    StatusService.Current.Notify("当前已是最新版本");
                    return;
                }
                IsExistUpdate = true;
            }
            catch (Exception ex)
            {
                Logger.Error("更新出错：", ex);
                StatusService.Current.Notify($"更新出错：{ex.Message}");
            }

        }

        public async void DownloadUpdate()
        {
            if (WindowService.Current.MainWindow.Dialog($"检测到新版本更新内容：{UpdateInfo.body}{Environment.NewLine}是否立即下载更新？", $"{ProductInfo.Title} | 更新提示") == true)
            {
                var upFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, UpdateInfo.assets.FirstOrDefault()?.name));
                //var name = Path.Combine(AppContext.BaseDirectory, @$"{ProductInfo.Title} {UpdateInfo.version}.zip");
                if (upFile.Exists)
                {
                    StatusService.Current.Notify("更新文件已经存在，不需要下载");
                    if (upFile.Extension == ".zip")
                    {
                        OverwriteUpgrade(upFile.FullName);
                    }
                    return;
                }
                var fileReq = WebRequest.Create(UpdateInfo.assets.FirstOrDefault()?.browser_download_url);
                await fileReq?.GetResponseAsync().ContinueWith(s =>
                {
                    long totalBytes = s.Result.ContentLength;
                    using Stream responseStream = s.Result.GetResponseStream();
                    using FileStream fileStream = new FileStream(upFile.FullName, FileMode.Create, FileAccess.Write);
                    long totalDownloadBytes = 0;
                    byte[] bs = new byte[1024];
                    int size = responseStream.Read(bs, 0, bs.Length);
                    while (size > 0)
                    {
                        totalDownloadBytes += size;
                        fileStream.Write(bs, 0, size);
                        ProgressValue = ((double)totalDownloadBytes / (double)totalBytes);
                        StatusService.Current.Set($"下载更新{ProgressValue:P}");
                        size = responseStream.Read(bs, 0, bs.Length);
                    }
                    fileStream.Flush();
                    fileStream.Close();
                    StatusService.Current.Set(Resources.Ready);
                    if (upFile.Extension == ".zip")
                    {
                        OverwriteUpgrade(upFile.FullName);
                    }
                    StatusService.Current.Notify($"{ProductInfo.Title} {UpdateInfo.version}版本已经下载到程序根目录下，请手动替换更新");
                });
            }
        }

        private void OverwriteUpgrade(string zipFile)
        {
            if (new FileInfo(zipFile).Length != UpdateInfo.assets.First().size)
            {
                File.Delete(zipFile);
                StatusService.Current.Notify("效验更新文件失败，重新下载更新...");
                DownloadUpdate();
                return;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                if (WindowService.Current.MainWindow.Dialog($"新版本下载完成，是否自动重启替换到新版本？", $"{ProductInfo.Title} | 更新提示") == true)
                {
                    var out_dir = Path.Combine(AppContext.BaseDirectory, Path.GetFileNameWithoutExtension(zipFile));
                    if (ZipHelper.UnpackFiles(zipFile, out_dir))
                    {
                        File.Delete(zipFile);
                        var batpath = Path.Combine(AppContext.BaseDirectory, "update.cmd");
                        File.WriteAllText(batpath,
                            string.Format(SteamTool.Core.Properties.Resources.ProgramUpdateCmd,
                            ProductInfo.Title + ".exe", out_dir, AppContext.BaseDirectory, App.Instance.ProgramName), Encoding.Default);
                        Process p = new Process();
                        p.StartInfo.FileName = batpath;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;//不显示程序窗口 
                        //管理员权限运行
                        p.StartInfo.Verb = "runas";
                        App.Current.Shutdown();
                        p.Start();//启动程序 
                    }
                }
            });
        }
    }
}

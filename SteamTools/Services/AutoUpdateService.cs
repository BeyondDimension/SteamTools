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

        public async void CheckUpdate()
        {
            try
            {
                StatusService.Current.Notify("正在从Github检查更新...");
                var result = await httpServices.Get(Const.GITHUB_LATEST_RELEASEAPI_URL);
                var model = JsonConvert.DeserializeObject<GithubReleaseModel>(result);
                if (!(ProductInfo.Version < model.version))
                {
                    StatusService.Current.Notify("当前已是最新版本");
                    return;
                }
                if (WindowService.Current.MainWindow.Dialog($"检测到新版本更新内容：{model.body}\r\n是否立即更新？", $"{ProductInfo.Title} | 更新提示") == true)
                {
                    //var name = model.assets.FirstOrDefault()?.name;
                    var name = Path.Combine(AppContext.BaseDirectory, @$"{ProductInfo.Title} {model.version}.zip");
                    if (File.Exists(name))
                    {
                        StatusService.Current.Notify("更新文件已经存在，不需要下载");
                        return;
                    }
                    var fileReq = WebRequest.Create(model.assets.FirstOrDefault()?.browser_download_url);
                    await fileReq?.GetResponseAsync().ContinueWith(s =>
                    {
                        long totalBytes = s.Result.ContentLength;
                        using Stream responseStream = s.Result.GetResponseStream();
                        using FileStream fileStream = new FileStream(name, FileMode.Create, FileAccess.Write);
                        long totalDownloadBytes = 0;
                        byte[] bs = new byte[4096];
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
                        StatusService.Current.Notify($"{ProductInfo.Title} {model.version}版本已经下载到程序根目录下，暂时请手动替换更新");
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("更新出错：", ex);
                WindowService.Current.ShowDialogWindow($"更新出错：{ex.Message}");
            }

        }
    }
}

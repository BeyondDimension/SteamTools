using Microsoft.Extensions.Options;
using System.Application.Models;

namespace System.Application.Services.Implementation
{
    internal sealed class AvaloniaDesktopAppUpdateServiceImpl : DesktopAppUpdateServiceImpl
    {
        public AvaloniaDesktopAppUpdateServiceImpl(IToast toast, ICloudServiceClient client, IOptions<AppSettings> options) : base(toast, client, options)
        {
        }

        protected override void OnExistNewVersion()
        {
            //if (WindowService.Current.MainWindow.Dialog($"检测到新版本更新内容：{UpdateInfo.body}{Environment.NewLine}是否立即下载更新？", $"{ProductInfo.Title} | 更新提示") == true)
            //{
            //    DownloadUpdate();
            //}
        }

        protected override void OnExit()
        {
            // App.Instance.compositeDisposable.Dispose();
        }
    }
}
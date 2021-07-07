using System.Application.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System.Application.Services
{
    /// <summary>
    /// 应用程序更新服务
    /// </summary>
    public interface IAppUpdateService
    {
        static IAppUpdateService Instance => DI.Get<IAppUpdateService>();

        const float MaxProgressValue = 100f;

        /// <summary>
        /// 进度值
        /// </summary>
        float ProgressValue { get; }

        /// <summary>
        /// 进度值描述
        /// </summary>
        string ProgressString { get; }

        /// <summary>
        /// 是否支持服务端分发，如果返回 <see langword="false"/> 将不能使用 DownloadAsync 与 OverwriteUpgradeAsync
        /// <para>对于 iOS 平台，必须使用 App Store 分发</para>
        /// </summary>
        bool IsSupportedServerDistribution { get; }

        /// <summary>
        /// 是否有更新
        /// </summary>
        bool IsExistUpdate { get; }

        /// <summary>
        /// 新版本信息
        /// </summary>
        AppVersionDTO? NewVersionInfo { get; }

        string NewVersionInfoDesc { get; }

        string NewVersionInfoTitle { get; }

        /// <inheritdoc cref="CheckUpdateAsync(bool, bool)"/>
        async void CheckUpdate(bool force = false, bool showIsExistUpdateFalse = true)
        {
            await CheckUpdateAsync(force, showIsExistUpdateFalse);
        }

        /// <summary>
        /// 检查更新，返回新版本信息
        /// </summary>
        /// <param name="force">是否强制检查，如果为 <see langword="false"/> 当有新版本内存中缓存时将跳过 api 请求</param>
        /// <param name="showIsExistUpdateFalse">是否显示已是最新版本吐司提示</param>
        Task CheckUpdateAsync(bool force = false, bool showIsExistUpdateFalse = true);

        ICommand StartUpdateCommand { get; }
    }

#if DEBUG

    [Obsolete("use IAppUpdateService.Instance", true)]
    public class AutoUpdateService
    {
        [Obsolete("use IAppUpdateService.Instance.CheckAsync", true)]
        public void CheckUpdate() => throw new NotImplementedException();

        [Obsolete("use IAppUpdateService.Instance.DownloadAsync", true)]
        public void DownloadUpdate() => throw new NotImplementedException();

        [Obsolete("use IAppUpdateService.Instance.OverwriteUpgradeAsync", true)]
        public void OverwriteUpgrade(string zipFile) => throw new NotImplementedException();

        [Obsolete("use IAppUpdateService.Instance.NewVersionInfo", true)]
        public object? UpdateInfo { get; set; }
    }

#endif
}
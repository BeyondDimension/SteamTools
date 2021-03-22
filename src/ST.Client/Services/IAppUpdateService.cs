using System.Application.Models;

namespace System.Application.Services
{
    /// <summary>
    /// 应用程序更新服务
    /// </summary>
    public interface IAppUpdateService
    {
        public static IAppUpdateService Instance => DI.Get<IAppUpdateService>();

        public const float MaxProgressValue = 100f;

        /// <summary>
        /// 当前进度值
        /// </summary>
        float CurrentProgressValue { get; }

        /// <summary>
        /// 总进度值
        /// </summary>
        float TotalProgressValue { get; }

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

        /// <summary>
        /// 检查更新，返回新版本信息
        /// </summary>
        /// <returns></returns>
        void CheckUpdate();
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
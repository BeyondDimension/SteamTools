// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public interface IGeneralSettings : ISettings
{
    /// <summary>
    /// 自动检查应用更新
    /// </summary>
    bool AutoCheckAppUpdate { get; set; }

    /// <summary>
    /// 选择下载更新渠道
    /// </summary>
    UpdateChannelType UpdateChannel { get; set; }

    /// <summary>
    /// 开机自启动
    /// </summary>
    bool AutoRunOnStartup { get; set; }

    /// <summary>
    /// 启动时最小化
    /// </summary>
    bool MinimizeOnStartup { get; set; }

    /// <summary>
    /// 启用托盘图标
    /// </summary>
    bool TrayIcon { get; set; }

    /// <summary>
    /// 游戏列表使用本地缓存
    /// </summary>
    bool GameListUseLocalCache { get; set; }

    /// <summary>
    /// 文本阅读器提供商，值为程序路径
    /// </summary>
    Dictionary<Platform, string> TextReaderProvider { get; set; }

    /// <summary>
    /// Hosts 文件编码类型
    /// </summary>
    EncodingType HostsFileEncodingType { get; set; }

    /// <summary>
    /// 是否使用硬件加速
    /// </summary>
    bool GPU { get; set; }

    /// <summary>
    /// 使用本机 OpenGL
    /// </summary>
    bool NativeOpenGL { get; set; }

    /// <summary>
    /// 屏幕捕获/允许截图，在一些含有机密的页面上是否允许截图，默认为 <see langword="false"/>
    /// </summary>
    bool ScreenCapture { get; set; }
}

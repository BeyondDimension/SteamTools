// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 获取 排除代理地址
    /// </summary>
    protected static string[] GetNoProxyHostName => new string[] {
        "10.*",
        "172.16.*",
        "172.17.*",
        "172.18.*",
        "172.19.*",
        "172.20.*",
        "172.21.*",
        "172.22.*",
        "172.23.*",
        "172.24.*",
        "172.25.*",
        "172.26.*",
        "172.27.*",
        "172.28.*",
        "172.29.*",
        "172.30.*",
        "172.31.*",
        "192.168.*",
        Constants.Urls.OfficialApiHostName,
        Constants.Urls.OfficialShopApiHostName,
        // Microsoft Store、Windows 搜索的网页预览等 AppContainer 应用不允许连接 127.0.0.1，
        // 系统代理指向本机回环端口时它们会直接失去网络，故在系统代理层面排除，令其直连。
        // 注：Windows 11 26H2(Build 26300) 起 CheckNetIsolation 的环回豁免失效（命令返回成功但状态不落盘），
        // 回环方案不可用，只能在此排除。
        "*.mp.microsoft.com", // 含 winget msstore 源（SFS）的 *.dsx.mp.microsoft.com，其证书固定到微软根 CA
        "*.microsoft.com",
        "*.s-microsoft.com",
        "*.microsoftonline.com",
        "*.live.com",
        "*.windows.com",
        "*.windowsupdate.com",
        "*.msftconnecttest.com",
        "*.msftncsi.com",
        "*.bing.com",
        "*.bing.net",
    };

    /// <summary>
    /// 设置启用或关闭系统代理
    /// </summary>
    Task<bool> SetAsSystemProxyAsync(bool state, IPAddress? ip = null, int port = -1) => Task.FromResult(false);

    /// <summary>
    /// 设置启用或关闭 PAC 代理
    /// </summary>
    /// <param name="state"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<bool> SetAsSystemPACProxyAsync(bool state, string? url = null) => Task.FromResult(false);
}
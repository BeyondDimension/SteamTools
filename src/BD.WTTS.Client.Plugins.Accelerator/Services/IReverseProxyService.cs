namespace BD.WTTS.Services;

partial interface IReverseProxyService
{
    /// <summary>
    /// 将当前服务上的配置项通过 IPC 同步到子进程的设置项
    /// </summary>
    /// <returns></returns>
    ValueTask SyncSettingsAsync();
}
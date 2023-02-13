#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// hosts 文件助手服务，修改需要管理员权限或 Root 权限
/// </summary>
public interface IHostsFileService
{
    static IHostsFileService Instance => Ioc.Get<IHostsFileService>();

    protected const string TAG = "HostsFileS";

    /// <summary>
    /// 打开 hosts 文件
    /// </summary>
    void OpenFile();

    /// <summary>
    /// 打开 hosts 所在文件夹
    /// </summary>
    void OpenFileDir();

    /// <summary>
    /// 重置 hosts 文件
    /// </summary>
    void ResetFile();

    /// <summary>
    /// 读取 hosts 文件
    /// </summary>
    /// <returns></returns>
    IOperationResult<List<(string ip, string domain)>> ReadHostsAllLines();

    /// <summary>
    /// 更新一条 hosts 纪录
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    IOperationResult UpdateHosts(string ip, string domain);

    /// <inheritdoc cref="UpdateHosts(IReadOnlyDictionary{string, string})"/>
    IOperationResult UpdateHosts(IEnumerable<(string ip, string domain)> hosts);

    /// <summary>
    /// 更新多条 hosts 纪录
    /// </summary>
    /// <param name="hosts"></param>
    /// <returns></returns>
    IOperationResult UpdateHosts(IReadOnlyDictionary<string, string> hosts);

    /// <summary>
    /// 移除一条 hosts 纪录
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    IOperationResult RemoveHosts(string ip, string domain);

    /// <inheritdoc cref="RemoveHosts(string, string)"/>
    IOperationResult RemoveHosts(string domain);

    /// <summary>
    /// 移除当前程序写入的 hosts 纪录并还原写入时冲突的备份纪录
    /// </summary>
    /// <returns></returns>
    IOperationResult RemoveHostsByTag();

    /// <summary>
    /// 当程序退出时还原 hosts 文件
    /// </summary>
    void OnExitRestoreHosts();

    /// <summary>
    /// 获取当前 hosts 文件是否包含修改过的标签，注意：产生异常时也将返回 <see langword="false"/>，并 <see cref="Toast"/> 显示 <see cref="Exception"/>
    /// </summary>
    /// <returns></returns>
    bool ContainsHostsByTag();

#if DEBUG
    void OccupyHosts();
#endif
}

#endif
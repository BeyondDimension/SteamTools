namespace BD.WTTS.Enums;

/// <summary>
/// 网络环境检查步骤
/// </summary>
public enum NetworkEnvCheckStep
{
    /// <summary>
    /// Hosts文件检查
    /// </summary>
    HostsFileCheck = 1,

    /// <summary>
    /// 网卡检查
    /// </summary>
    NetworkInterfaceCheck = 2,

    /// <summary>
    /// LSP检查
    /// </summary>
    LSPCheck = 3
}
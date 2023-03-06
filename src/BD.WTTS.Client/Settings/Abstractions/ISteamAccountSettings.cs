namespace BD.WTTS.Settings.Abstractions;

/// <summary>
/// Steam 账号设置
/// </summary>
public interface ISteamAccountSettings
{
    /// <summary>
    /// Steam 账号备注字典
    /// </summary>
    ConcurrentDictionary<long, string?>? AccountRemarks { get; }
}

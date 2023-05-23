namespace BD.WTTS.Settings.Abstractions;

/// <summary>
/// Steam 账号设置
/// </summary>
public interface IGameAccountSettings
{
    static IGameAccountSettings? Instance => Ioc.Get_Nullable<IOptionsMonitor<IGameAccountSettings>>()?.CurrentValue;

    /// <summary>
    /// Steam 账号备注字典
    /// </summary>
    ConcurrentDictionary<long, string?>? AccountRemarks { get; set; }

    /// <summary>
    /// Steam 家庭共享临时禁用
    /// </summary>
    IReadOnlyCollection<DisableAuthorizedDevice>? DisableAuthorizedDevice { get; set; }

    static readonly IReadOnlyCollection<DisableAuthorizedDevice> DefaultDisableAuthorizedDevice = Array.Empty<DisableAuthorizedDevice>();
}

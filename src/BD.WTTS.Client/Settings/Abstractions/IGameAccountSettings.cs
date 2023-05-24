#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1634 // File header should show copyright
// <console-tools-generated/>
#pragma warning restore SA1634 // File header should show copyright
#pragma warning restore IDE0079 // 请删除不必要的忽略
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public interface IGameAccountSettings
{
    static IGameAccountSettings? Instance
        => Ioc.Get_Nullable<IOptionsMonitor<IGameAccountSettings>>()?.CurrentValue;

    /// <summary>
    /// Steam 账号备注字典
    /// </summary>
    ConcurrentDictionary<long, string?>? AccountRemarks { get; set; }

    /// <summary>
    /// Steam 账号备注字典的默认值
    /// </summary>
    const ConcurrentDictionary<long, string?>? DefaultAccountRemarks = null;

    /// <summary>
    /// Steam 家庭共享临时禁用
    /// </summary>
    IReadOnlyCollection<DisableAuthorizedDevice>? DisableAuthorizedDevice { get; set; }

    /// <summary>
    /// Steam 家庭共享临时禁用的默认值
    /// </summary>
    const IReadOnlyCollection<DisableAuthorizedDevice>? DefaultDisableAuthorizedDevice = null;

}

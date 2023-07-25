#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1634 // File header should show copyright
// <console-tools-generated/>
#pragma warning restore SA1634 // File header should show copyright
#pragma warning restore IDE0079 // 请删除不必要的忽略
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public partial interface IPartialGameAccountSettings
{
    static IPartialGameAccountSettings? Instance
        => Ioc.Get_Nullable<IPartialGameAccountSettings>();

    /// <summary>
    /// 账号备注字典
    /// </summary>
    ConcurrentDictionary<string, string?>? AccountRemarks { get; set; }

    /// <summary>
    /// Steam 家庭共享临时禁用
    /// </summary>
    IReadOnlyCollection<DisableAuthorizedDevice>? DisableAuthorizedDevice { get; set; }

    /// <summary>
    /// 启用的账号平台集合
    /// </summary>
    HashSet<string>? EnablePlatforms { get; set; }

}

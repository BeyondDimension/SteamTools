namespace BD.WTTS.Settings.Abstractions;

public interface IGameLibrarySettings
{
    /// <summary>
    /// 隐藏的游戏列表
    /// </summary>
    Dictionary<uint, string?> HideGameList { get; }

    /// <summary>
    /// 挂时长游戏列表
    /// </summary>
    Dictionary<uint, string?>? AFKAppList { get; }

    /// <summary>
    /// 启用自动挂机
    /// </summary>
    bool IsAutoAFKApps { get; }
}

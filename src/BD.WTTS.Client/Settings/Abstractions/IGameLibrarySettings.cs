namespace BD.WTTS.Settings.Abstractions;

/// <summary>
/// Steam 游戏库存设置
/// </summary>
public interface IGameLibrarySettings : ISettings
{
    /// <summary>
    /// 已安装游戏筛选
    /// </summary>
    bool GameInstalledFilter { get; set; }

    /// <summary>
    /// 支持云存档游戏筛选状态
    /// </summary>
    bool GameCloudArchiveFilter { get; set; }

    /// <summary>
    /// 游戏类型筛选状态列表
    /// </summary>
    List<SteamAppType> GameTypeFiltres { get; set; }

    /// <summary>
    /// 隐藏的游戏列表
    /// </summary>
    Dictionary<uint, string?> HideGameList { get; set; }

    /// <summary>
    /// 挂时长游戏列表
    /// </summary>
    Dictionary<uint, string?>? AFKAppList { get; set; }

    /// <summary>
    /// 启用自动挂机
    /// </summary>
    bool IsAutoAFKApps { get; set; }
}

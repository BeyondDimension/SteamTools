namespace BD.WTTS;

public partial interface IApplication
{
    Dictionary<string, TrayMenuItem>? TrayMenus { get; }

    /// <summary>
    /// 创建或修改托盘菜单
    /// </summary>
    /// <param name="menuKey"></param>
    /// <param name="trayMenuItem"></param>
    void UpdateMenuItems(string menuKey, TrayMenuItem trayMenuItem);
}

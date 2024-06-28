namespace BD.WTTS;

public partial interface IApplication
{
    [Mobius(
"""
Mobius_TODO TrayIcon
""")]
    Dictionary<string, TrayMenuItem>? TrayMenus { get; }

    /// <summary>
    /// 创建或修改托盘菜单
    /// </summary>
    /// <param name="menuKey"></param>
    /// <param name="trayMenuItem"></param>
    [Mobius(
"""
Mobius_TODO TrayIcon
""")]
    void UpdateMenuItems(string menuKey, TrayMenuItem trayMenuItem);
}

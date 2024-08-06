// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication : IDisposableHolder
{
    [Mobius(
"""
App.CompositeDisposable
""")]
    new CompositeDisposable CompositeDisposable { get; }

    /// <summary>
    /// 切换当前桌面应用的主题而不改变设置值
    /// </summary>
    /// <param name="value"></param>
    [Mobius(
"""
App.SetThemeNotChangeValue
""")]
    void SetThemeNotChangeValue(AppTheme value) { }

    /// <summary>
    /// 退出整个程序
    /// </summary>
    [Mobius(
"""
App.Shutdown
""")]
    void Shutdown() { }

    /// <summary>
    /// 主窗口恢复显示
    /// </summary>
    [Mobius(
"""
App.RestoreMainWindow
""")]
    void RestoreMainWindow() { }

    /// <summary>
    /// 主窗口置顶一次
    /// </summary>
    [Mobius(
"""
App.RestoreMainWindow
""")]
    void SetTopmostOneTime() { }

    /// <summary>
    /// 托盘菜单
    /// </summary>
    [Mobius(
"""
App.TrayIconMenus
""")]
    IReadOnlyDictionary<string, ICommand> TrayIconMenus { get; }

    /// <summary>
    /// 是否有活动窗口
    /// </summary>
    /// <returns></returns>
    [Mobius(
"""
App.HasActiveWindow
""")]
    bool HasActiveWindow();
}
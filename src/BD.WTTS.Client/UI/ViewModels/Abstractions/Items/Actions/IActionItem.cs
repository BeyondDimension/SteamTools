// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

/// <summary>
/// 右上角 Toolbar Menu
/// <para>https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/toolbaritem</para>
/// </summary>
/// <typeparam name="TEnum"></typeparam>
public interface IActionItem<TEnum> where TEnum : struct, Enum
{
    /// <summary>
    /// Toolbar Menu 显示文本
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    string ToString2(TEnum action);

    /// <summary>
    /// Toolbar Menu 显示图标
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    string GetIcon(TEnum action);

    /// <summary>
    /// Toolbar Menu 是否仅显示图标或折叠在...中仅显示文本
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    bool IsPrimary(TEnum action) => false;

    /// <summary>
    /// Toolbar Menu 点击事件
    /// </summary>
    /// <param name="action"></param>
    void MenuItemClick(TEnum action);
}
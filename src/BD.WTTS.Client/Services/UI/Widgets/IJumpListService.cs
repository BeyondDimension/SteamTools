#if WINDOWS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 表示作为菜单显示在 Windows 7 任务栏按钮上的项和任务的列表。
/// <para>https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.shell.jumplist</para>
/// <para>https://docs.microsoft.com/zh-cn/uwp/api/Windows.UI.StartScreen.JumpList</para>
/// <para>Taskbar Extensions</para>
/// <para>https://docs.microsoft.com/zh-cn/windows/win32/shell/taskbar-extensions</para>
/// </summary>
public interface IJumpListService
{
    protected const string TAG = "JumpListS";

    static IJumpListService Instance => Ioc.Get<IJumpListService>();

    ValueTask AddJumpItemsAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items);

    ValueTask AddJumpItemsAsync(params (string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)[] items) => AddJumpItemsAsync(items.AsEnumerable());
}
#endif
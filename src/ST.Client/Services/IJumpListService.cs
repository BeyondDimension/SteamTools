using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Services
{
    /// <summary>
    /// 表示作为菜单显示在 Windows 7 任务栏按钮上的项和任务的列表。
    /// <para><see cref="https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.shell.jumplist?view=net-5.0"/></para>
    /// <para>Taskbar Extensions</para>
    /// <para><see cref="https://docs.microsoft.com/zh-cn/windows/win32/shell/taskbar-extensions?redirectedfrom=MSDN"/></para>
    /// </summary>
    public interface IJumpListService
    {
        protected const string TAG = "JumpListS";

        static IJumpListService Instance => DI.Get<IJumpListService>();

        Task AddJumpItemsAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items);

        Task AddJumpItemsAsync(params (string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)[] items) => AddJumpItemsAsync(items.AsEnumerable());

        [Obsolete("", true)]
        Task AddJumpTaskAsync(string title, string applicationPath, string iconResourcePath, string arguments = "", string description = "", string customCategory = "")
        {
            return default;
        }
    }
}
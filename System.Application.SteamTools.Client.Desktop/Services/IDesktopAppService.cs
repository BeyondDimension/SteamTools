using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System.Application.Services
{
    public interface IDesktopAppService
    {
        /// <summary>
        /// 当前桌面应用的主题
        /// </summary>
        AppTheme Theme { get; set; }

        /// <summary>
        /// 打开子窗口
        /// </summary>
        Task ShowChildWindow(object window);

        void ShowWindow(object window);

        /// <summary>
        /// 退出整个程序
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 主窗口恢复显示
        /// </summary>
        void RestoreMainWindow();

        /// <summary>
        /// 托盘菜单
        /// </summary>
        IReadOnlyDictionary<string, ICommand> NotifyIconMenus { get; }
    }
}
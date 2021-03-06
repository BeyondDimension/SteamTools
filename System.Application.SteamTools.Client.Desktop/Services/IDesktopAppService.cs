using System.Collections.Generic;
using System.Windows.Input;

namespace System.Application.Services
{
    public interface IDesktopAppService
    {
        /// <summary>
        /// 当前桌面应用的主题
        /// <para><see langword="true"/> Light</para>
        /// <para><see langword="false"/> Dark</para>
        /// </summary>
        bool Theme { get; set; }

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
using System.Application.Models;
using System.Application.Mvvm;
using System.Collections.Generic;
using System.Windows.Input;

namespace System.Application.Services
{
    public interface IDesktopAppService
    {
        public static IDesktopAppService Instance => DI.Get<IDesktopAppService>();

        /// <summary>
        /// 当前桌面应用的主题
        /// </summary>
        AppTheme Theme { get; set; }

        /// <summary>
        /// 切换当前桌面应用的主题而不改变设置值
        /// </summary>
        /// <param name="switch_value"></param>
        void SetThemeNotChangeValue(AppTheme switch_value);

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

        void SetClipboardText(string? s);

        bool IsCefInitComplete { get; }

        CompositeDisposable CompositeDisposable { get; }

        /// <summary>
        /// 是否有活动窗口
        /// </summary>
        /// <returns></returns>
        bool HasActiveWindow();
    }
}

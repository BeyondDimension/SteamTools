using System.Application.Models;
using System.Application.Mvvm;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Windows.Input;

namespace System.Application.UI
{
    partial interface IApplication : IDisposableHolder
    {
        new CompositeDisposable CompositeDisposable { get; }

        /// <summary>
        /// 切换当前桌面应用的主题而不改变设置值
        /// </summary>
        /// <param name="value"></param>
        void SetThemeNotChangeValue(AppTheme value);

        /// <summary>
        /// 退出整个程序
        /// </summary>
        void Shutdown() { }

        /// <summary>
        /// 主窗口恢复显示
        /// </summary>
        void RestoreMainWindow() { }

        /// <summary>
        /// 主窗口置顶一次
        /// </summary>
        void SetTopmostOneTime() { }

        /// <summary>
        /// 托盘菜单
        /// </summary>
        IReadOnlyDictionary<string, ICommand> NotifyIconMenus { get; }

        /// <summary>
        /// 是否有活动窗口
        /// </summary>
        /// <returns></returns>
        bool HasActiveWindow();
    }
}
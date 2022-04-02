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
        /// <summary>
        /// 切换当前桌面应用的主题而不改变设置值
        /// </summary>
        /// <param name="switch_value"></param>
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

#if NET6_0_OR_GREATER
    // 接口默认实现在 Xamarin.Android 上不可用，将引发 Java.Lang.AbstractMethodError: Exception of type 'Java.Lang.AbstractMethodError' was thrown.
    partial interface IApplication
    {
        new CompositeDisposable CompositeDisposable { get; }

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => CompositeDisposable;
    }
#endif
}
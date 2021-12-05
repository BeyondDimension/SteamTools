using ReactiveUI;
using System.Application.Mvvm;
using System.Application.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public interface IViewModelBase : IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable, IDisposableHolder
    {
        new CompositeDisposable CompositeDisposable { get; }

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => CompositeDisposable;

        bool Disposed { get; }

        /// <summary>
        /// 是否在设计器的上下文中运行
        /// </summary>
        public static bool IsInDesignMode { protected get; set; }
            = IApplication.IsDesktopPlatform;

        /// <summary>
        /// 是否使用移动端布局
        /// </summary>
        public static bool IsMobileLayout { protected get; set; }
            = !IApplication.IsDesktopPlatform;
    }
}

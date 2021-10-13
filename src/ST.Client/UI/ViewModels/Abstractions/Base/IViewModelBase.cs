using ReactiveUI;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Application.Mvvm;
using System.Collections.Generic;
using System.Application.Services;

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
        public static bool IsInDesignMode { protected get; set; } = true;

        public static bool IsMobile { get; } = IPlatformService.Instance.IsMobile;
    }
}

using ReactiveUI;
using System.Application.Mvvm;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Application.UI.ViewModels
{
    public class ViewModelBase : ReactiveObject, IViewModelBase
    {
        [IgnoreDataMember]
        public CompositeDisposable CompositeDisposable { get; } = new();

        [IgnoreDataMember]
        public bool Disposed { get; private set; }

        /// <summary>
        /// 是否在设计器的上下文中运行
        /// </summary>
        public static bool IsInDesignMode { get; set; } = true;

        /// <summary>
        /// 释放该实例使用的所有资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;
            if (disposing) CompositeDisposable?.Dispose();
            Disposed = true;
        }
    }

    public interface IViewModelBase : IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    {
        CompositeDisposable CompositeDisposable { get; }

        bool Disposed { get; }
    }
}
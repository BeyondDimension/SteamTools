using ReactiveUI;
using System.Application.Mvvm;

namespace System.Application.UI.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        [NonSerialized] CompositeDisposable? _compositeDisposable;

        [NonSerialized] bool _disposed;

        /// <summary>
        /// 是否在设计器的上下文中运行
        /// </summary>
        public static bool IsInDesignMode { get; set; }

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
            if (_disposed) return;
            if (disposing) _compositeDisposable?.Dispose();
            _disposed = true;
        }
    }
}
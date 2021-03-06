using ReactiveUI;
using System.Application.Mvvm;

namespace System.Application.UI.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        [NonSerialized] private CompositeDisposable? _compositeDisposable = new CompositeDisposable();
        public CompositeDisposable? CompositeDisposable
        {
            get => _compositeDisposable;
            set
            {
                if (_compositeDisposable != value)
                {
                    _compositeDisposable = value;
                }
            }
        }

        [NonSerialized] bool _disposed;

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
            if (_disposed) return;
            if (disposing) CompositeDisposable?.Dispose();
            _disposed = true;
        }
    }
}
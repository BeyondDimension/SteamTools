using ReactiveUI;
using System.Application.Mvvm;
using System.Runtime.Serialization;

namespace System.Application.UI.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        [NonSerialized] private CompositeDisposable? _compositeDisposable = new();
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

#pragma warning disable IDE1006 // 命名样式
        [IgnoreDataMember] protected bool _disposed { get; private set; }
#pragma warning restore IDE1006 // 命名样式

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
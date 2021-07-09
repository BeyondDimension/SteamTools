using ReactiveUI;
#if __MOBILE__
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
#else
using CompositeDisposable = System.Application.Mvvm.CompositeDisposable;
#endif
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Application.Mvvm;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public class ViewModelBase : ReactiveObject, IViewModelBase, IActivatableViewModel
    {
        [IgnoreDataMember]
        public CompositeDisposable CompositeDisposable { get; } = new();

        [IgnoreDataMember]
        public bool Disposed { get; private set; }

        /// <summary>
        /// 是否在设计器的上下文中运行
        /// </summary>
        public static bool IsInDesignMode { get; set; } = true;

        public ViewModelActivator Activator { get; }

        public ViewModelBase()
        {
            Activator = new ViewModelActivator();

            this.WhenActivated(disposables =>
            {
                Activation();
                Disposable.Create(() => { Deactivation(); })
                                  .DisposeWith(disposables);
            });
        }

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

        public bool IsFirstActivation = true;

        public bool IsDeactivation = false;

        public async virtual void Activation()
        {
            if (IsFirstActivation)
            {
                IsFirstActivation = false;
            }
            IsDeactivation = false;
            await Task.CompletedTask;
        }

        public async virtual void Deactivation()
        {
            IsDeactivation = true;
            await Task.CompletedTask;
        }
    }

    public interface IViewModelBase : IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable, IDisposableHolder
    {
        new CompositeDisposable CompositeDisposable { get; }

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => CompositeDisposable;

        bool Disposed { get; }
    }
}
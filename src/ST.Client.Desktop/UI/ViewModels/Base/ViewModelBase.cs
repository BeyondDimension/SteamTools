using ReactiveUI;
using System.Application.Mvvm;
using M_CompositeDisposable = System.Application.Mvvm.CompositeDisposable;
using R_CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class ViewModelBase : ReactiveObject, IViewModelBase, IActivatableViewModel
    {
        [IgnoreDataMember]
        public M_CompositeDisposable CompositeDisposable { get; } = new();

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
                Disposable.Create(() => { DeActivation(); })
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

        protected bool IsFirstActivation = true;
        internal async virtual void Activation()
        {
            if (IsFirstActivation)
            {
                IsFirstActivation = false;
            }
            await Task.CompletedTask;
        }
        internal async virtual void DeActivation()
        {
            await Task.CompletedTask;
        }
    }

    public interface IViewModelBase : IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    {
        M_CompositeDisposable CompositeDisposable { get; }

        bool Disposed { get; }
    }
}
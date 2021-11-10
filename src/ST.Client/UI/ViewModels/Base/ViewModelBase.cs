using ReactiveUI;
using System.Reactive.Disposables;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IViewModelBase, IActivatableViewModel
    {
        [IgnoreDataMember]
        public CompositeDisposable CompositeDisposable { get; } = new();

        [IgnoreDataMember]
        public bool Disposed { get; private set; }

        /// <inheritdoc cref="IViewModelBase.IsInDesignMode"/>
        public static bool IsInDesignMode => IViewModelBase.IsInDesignMode;

        /// <inheritdoc cref="IViewModelBase.IsMobileLayout"/>
        public static bool IsMobileLayout => IViewModelBase.IsMobileLayout;

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

        public virtual void Activation()
        {
            if (IsFirstActivation)
            {
                IsFirstActivation = false;
            }
            IsDeactivation = false;
        }

        public virtual void Deactivation()
        {
            IsDeactivation = true;
        }
    }
}

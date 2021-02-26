using ReactiveUI;
using System.Application.Mvvm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Application.UI.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable, ILocalizationViewModel
    {
        [NonSerialized] private CompositeDisposable _compositeDisposable;

        [NonSerialized] private bool _disposed;

        public ViewModelBase()
        {
            ILocalizationViewModel localization = this;
            localization.OnChangeLanguage();
        }

        /// <summary>
        /// 是否在设计器的上下文中运行
        /// </summary>
        public static bool IsInDesignMode { get; set; }

        protected static TViewModel GetViewModel<TViewModel>() where TViewModel : ViewModelBase, new()
        {
            if (IsInDesignMode) return new TViewModel();
            return DI.Get<TViewModel>();
        }

        /// <summary>
        ///  释放该实例使用的所有资源
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
        protected static TViewModel GetViewModel<TViewModel>() where TViewModel : ViewModelBase => DI.Get<TViewModel>();
    }
}
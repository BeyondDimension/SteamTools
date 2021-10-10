using ReactiveUI;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Application.Mvvm;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public interface IViewModelBase : IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable, IDisposableHolder
    {
        new CompositeDisposable CompositeDisposable { get; }

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => CompositeDisposable;

        bool Disposed { get; }
    }
}

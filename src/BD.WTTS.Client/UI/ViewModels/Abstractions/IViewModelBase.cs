// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public interface IViewModelBase : IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable, IDisposableHolder
{
    new CompositeDisposable CompositeDisposable { get; }

    ICollection<IDisposable> IDisposableHolder.CompositeDisposable => CompositeDisposable;
}

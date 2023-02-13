// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public interface IViewModelBase : IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable, IDisposableHolder
{
    new CompositeDisposable CompositeDisposable { get; }

    ICollection<IDisposable> IDisposableHolder.CompositeDisposable => CompositeDisposable;

    bool Disposed { get; }

    /// <summary>
    /// 是否在设计器的上下文中运行
    /// </summary>
    public static bool IsInDesignMode { protected get; set; } = IApplication.IsDesktop();
}

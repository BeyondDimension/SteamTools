// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract partial class ViewModelBase : BaseNotifyPropertyChanged, IViewModelBase, IActivatableViewModel, IDisposable
{
    /// <summary>
    /// 当前视图模型是否为单例
    /// </summary>
    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    protected virtual bool IsSingleInstance { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public CompositeDisposable CompositeDisposable { get; private set; } = new();

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ViewModelActivator Activator { get; protected set; }

    public ViewModelBase()
    {
        Activator = new ViewModelActivator();

        this.WhenActivated(disposables =>
        {
            Activation();

            CompositeDisposable.Add(Disposable.Create(Deactivation)
                                              .DisposeWith(disposables));
        });
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public bool IsFirstActivation = true;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public bool IsDeactivation = false;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public bool Disposed { get; private set; }

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

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                if (IsSingleInstance)
                {
                    try
                    {
                        IViewModelManager.Instance.Dispose(this);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(ViewModelBase), ex, "Dispose fail.");
                    }
                }

                // 释放托管状态(托管对象)
                Activator?.Dispose();
                CompositeDisposable?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            Activator = null!;
            CompositeDisposable = null!;
            Disposed = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

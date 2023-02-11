namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract partial class ViewModelBase : BaseNotifyPropertyChanged, IViewModelBase, IActivatableViewModel
{
    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public CompositeDisposable CompositeDisposable { get; } = new();

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public bool Disposed { get; private set; }

    /// <inheritdoc cref="IViewModelBase.IsInDesignMode"/>
    public static bool IsInDesignMode => IViewModelBase.IsInDesignMode;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ViewModelActivator Activator { get; }

    public ViewModelBase()
    {
        Activator = new ViewModelActivator();

        this.WhenActivated(disposables =>
        {
            Activation();
            Disposable.Create(Deactivation)
                .DisposeWith(disposables);
        });
    }

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

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public bool IsFirstActivation = true;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
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

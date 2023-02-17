// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract class RIdTitleIconViewModel<TId, TIcon> : IdTitleIconViewModel<TId, TIcon>, IDisposable
{
    IDisposable? disposable;
    bool disposedValue;

    /// <summary>
    /// 绑定父视图模型
    /// </summary>
    /// <param name="vm"></param>
    protected virtual void OnBind(IDisposableHolder vm)
    {
        disposable = ResourceService.Subscribe(OnResChanged);
        disposable.AddTo(vm);
    }

    protected virtual void OnResChanged()
    {
        Title = GetTitleById(id);
    }

    /// <summary>
    /// 解绑父视图模型
    /// </summary>
    /// <param name="vm"></param>
    protected virtual void OnUnbind(IDisposableHolder vm)
    {
        if (disposable != null)
        {
            disposable.RemoveTo(vm);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                disposable?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposable = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public abstract class RIdTitleViewModel<TId> : RIdTitleIconViewModel<TId, byte>
{
    protected sealed override bool IgnoreIcon => true;

    protected sealed override byte GetIconById(TId? id) => default;
}
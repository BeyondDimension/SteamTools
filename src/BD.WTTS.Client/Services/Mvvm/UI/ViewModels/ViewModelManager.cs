// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

internal sealed class ViewModelManager : ReactiveObject, IViewModelManager
{
    bool disposedValue;
    Dictionary<Type, byte[]> viewModelDataCaches = new();
    Dictionary<Type, ViewModelBase> viewModelCaches = new();

    public T Get<T>() where T : ViewModelBase
    {
        var vmType = typeof(T);
        if (viewModelCaches.TryGetValue(vmType, out var vmValue))
        {
            return (T)vmValue;
        }
        else
        {
            T vmValue2;
            if (viewModelDataCaches.TryGetValue(vmType, out var value))
            {
                var viewModel = Serializable.DMP2<T>(value);
                vmValue2 = viewModel.ThrowIsNull();
            }
            else
            {
                vmValue2 = Activator.CreateInstance<T>();
            }
            viewModelCaches[vmType] = vmValue2;
            return vmValue2;
        }
    }

    public ViewModelBase Get(Type vmType)
    {
        if (viewModelCaches.TryGetValue(vmType, out var vmValue))
        {
            return vmValue;
        }
        else
        {
            ViewModelBase? vmValue2;
            if (viewModelDataCaches.TryGetValue(vmType, out var value))
            {
                var viewModel = (ViewModelBase?)Serializable.DMP2(vmType, value);
                vmValue2 = viewModel.ThrowIsNull();
            }
            else
            {
                vmValue2 = (ViewModelBase)Activator.CreateInstance(vmType)!;
            }
            viewModelCaches[vmType] = vmValue2;
            return vmValue2;
        }
    }

    public void Dispose(ViewModelBase viewModel)
    {
        var vmType = viewModel.GetType();
        var value = Serializable.SMP2(vmType, viewModel);
        viewModelDataCaches[vmType] = value;
        viewModelCaches.Remove(vmType);
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                viewModelDataCaches.Clear();
                viewModelCaches.Clear();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            viewModelDataCaches = null!;
            viewModelCaches = null!;
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
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.Pipes;

namespace BD.WTTS.Services.Implementation;

public sealed class IPCServiceImpl : IPCService
{
    bool disposedValue;
    IpcProvider? ipcProvider;
    PeerProxy? peer;
    TaskCompletionSource? tcs;

    public async Task RunAsync(string moduleName, TaskCompletionSource tcs, string pipeName,
        Action<IpcProvider>? configureIpcProvider = null)
    {
        this.tcs = tcs;
        ipcProvider = new IpcProvider(
            IPCModuleService.Constants.GetClientPipeName(moduleName, pipeName));
        ipcProvider.CreateIpcJoint<IPCModuleService>(new IPCModuleServiceImpl(this));
        configureIpcProvider?.Invoke(ipcProvider);
        ipcProvider.StartServer();

        peer = await ipcProvider.GetAndConnectToPeerAsync(pipeName);
    }

    sealed class IPCModuleServiceImpl : IPCModuleService
    {
        readonly IPCServiceImpl impl;

        public IPCModuleServiceImpl(IPCServiceImpl impl)
        {
            this.impl = impl;
        }

        public void Dispose() => impl.Dispose();
    }

    public T? GetService<T>() where T : class
    {
        if (ipcProvider != null && peer != null)
        {
            return ipcProvider.CreateIpcProxy<T>(peer);
        }
        return default;
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                try
                {
                    ipcProvider?.Dispose();
                }
                catch (InvalidOperationException)
                {
                    // Unhandled exception. System.InvalidOperationException: 未启动之前，不能获取 IpcServerService 属性的值
                    // at dotnetCampus.Ipc.Pipes.IpcProvider.get_IpcServerService()
                    // at dotnetCampus.Ipc.Pipes.IpcProvider.Dispose()
                    // at BD.WTTS.Services.Implementation.IPCServiceImpl.Dispose(Boolean disposing)
                }
                tcs?.TrySetResult();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            peer = null;
            ipcProvider = null;
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

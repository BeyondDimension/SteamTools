namespace BD.WTTS.Services.Implementation;

sealed partial class BackendAcceleratorServiceImpl : IAcceleratorService
{
    readonly IPCSubProcessService ipc;
    readonly IXunYouAccelStateToFrontendCallback accelStateToFrontendCallback;

    public BackendAcceleratorServiceImpl(IPCSubProcessService ipc)
    {
        this.ipc = ipc;
        accelStateToFrontendCallback = ipc.GetService<IXunYouAccelStateToFrontendCallback>().ThrowIsNull(nameof(accelStateToFrontendCallback));
    }
}

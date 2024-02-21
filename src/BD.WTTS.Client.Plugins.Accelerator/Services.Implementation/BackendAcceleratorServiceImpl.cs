namespace BD.WTTS.Services.Implementation;

sealed partial class BackendAcceleratorServiceImpl : IAcceleratorService
{
    public BackendAcceleratorServiceImpl()
    {
    }

    public void InitStateSubscribe()
    {
        try
        {
            if (XunYouSDK.IsSupported)
                XunYouSDK.GetState(XunYouAccelStateCallback);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(BackendAcceleratorServiceImpl), ex, "GetAccelStateEx2 fail.");
        }
    }
}

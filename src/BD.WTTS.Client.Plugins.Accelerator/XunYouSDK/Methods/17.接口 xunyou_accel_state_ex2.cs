namespace Mobius;

partial class XunYouSDK // 17.接口 xunyou_status
{
    /// <summary>
    /// 迅游加速状态回调函数委托
    /// </summary>
    /// <param name="status">迅游加速状态</param>
    /// <param name="thisptr">用户自定义，回调参数指针，该指针的生命周期由调用方维护</param>
    /// <returns>返回值为 0 即可。</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int XunYouStateCallback(XunYouState status, nint thisptr);

    static XunYouStateCallback? globalXunYouStateCallback;

    /// <summary>
    /// 异步获取当前加速器的加速结果。
    /// </summary>
    /// <param name="callback">加速结果回调</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XunYouAccelStateEx GetState(XunYouStateCallback callback)
    {
        int result;
        unsafe
        {
            var xunyou_callback = (delegate* unmanaged[Stdcall]<int, nint, int>)Marshal.GetFunctionPointerForDelegate(globalXunYouStateCallback = callback);
            result = xunyou_status(appId, default, xunyou_callback);
        }
        return (XunYouAccelStateEx)result;
    }

    /// <summary>
    /// 异步获取当前加速器的加速结果。
    /// </summary>
    /// <param name="id">合作 id，由迅游给出明确值。</param>
    /// <param name="ptr">用户自定义，回调参数指针，该指针的生命周期由调用方维护</param>
    /// <param name="callback">加速结果回调</param>
    /// <returns></returns>
    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_status(
        int id,
        nint ptr,
        delegate* unmanaged[Stdcall]<int, nint, int> callback);
}
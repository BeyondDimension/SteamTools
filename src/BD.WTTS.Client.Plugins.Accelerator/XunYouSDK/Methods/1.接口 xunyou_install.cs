namespace Mobius;

partial class XunYouSDK // 21.接口 xunyou_install_ex
{
    /// <summary>
    /// 迅游下载进度回调函数委托
    /// </summary>
    /// <param name="par">若 par 小于等于 100 表示安装进度，若 par 大于 100 表示异步安装中返回的状态值，参见 1.4.2 返回值说明。</param>
    /// <param name="thisptr">用户自定义，回调参数指针</param>
    /// <returns>返回值为 0 即可。</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int XunYouDownLoadCallback(int par, nint thisptr);

    static XunYouDownLoadCallback? globalXunYouDownLoadCallback_xunyou_install;

    /// <summary>
    /// 用于下载并安装迅游加速器。此接口只安装迅游，不启动迅游。如果已安装迅游，不再进行其它操作。
    /// </summary>
    /// <param name="callback">迅游启动进度(包含下载和加速，无安装)</param>
    /// <param name="ptr">保存回调函数的返回参数可以为传默认值</param>
    /// <param name="installPath">迅游加速器安装路径，不指定，则默认安装在 xunyoucall.dll 所在目录</param>
    /// <param name="installPackPath">迅游加速器安装包下载文件夹路径，不指定，则默认下载在xunyoucall.dll所在目录</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Install(XunYouDownLoadCallback? callback = null,
        nint ptr = default,
        string? installPath = null,
        string? installPackPath = null)
    {
        int result;
        unsafe
        {
            var xunyou_callback = callback is null ? null : (delegate* unmanaged[Stdcall]<int, nint, int>)
                Marshal.GetFunctionPointerForDelegate(globalXunYouDownLoadCallback_xunyou_install = callback);
            result = xunyou_install_ex(appId, xunyou_callback, ptr,
                installPackPath, installPackPath?.Length ?? 0,
                installPath, installPath?.Length ?? 0);
        }
        return result;
    }

    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_install_ex(int id,
        delegate* unmanaged[Stdcall]<int, nint, int> callback = null,
        nint ptr = default,
        [MarshalAs(UnmanagedType.LPStr)] string? installpack_path = default,
        int installpack_path_len = 0,
        [MarshalAs(UnmanagedType.LPStr)] string? install_path = default,
        int install_path_len = 0);
}
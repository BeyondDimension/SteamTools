namespace Mobius;

partial class XunYouSDK // 22.接口 xunyou_start_ex3
{
    /// <summary>
    /// <list type="table">
    /// <item>此接口支持自动登录，对指定启动游戏，并对传入的游戏分区进行加速（Jwt）。</item>
    /// <item>如果没有安装迅游，则先下载并安装，然后启动迅游或启动加速。</item>
    /// <item>如果 xunyou_callback 为空，则该接口走同步的方式；</item>
    /// <item>如果 xunyou_callback 不为空，则该接口走异步的方式。</item>
    /// <item>异步启动是直接启动线程操作，返回值在回调函数里面调用；</item>
    /// <item>同步是直接返回相关结果。异步回调的方式会返回一个系统默认值 1。</item>
    /// </list>
    /// </summary>
    /// <param name="jwt">登录 token，由调用方提供</param>
    /// <param name="nickname">账号昵称</param>
    /// <param name="show"><see cref="PInvoke.User32.WindowStyles"/></param>
    /// <param name="gameid">游戏 id，由迅游给出明确值，或传 0，如果传 0，表示只启动迅游，不自动加速。</param>
    /// <param name="area">游戏分区 id ，由游戏给出区服名和 id 对照表，或传 0，如果传 0，表示只启动迅游，不自动加速。</param>
    /// <param name="server">游戏服 id</param>
    /// <param name="callback"></param>
    /// <param name="ptr"></param>
    /// <param name="installPath">迅游加速器安装路径，不指定，则默认安装在 xunyoucall.dll 所在目录</param>
    /// <param name="areaName">游戏区名</param>
    /// <param name="svrName">游戏服名</param>
    /// <returns></returns>
    public static int StartEx3(
        string jwt,
        string nickname,
        XunYouShowCommands show = XunYouShowCommands.SW_HIDE,
        int gameid = 0,
        int area = 0,
        int server = 0,
        XunYouDownLoadCallback? callback = null,
        nint ptr = default,
        string? installPath = default,
        string? areaName = default,
        string? svrName = default)
    {
        int result;
        unsafe
        {
            var xunyou_callback = callback is null ? null :
                (delegate* unmanaged[Stdcall]<int, nint, int>)Marshal.GetFunctionPointerForDelegate(
                    globalXunYouDownLoadCallback_xunyou_start_ex2 = callback);
            int install_path_len = installPath?.Length ?? 0;
            int area_name_len = areaName?.Length ?? 0;
            int svr_name_len = svrName?.Length ?? 0;
            int jwt_len = jwt?.Length ?? 0;
            int nickname_len = jwt?.Length ?? 0;

            result = xunyou_start_ex2(appId, jwt, jwt_len,
                userType, channel_no, channel_no.Length,
                nickname, nickname_len, show,
                gameid,
                area, server, xunyou_callback, ptr,
                installPath, install_path_len, areaName,
                area_name_len, svrName, svr_name_len);
        }
        return result;
    }

    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_start_ex3(
        int id,
        [MarshalAs(UnmanagedType.LPStr)] string? jwt,
        int jwt_len,
        int user_type,
        [MarshalAs(UnmanagedType.LPStr)] string? channel_no,
        int channel_no_len,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? nickname,
        int nickname_len,
        XunYouShowCommands show = XunYouShowCommands.SW_HIDE,
        int gameid = 0,
        int area = 0,
        int server = 0,
        delegate* unmanaged[Stdcall]<int, nint, int> callback = null,
        nint ptr = default,
        [MarshalAs(UnmanagedType.LPStr)] string? install_path = default,
        int install_path_len = 0,
        [MarshalAs(UnmanagedType.LPStr)] string? area_name = default,
        int area_name_len = 0,
        [MarshalAs(UnmanagedType.LPStr)] string? svr_name = default,
        int svr_name_len = 0);
}
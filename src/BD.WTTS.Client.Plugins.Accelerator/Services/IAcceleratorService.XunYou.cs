namespace BD.WTTS.Services;

partial interface IAcceleratorService // XunYou
{
    /// <summary>
    /// 迅游是否安装
    /// </summary>
    /// <returns></returns>
    Task<ApiRsp<bool>> XY_IsInstall(CancellationToken cancellationToken = default);

    /// <summary>
    /// 卸载迅游
    /// </summary>
    /// <returns></returns>
    Task<ApiRsp<XunYouUninstallCode>> XY_Uninstall(CancellationToken cancellationToken = default);

    /// <summary>
    /// 迅游启动加速
    /// </summary>
    /// <param name="openid">OpenId</param>
    /// <param name="nickname">用户昵称</param>
    /// <param name="gameid">游戏ID</param>
    /// <param name="area">区服Id</param>
    /// <param name="server">游戏服Id</param>
    /// <param name="areaPath">区服名称</param>
    /// <param name="svrPath">游戏服名</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp<int>> XY_StartEx2(
        string openid,
        string nickname,
        int gameid,
        int area,
        int server,
        string? areaPath = default,
        string? svrPath = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 同步发送加速命令
    /// </summary>
    /// <param name="gameid">游戏Id</param>
    /// <param name="area">区服Id</param>
    /// <param name="serverid"></param>
    /// <param name="areaName">区服名称</param>
    /// <param name="svrName">游戏服名</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp<int>> XY_StartAccel(
      int gameid,
      int area,
      int serverid = default,
      string? areaName = default,
      string? svrName = default,
      CancellationToken cancellationToken = default);

    /// <summary>
    /// 安装迅游
    /// </summary>
    /// <param name="installPath">安装路径</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<ApiRsp<int>> XY_Install(string installPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取迅游加速状态
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp<XunYouAccelStateModel?>> XY_GetAccelStateEx(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止加速
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp<XunYouSendResultCode>> XY_StopAccel(CancellationToken cancellationToken = default);

    /// <summary>
    /// 迅游加速器是否在运行
    /// </summary>
    /// <returns></returns>
    Task<ApiRsp<XunYouIsRunningCode>> XY_IsRunning(CancellationToken cancellationToken = default);

    /// <summary>
    /// 迅游加速器启动游戏
    /// </summary>
    /// <returns></returns>
    Task<ApiRsp<XunYouStartGameCode>> XY_StartGame(CancellationToken cancellationToken = default);

    /// <summary>
    /// 迅游加速器显示隐藏
    /// </summary>
    /// <returns></returns>
    Task<ApiRsp<int>> XY_ShowWinodw(bool showHide, CancellationToken cancellationToken = default);
}

partial interface IXunYouAccelStateToFrontendCallback
{
    void XunYouAccelStateToFrontendCallback(XunYouAccelStateModel m);
}
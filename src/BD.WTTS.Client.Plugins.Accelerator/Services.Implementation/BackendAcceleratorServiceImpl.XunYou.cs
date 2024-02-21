using System.Threading.Channels;

namespace BD.WTTS.Services.Implementation;

partial class BackendAcceleratorServiceImpl
{
    XunYouState lastXunYouState = default;

    int XunYouAccelStateCallback(XunYouState status, nint thisptr)
    {
        lastXunYouState = status;
        XunYouAccelStateModel m;
        switch (status)
        {
            case XunYouState.加速中:
            case XunYouState.加速已完成:
            case XunYouState.停止加速中:
                m = GetXunYouAccelStateModel(status);
                break;
            default:
                m = new XunYouAccelStateModel
                {
                    State = status,
                };
                break;
        }
#if DEBUG
        Console.WriteLine($"迅游加速状态变更：{m.State}, {m.AccelState}, GameId: {m.GameId}, AreaId: {m.AreaId}, ServerId: {m.ServerId}");
#endif
        XunYouAccelStateToFrontendCallback(m);

        return 0;
    }

    /*async*/
    void XunYouAccelStateToFrontendCallback(XunYouAccelStateModel m)
    {
        try
        {
            // 通知前端
            //var ipc = Ioc.Get<IPCSubProcessService>();
            //var accelStateToFrontendCallback = ipc.GetService<IXunYouAccelStateToFrontendCallback>();
            var accelStateToFrontendCallback = Ioc.Get<IXunYouAccelStateToFrontendCallback>();
            accelStateToFrontendCallback.ThrowIsNull(nameof(accelStateToFrontendCallback)).XunYouAccelStateToFrontendCallback(m);
            //await Plugin.IpcServer.HubContext.Clients.All.SendAsync(nameof(XunYouAccelStateModel), m);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(BackendAcceleratorServiceImpl), "XunYouAccelStateToFrontendCallback fail.", ex);
        }
    }

    static XunYouAccelStateModel GetXunYouAccelStateModel(XunYouState status)
    {
        var accState = XunYouSDK.GetAccelStateEx(out var gameid, out var areaid, out var serverid);
        return new XunYouAccelStateModel
        {
            State = status,
            AccelState = accState,
            GameId = gameid,
            AreaId = areaid,
            ServerId = serverid,
        };
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<bool>> XY_IsInstall(CancellationToken cancellationToken = default)
    {
        var result = XunYouSDK.IsInstall();
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<XunYouUninstallCode>> XY_Uninstall(CancellationToken cancellationToken = default)
    {
        var result = XunYouSDK.Uninstall();
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<int>> XY_StartEx2(
        string openid,
        string nickname,
        int gameid,
        int area,
        string? areaPath = default,
        string? svrPath = default,
        CancellationToken cancellationToken = default)
    {
        var result = XunYouSDK.StartEx2(
            openid,
            nickname,
            gameid,
            area,
            null,
            default,
            GameAcceleratorSettings.WattAcceleratorDirPath.Value!,
            areaPath,
            svrPath);
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<int>> XY_StartAccel(
      int gameid,
      int area,
      int serverid = default,
      string? areaName = default,
      string? svrName = default,
      CancellationToken cancellationToken = default)
    {
        var result = XunYouSDK.StartAccel(
            gameid,
            area,
            areaName,
            serverid,
            svrName);
        await Task.CompletedTask;
        return result == XunYouSendResultCode.发送失败 ? 300 : 101;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<XunYouAccelStateModel?>> XY_GetAccelStateEx(CancellationToken cancellationToken = default)
    {
        var result = GetXunYouAccelStateModel(lastXunYouState);
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ApiRsp<int>> XY_Install(string installPath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<int>();
        int XunYouDownLoadCallback(int par, nint thisptr)
        {
            channel.Writer.TryWrite(par);
            if (par > 100)
            {
                channel.Writer.Complete();
            }
            return 0;
        }
        var result = XunYouSDK.Install(XunYouDownLoadCallback, default, installPath);
        if (result == 101) // 成功
        {
            await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return item;
            }
        }
        else
        {
            yield return result;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<XunYouSendResultCode>> XY_StopAccel(CancellationToken cancellationToken = default)
    {
        var result = XunYouSDK.Stop();
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<XunYouIsRunningCode>> XY_IsRunning(CancellationToken cancellationToken = default)
    {
        var result = XunYouSDK.IsRunning();
        await Task.CompletedTask;
        return result;
    }
}

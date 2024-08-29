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
        static string? EnumToStringDebug<T>(T? value) where T : struct, Enum
        {
            if (value is null)
                return null;

            if (Enum.IsDefined(typeof(T), value))
            {
                return $"{value}({ConvertibleHelper.Convert<int, T>(value.Value)})";
            }
            return value.ToString();
        }
        Console.WriteLine($"迅游加速状态变更：{EnumToStringDebug((XunYouState?)m.State)}, {EnumToStringDebug(m.AccelState)}, GameId: {m.GameId}, AreaId: {m.AreaId}, ServerId: {m.ServerId}");
#endif
        XunYouAccelStateToFrontendCallback(m);

        return 0;
    }

    /*async*/
    void XunYouAccelStateToFrontendCallback(XunYouAccelStateModel m)
    {
        if (disposedValue)
            return;

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
            Log.Error(nameof(BackendAcceleratorServiceImpl), ex, "XunYouAccelStateToFrontendCallback fail.");
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
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok(false);
        }

        var result = XunYouSDK.IsInstall();
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<XunYouUninstallCode>> XY_Uninstall(CancellationToken cancellationToken = default)
    {
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok(XunYouUninstallCode.卸载成功);
        }

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
        int server,
        string? areaPath = default,
        string? svrPath = default,
        CancellationToken cancellationToken = default)
    {
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok(0);
        }

        var show = XunYouShowCommands.SW_HIDE;
        var result = XunYouSDK.StartEx2(
            openid,
            nickname,
            show,
            gameid,
            area,
            server,
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
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok(0);
        }

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
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok((XunYouAccelStateModel?)null);
        }

        var result = GetXunYouAccelStateModel(lastXunYouState);
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ApiRsp<int>> XY_Install(string installPath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!XunYouSDK.IsSupported)
        {
            yield break;
        }

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
        var installPackPath = Plugin.Instance.CacheDirectory;
        var result = XunYouSDK.Install(XunYouDownLoadCallback, default, installPath, installPackPath);
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
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok(XunYouSendResultCode.发送成功);
        }

        var result = XunYouSDK.Stop();
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<XunYouIsRunningCode>> XY_IsRunning(CancellationToken cancellationToken = default)
    {
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok(XunYouIsRunningCode.加速器未启动);
        }

        var result = XunYouSDK.IsRunning();
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<XunYouStartGameCode>> XY_StartGame(CancellationToken cancellationToken = default)
    {
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok(XunYouStartGameCode.失败);
        }

        var result = XunYouSDK.StartGame();
        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<int>> XY_ShowWinodw(bool showHide, CancellationToken cancellationToken = default)
    {
        if (!XunYouSDK.IsSupported)
        {
            return ApiRspHelper.Ok(0);
        }

        XunYouSDK.xunyou_show(Convert.ToInt32(showHide));
        await Task.CompletedTask;
        return ApiRspHelper.Ok(0);
    }
}

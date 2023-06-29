namespace BD.WTTS.Enums;

public enum CommandExitCode
{
    #region 100 ~ 5xx HttpStatusCode

    /// <inheritdoc cref="HttpStatusCode.OK"/>
    HttpStatusCodeOk = 200,

    /// <inheritdoc cref="HttpStatusCode.BadRequest"/>
    HttpStatusBadRequest = 400,

    /// <inheritdoc cref="HttpStatusCode.InternalServerError"/>
    HttpStatusCodeInternalServerError = 500,

    #endregion

    #region xxxx 4 位数通用错误码

    /// <summary>
    /// 参数不能为空数组
    /// </summary>
    EmptyArrayArgs = 4001,

    /// <summary>
    /// IPC 管道名称不能为空
    /// </summary>
    EmptyPipeName = 4002,

    /// <summary>
    /// 主进程 Id 不能为空
    /// </summary>
    EmptyMainProcessId = 4003,

    /// <summary>
    /// 主进程 Id 找不到
    /// </summary>
    NotFoundMainProcessId = 4004,

    /// <summary>
    /// 子进程参数，下标为 2 的模型不能为 null
    /// </summary>
    SubProcessArgumentIndex2ModelIsNull,

    /// <summary>
    /// 获取一组插件失败
    /// </summary>
    GetPluginsFail = 4040,

    /// <summary>
    /// 获取单个插件失败
    /// </summary>
    GetPluginFail = 4041,

    /// <summary>
    /// 获取子进程启动配置失败
    /// </summary>
    GetSubProcessBootConfigurationFail = 4042,

    #endregion
}
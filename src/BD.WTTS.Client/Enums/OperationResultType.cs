namespace BD.WTTS.Enums;

/// <summary>
/// 表示操作结果的枚举
/// </summary>
[Description("操作结果的枚举")]
public enum OperationResultType
{
    /// <summary>
    /// 0 - 操作成功
    /// </summary>
    [Description("操作成功。")]
    Success,

    /// <summary>
    /// 1 - 操作取消或操作没引发任何变化
    /// </summary>
    [Description("操作没有引发任何变化，提交取消。")]
    NoChanged,

    /// <summary>
    /// 2 - 参数错误
    /// </summary>
    [Description("参数错误。")]
    ParamError,

    /// <summary>
    /// 3 - 指定参数的数据不存在
    /// </summary>
    [Description("指定参数的数据不存在。")]
    QueryNull,

    /// <summary>
    /// 4 - 权限不足
    /// </summary>
    [Description("当前用户权限不足，不能继续操作。")]
    PurviewLack,

    /// <summary>
    /// 5 - 登录超时
    /// </summary>
    [Description("登录超时")]
    LoginTimeOut,

    /// <summary>
    /// 6 - 非法操作
    /// </summary>
    [Description("非法操作。")]
    IllegalOperation,

    /// <summary>
    /// 7 - 警告
    /// </summary>
    [Description("警告")]
    Warning,

    /// <summary>
    /// 8 - 操作引发错误
    /// </summary>
    [Description("操作引发错误。")]
    Error,
}
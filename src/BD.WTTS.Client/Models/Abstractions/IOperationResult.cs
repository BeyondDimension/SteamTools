namespace BD.WTTS.Models.Abstractions;

/// <summary>
/// 操作结果信息类，对操作结果进行封装
/// </summary>
public interface IOperationResult
{
    /// <summary>
    /// 获取或设置 操作结果类型
    /// </summary>
    OperationResultType ResultType { get; set; }

    /// <summary>
    /// 获取或设置 操作返回信息
    /// </summary>
    string Message { get; set; }

    /// <summary>
    /// 获取或设置 操作返回的日志消息，用于记录日志
    /// </summary>
    string LogMessage { get; set; }
}

/// <inheritdoc cref="IOperationResult"/>
public interface IOperationResult<T> : IOperationResult
{
    /// <summary>
    /// 获取或设置 操作结果附加信息
    /// </summary>
    T AppendData { get; set; }
}

/// <inheritdoc cref="IOperationResult{T}"/>
public abstract class OperationResultBase<T> : IOperationResult<T>
{
    #region 构造函数

    public OperationResultBase()
    {
        ResultType = OperationResultType.Error;
    }

    /// <summary>
    /// 初始化一个 操作结果信息类 的新实例
    /// </summary>
    /// <param name="resultType">操作结果类型</param>
    public OperationResultBase(OperationResultType resultType)
    {
        ResultType = resultType;
    }

    /// <summary>
    /// 初始化一个 定义返回消息的操作结果信息类 的新实例
    /// </summary>
    /// <param name="resultType">操作结果类型</param>
    /// <param name="message">返回消息</param>
    public OperationResultBase(OperationResultType resultType, string message)
        : this(resultType)
    {
        Message = message;
    }

    /// <summary>
    /// 初始化一个 定义返回消息与附加数据的操作结果信息类 的新实例
    /// </summary>
    /// <param name="resultType">操作结果类型</param>
    /// <param name="message">返回消息</param>
    /// <param name="appendData">返回数据</param>
    public OperationResultBase(OperationResultType resultType, string message, T? appendData)
        : this(resultType, message)
    {
        _AppendData = appendData;
    }

    /// <summary>
    /// 初始化一个 定义返回消息与日志消息的操作结果信息类 的新实例
    /// </summary>
    /// <param name="resultType">操作结果类型</param>
    /// <param name="message">返回消息</param>
    /// <param name="logMessage">日志记录消息</param>
    public OperationResultBase(OperationResultType resultType, string message, string logMessage)
        : this(resultType, message)
    {
        LogMessage = logMessage;
    }

    /// <summary>
    /// 初始化一个 定义返回消息、日志消息与附加数据的操作结果信息类 的新实例
    /// </summary>
    /// <param name="resultType">操作结果类型</param>
    /// <param name="message">返回消息</param>
    /// <param name="logMessage">日志记录消息</param>
    /// <param name="appendData">返回数据</param>
    public OperationResultBase(OperationResultType resultType, string message, string logMessage, T? appendData)
        : this(resultType, message, logMessage)
    {
        _AppendData = appendData;
    }

    #endregion

    #region 属性

    public OperationResultType ResultType { get; set; }

    public string Message { get; set; } = string.Empty;

    public string LogMessage { get; set; } = string.Empty;

    protected T? _AppendData;

    public virtual T AppendData
    {
        get
        {
            _AppendData ??= Activator.CreateInstance<T>();
            return _AppendData;
        }
        set => _AppendData = value;
    }

    #endregion
}
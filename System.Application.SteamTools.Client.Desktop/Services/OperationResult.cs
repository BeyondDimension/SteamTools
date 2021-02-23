namespace System.Application.Services
{
    /// <summary>
    /// 操作结果信息类，对操作结果进行封装
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OperationResult<T>
    {
        #region 构造函数

        public OperationResult()
        {
            ResultType = OperationResultType.Error;
        }

        /// <summary>
        /// 初始化一个 操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        public OperationResult(OperationResultType resultType)
        {
            ResultType = resultType;
        }

        /// <summary>
        /// 初始化一个 定义返回消息的操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        /// <param name="message">返回消息</param>
        public OperationResult(OperationResultType resultType, string message)
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
        public OperationResult(OperationResultType resultType, string message, T appendData)
            : this(resultType, message)
        {
            AppendData = appendData;
        }

        /// <summary>
        /// 初始化一个 定义返回消息与日志消息的操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        /// <param name="message">返回消息</param>
        /// <param name="logMessage">日志记录消息</param>
        public OperationResult(OperationResultType resultType, string message, string logMessage)
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
        public OperationResult(OperationResultType resultType, string message, string logMessage, T appendData)
            : this(resultType, message, logMessage)
        {
            AppendData = appendData;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置 操作结果类型
        /// </summary>
        public OperationResultType ResultType { get; set; }

        /// <summary>
        /// 获取或设置 操作返回信息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 获取或设置 操作返回的日志消息，用于记录日志
        /// </summary>
        public string? LogMessage { get; set; }

        /// <summary>
        /// 获取或设置 操作结果附加信息
        /// </summary>
        public T? AppendData { get; set; }

        #endregion
    }

    /// <inheritdoc cref="OperationResult{T}"/>
    public class OperationResult : OperationResult<object> { }
}
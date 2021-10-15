using System.Diagnostics.CodeAnalysis;

namespace System.Application.Services
{
    /// <summary>
    /// 操作结果信息类，对操作结果进行封装
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class OperationResultBase<T>
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
            AppendData = appendData;
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
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置 操作返回的日志消息，用于记录日志
        /// </summary>
        public string LogMessage { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置 操作结果附加信息
        /// </summary>
        public abstract T? AppendData { get; set; }

        #endregion
    }

    /// <inheritdoc cref="OperationResultBase{T}"/>
    public class OperationResult : OperationResultBase<object>
    {
        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase"/>
        public OperationResult()
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType)"/>
        public OperationResult(OperationResultType resultType) : base(resultType)
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string)"/>
        public OperationResult(OperationResultType resultType, string message) : base(resultType, message)
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, T)"/>
        public OperationResult(OperationResultType resultType, string message, object? appendData) : base(resultType, message, appendData)
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, string)"/>
        public OperationResult(OperationResultType resultType, string message, string logMessage) : base(resultType, message, logMessage)
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, string, T)"/>
        public OperationResult(OperationResultType resultType, string message, string logMessage, object? appendData) : base(resultType, message, logMessage, appendData)
        {
        }

        public sealed override object? AppendData { get; set; }
    }

    public class OperationResult<T> : OperationResultBase<T> where T : new()
    {
        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase"/>
        public OperationResult()
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType)"/>
        public OperationResult(OperationResultType resultType) : base(resultType)
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string)"/>
        public OperationResult(OperationResultType resultType, string message) : base(resultType, message)
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, T)"/>
        public OperationResult(OperationResultType resultType, string message, T appendData) : base(resultType, message, appendData)
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, string)"/>
        public OperationResult(OperationResultType resultType, string message, string logMessage) : base(resultType, message, logMessage)
        {
        }

        /// <inheritdoc cref="OperationResultBase{T}.OperationResultBase(OperationResultType, string, string, T)"/>
        public OperationResult(OperationResultType resultType, string message, string logMessage, T appendData) : base(resultType, message, logMessage, appendData)
        {
        }

        [NotNull, DisallowNull] // C# 8 not null
#pragma warning disable CS8765 // 参数类型的为 Null 性与重写成员不匹配(可能是由于为 Null 性特性)。
        public sealed override T? AppendData { get; set; } = new T();
#pragma warning restore CS8765 // 参数类型的为 Null 性与重写成员不匹配(可能是由于为 Null 性特性)。
    }
}
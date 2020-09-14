using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SteamTool.Core.Common
{
    /// <summary>
    ///     操作结果信息类，对操作结果进行封装
    /// </summary>
    public class OperationResult
    {
        #region 构造函数

        public OperationResult()
        {
            ResultType = OperationResultType.Error;
        }

        /// <summary>
        ///     初始化一个 操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        public OperationResult(OperationResultType resultType)
        {
            ResultType = resultType;
        }

        /// <summary>
        ///     初始化一个 定义返回消息的操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        /// <param name="message">返回消息</param>
        public OperationResult(OperationResultType resultType, string message)
            : this(resultType)
        {
            Message = message;
        }

        /// <summary>
        ///     初始化一个 定义返回消息与附加数据的操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        /// <param name="message">返回消息</param>
        /// <param name="appendData">返回数据</param>
        public OperationResult(OperationResultType resultType, string message, object appendData)
            : this(resultType, message)
        {
            AppendData = appendData;
        }

        /// <summary>
        ///     初始化一个 定义返回消息与日志消息的操作结果信息类 的新实例
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
        ///     初始化一个 定义返回消息、日志消息与附加数据的操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        /// <param name="message">返回消息</param>
        /// <param name="logMessage">日志记录消息</param>
        /// <param name="appendData">返回数据</param>
        public OperationResult(OperationResultType resultType, string message, string logMessage, object appendData)
            : this(resultType, message, logMessage)
        {
            AppendData = appendData;
        }

        #endregion

        #region 属性

        /// <summary>
        ///     获取或设置 操作结果类型
        /// </summary>
        public OperationResultType ResultType { get; set; }

        /// <summary>
        ///     获取或设置 操作返回信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     获取或设置 操作返回的日志消息，用于记录日志
        /// </summary>
        public string LogMessage { get; set; }

        /// <summary>
        ///     获取或设置 操作结果附加信息
        /// </summary>
        public object AppendData { get; set; }

        #endregion
    }
    /// <summary>
    ///     操作结果信息类，对操作结果进行封装
    /// </summary>
    public class OperationResult<T> where T : new()
    {
        #region 构造函数

        public OperationResult()
        {
            ResultType = OperationResultType.Error;
        }

        /// <summary>
        ///     初始化一个 操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        public OperationResult(OperationResultType resultType)
        {
            ResultType = resultType;
        }

        /// <summary>
        ///     初始化一个 定义返回消息的操作结果信息类 的新实例
        /// </summary>
        /// <param name="resultType">操作结果类型</param>
        /// <param name="message">返回消息</param>
        public OperationResult(OperationResultType resultType, string message)
            : this(resultType)
        {
            Message = message;
        }

        /// <summary>
        ///     初始化一个 定义返回消息与附加数据的操作结果信息类 的新实例
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
        ///     初始化一个 定义返回消息与日志消息的操作结果信息类 的新实例
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
        ///     初始化一个 定义返回消息、日志消息与附加数据的操作结果信息类 的新实例
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
        ///     获取或设置 操作结果类型
        /// </summary>
        public OperationResultType ResultType { get; set; }

        /// <summary>
        ///     获取或设置 操作返回信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     获取或设置 操作返回的日志消息，用于记录日志
        /// </summary>
        public string LogMessage { get; set; }

        /// <summary>
        ///     获取或设置 操作结果附加信息
        /// </summary>
        public T AppendData { get; set; }

        #endregion
    }

    /// <summary>
    ///     表示操作结果的枚举
    /// </summary>
    [Description("操作结果的枚举")]
    public enum OperationResultType
    {
        /// <summary>
        ///    0 - 操作成功
        /// </summary>
        [Description("操作成功。")]
        Success,

        /// <summary>
        ///    1 - 操作取消或操作没引发任何变化
        /// </summary>
        [Description("操作没有引发任何变化，提交取消。")]
        NoChanged,

        /// <summary>
        ///    2 - 参数错误
        /// </summary>
        [Description("参数错误。")]
        ParamError,

        /// <summary>
        ///    3 - 指定参数的数据不存在
        /// </summary>
        [Description("指定参数的数据不存在。")]
        QueryNull,

        /// <summary>
        ///    4 - 权限不足
        /// </summary>
        [Description("当前用户权限不足，不能继续操作。")]
        PurviewLack,

        /// <summary>
        ///    5 - 登录超时
        /// </summary>
        [Description("登录超时")]
        LoginTimeOut,

        /// <summary>
        ///    6 - 非法操作
        /// </summary>
        [Description("非法操作。")]
        IllegalOperation,

        /// <summary>
        ///    7 - 警告
        /// </summary>
        [Description("警告")]
        Warning,

        /// <summary>
        ///    8 - 操作引发错误
        /// </summary>
        [Description("操作引发错误。")]
        Error,
    }
}

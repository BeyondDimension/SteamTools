using System.Diagnostics.CodeAnalysis;

namespace System.Application.Models
{
    /// <summary>
    /// API响应模型
    /// </summary>
    public interface IApiResponse
    {
        /// <summary>
        /// 响应码
        /// </summary>
        ApiResponseCode Code { get; set; }

        /// <summary>
        /// 显示消息
        /// </summary>
        string Message => ApiResponse.GetMessage(this);

        /// <summary>
        /// 获取显示消息并在末尾追加文本
        /// </summary>
        /// <param name="errorAppendText"></param>
        /// <returns></returns>
        string GetMessageByAppendText(string? errorAppendText) => ApiResponse.GetMessage(this, errorAppendText);

        /// <summary>
        /// 通过自定义格式化文本获取显示消息
        /// </summary>
        /// <param name="errorFormat"></param>
        /// <param name="errorAppendText"></param>
        /// <returns></returns>
        string GetMessageByFormat(string errorFormat, string? errorAppendText = null) => ApiResponse.GetMessage(this, errorAppendText, errorFormat);

        /// <summary>
        /// 内部显示消息
        /// </summary>
        internal string? InternalMessage { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        /// <returns></returns>
        bool IsSuccess { get; }
    }

    /// <inheritdoc cref="IApiResponse"/>
    public interface IApiResponse<out T> : IApiResponse
    {
        /// <summary>
        /// 附加内容
        /// </summary>
        [MaybeNull]
        T Content { get; }
    }
}
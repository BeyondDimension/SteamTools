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
        [Obsolete("use IsSuccess")]
        string? Message { get; set; }

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
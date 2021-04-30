using System.Application.Models.Abstractions;

namespace System.Application.Models
{
    /// <summary>
    /// 验证短信验证码接口返回模型
    /// </summary>
    public interface ICheckSmsResult : ISmsResult
    {
        /// <summary>
        /// 是否校验成功
        /// </summary>
        bool IsCheckSuccess { get; }
    }
}
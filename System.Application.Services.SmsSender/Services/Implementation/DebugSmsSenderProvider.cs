using System.Application.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <summary>
    /// 调试模式下可使用此实现，验证码全输6
    /// </summary>
    public sealed class DebugSmsSenderProvider : ISmsSender
    {
        public const string Name = nameof(DebugSmsSenderProvider);

        public string Channel => Name;

        public bool SupportCheck => true;

        public Task<ICheckSmsResult> CheckSmsAsync(string __, string ___, CancellationToken _)
        {
            return Task.FromResult<ICheckSmsResult>(new CheckSmsResult
            {
                IsSuccess = true,
                IsCheckSuccess = true,
            });
        }

        public Task<ISendSmsResult> SendSmsAsync(string __, string ___, ushort ____, CancellationToken _)
        {
            return Task.FromResult<ISendSmsResult>(new SendSmsResult
            {
                IsSuccess = true,
            });
        }

        public string GenerateRandomNum(int length)
        {
            return string.Join(null, new char[length].Select(x => '6'));
        }
    }
}
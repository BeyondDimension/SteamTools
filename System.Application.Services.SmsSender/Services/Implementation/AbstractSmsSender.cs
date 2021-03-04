using System.Application.Models;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    public abstract class AbstractSmsSender : ISmsSender
    {
        public abstract string Channel { get; }

        public abstract bool SupportCheck { get; }

        public abstract Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken);

        public abstract Task<ISendSmsResult> SendSmsAsync(string number, string message, int type, CancellationToken cancellationToken);

        /// <summary>
        /// 生成随机短信验证码值，某些平台可能提供了随机生成，可以重写该函数替换
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public virtual string GenerateRandomNum(int length)
        {
            return Random2.GenerateRandomNum((ushort)length).ToString();
        }
    }
}
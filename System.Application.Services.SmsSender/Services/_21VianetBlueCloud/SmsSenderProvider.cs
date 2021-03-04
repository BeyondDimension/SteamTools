using System.Application.Models;
using System.Application.Services.Implementation;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services._21VianetBlueCloud
{
    /// <summary>
    /// 短信服务提供商 - 世纪互联蓝云
    /// </summary>
    public class SmsSenderProvider : AbstractSmsSender, ISmsSender
    {
        public const string Name = nameof(_21VianetBlueCloud);

        public override string Channel => Name;

        public override bool SupportCheck => throw new NotImplementedException();

        public override Task<ISendSmsResult> SendSmsAsync(string number, string message, int type, CancellationToken cancellationToken)
        {
            // 明天再写
            throw new NotImplementedException();
        }

        public override Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
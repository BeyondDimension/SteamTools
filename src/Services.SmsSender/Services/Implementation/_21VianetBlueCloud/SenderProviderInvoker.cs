using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Application.Models;

namespace System.Application.Services.Implementation._21VianetBlueCloud;

internal sealed class SenderProviderInvoker<TSmsSettings> : SmsSenderProvider
    where TSmsSettings : class, ISmsSettings
{
    public SenderProviderInvoker(ILogger<SenderProviderInvoker<TSmsSettings>> logger, IOptions<TSmsSettings> settings, HttpClient httpClient) : base(logger, settings.Value?.SmsOptions?._21VianetBlueCloud, httpClient)
    {
    }
}
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Application.Models;

namespace System.Application.Services.Implementation;

internal sealed class SmsSenderWrapper<TSmsSender, TSmsSettings> : ISmsSender
    where TSmsSender : ISmsSender
    where TSmsSettings : class, ISmsSettings
{
    readonly ISmsSender smsSender;

    public SmsSenderWrapper(
      IWebHostEnvironment hostingEnvironment,
      IOptions<TSmsSettings> options,
      TSmsSender smsSender,
      DebugSmsSenderProvider debug)
    {
        this.smsSender = smsSender;

        var settings = options.Value;

        if (settings != null)
        {
            if (settings.SmsOptions == null || ((settings.UseDebugSmsSender.HasValue && settings.UseDebugSmsSender.Value) || (!settings.UseDebugSmsSender.HasValue
                && hostingEnvironment.IsDevelopment())))
            {
                this.smsSender = debug;
            }
        }
    }

    public string Channel => smsSender.Channel;

    public bool SupportCheck => smsSender.SupportCheck;

    public Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken)
    {
        return smsSender.CheckSmsAsync(number, message, cancellationToken);
    }

    public Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken)
    {
        return smsSender.SendSmsAsync(number, message, type, cancellationToken);
    }

    public string GenerateRandomNum(int length)
    {
        return smsSender.GenerateRandomNum(length);
    }
}
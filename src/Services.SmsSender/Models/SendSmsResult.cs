using System.Application.Models.Abstractions;

namespace System.Application.Models
{
    public sealed class SendSmsResult : SmsResult<SendSmsResult>, ISendSmsResult
    {
    }

    public class SendSmsResult<TResult> : SmsResult<TResult, SendSmsResult<TResult>>, ISendSmsResult
      where TResult : JsonModel
    {
    }
}
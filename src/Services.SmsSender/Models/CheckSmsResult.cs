using System.Application.Models.Abstractions;

namespace System.Application.Models
{
    public sealed class CheckSmsResult : SmsResult<CheckSmsResult>, ICheckSmsResult
    {
        public bool IsCheckSuccess { get; set; }
    }

    public class CheckSmsResult<TResult> : SmsResult<TResult, CheckSmsResult<TResult>>, ICheckSmsResult
      where TResult : JsonModel
    {
        public bool IsCheckSuccess { get; set; }
    }
}
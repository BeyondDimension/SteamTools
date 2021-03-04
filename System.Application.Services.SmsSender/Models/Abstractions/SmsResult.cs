using System.Application.Columns;

namespace System.Application.Models.Abstractions
{
    public abstract class SmsResult<TImplement> :
        JsonModel<TImplement>,
        IResult<object?>
        where TImplement : SmsResult<TImplement>
    {
        public bool IsSuccess { get; set; }

        public object? Result { get; set; }

        public int HttpStatusCode { get; set; }
    }

    public class SmsResult<TResult, TImplement> : SmsResult<TImplement>, ISmsResult, IResult<TResult?>
       where TResult : JsonModel
       where TImplement : SmsResult<TResult, TImplement>
    {
        public new TResult? Result { get; set; }

        public object? ResultObject { get => base.Result; set => base.Result = value; }

        object? ISmsResult.Result => ResultObject;
    }
}
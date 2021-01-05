using System;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;

namespace Titanium.Web.Proxy.Extensions
{
    internal static class FuncExtensions
    {
        internal static async Task InvokeAsync<T>(this AsyncEventHandler<T> callback, object sender, T args,
            ExceptionHandler exceptionFunc)
        {
            var invocationList = callback.GetInvocationList();

            foreach (var @delegate in invocationList)
            {
                await internalInvokeAsync((AsyncEventHandler<T>)@delegate, sender, args, exceptionFunc);
            }
        }

        private static async Task internalInvokeAsync<T>(AsyncEventHandler<T> callback, object sender, T args,
            ExceptionHandler exceptionFunc)
        {
            try
            {
                await callback(sender, args);
            }
            catch (Exception e)
            {
                exceptionFunc(new Exception("Exception thrown in user event", e));
            }
        }
    }
}

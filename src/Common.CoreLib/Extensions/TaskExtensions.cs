// https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class TaskExtensions
    {
        static readonly TaskFactory _myTaskFactory = new(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static TResult RunSync<TResult>(this Func<Task<TResult>> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }

        public static void RunSync(this Func<Task> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }

        public static TResult RunSync<TResult>(this Func<ValueTask<TResult>> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func().AsTask();
            }).Unwrap().GetAwaiter().GetResult();
        }

        public static void RunSync(this Func<ValueTask> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func().AsTask();
            }).Unwrap().GetAwaiter().GetResult();
        }
    }
}
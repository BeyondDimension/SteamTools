using Android.App;
using Android.Content;
using Android.OS;
using NativeService = System.Application.Services.Native.IServiceBase;

// ReSharper disable once CheckNamespace
namespace System
{
    partial class ContextExtensions
    {
        /// <summary>
        /// 启动或关闭前台服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="context"></param>
        /// <param name="startOrStop"></param>
        public static void StartOrStopForegroundService<TService>(this Context context, bool startOrStop) where TService : Service, NativeService
        {
            Intent intent = new(context, typeof(TService));
            intent.SetAction(startOrStop ? NativeService.START : NativeService.STOP);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                context.StartForegroundService(intent);
            }
            else
            {
                context.StartService(intent);
            }
        }
    }
}

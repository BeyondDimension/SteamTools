//extern alias NUGET_PACKAGE_TOAST_IOS;

//using Microsoft.Extensions.DependencyInjection;
//using System.Threading.Tasks;
//using iOSToast = NUGET_PACKAGE_TOAST_IOS::GlobalToast.Toast;

//// TODO Use CommunityToolkit.Maui.Core.Views.PlatformToast
//// https://github.com/CommunityToolkit/Maui/blob/main/src/CommunityToolkit.Maui/Alerts/Toast/Toast.macios.cs
//// https://github.com/CommunityToolkit/Maui/blob/main/src/CommunityToolkit.Maui.Core/Views/Toast/PlatformToast.macios.cs

//namespace System.Application.Services.Implementation
//{
//    /// <summary>
//    /// https://github.com/andrius-k/Toast
//    /// </summary>
//    internal sealed class PlatformToastImpl : ToastBaseImpl
//    {
//        public PlatformToastImpl(IToastIntercept intercept) : base(intercept)
//        {
//        }

//        protected override async void PlatformShow(string text, int duration)
//        {
//            var toast = iOSToast.ShowToast(text)
//                .SetDuration(duration)
//                .SetShowShadow(false)
//                .SetAutoDismiss(true)
//                .Show();
//            await Task.Delay(duration);
//            toast.Dismiss();
//        }

//        internal static IServiceCollection TryAddToast(IServiceCollection services)
//            => TryAddToast<PlatformToastImpl>(services);
//    }
//}
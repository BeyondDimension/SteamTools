using Android.Content;
using Android.Telephony;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ITelephonyService"/>
    internal sealed class PlatformTelephonyServiceImpl : TelephonyServiceImpl
    {
        public PlatformTelephonyServiceImpl(IPermissions pf, IPermissions.IGetPhoneNumber p1) : base(pf, p1)
        {
        }

        protected override string? PlatformGetPhoneNumber()
        {
            var telephony = XEPlatform.AppContext.GetSystemService<TelephonyManager>(Context.TelephonyService);
            var value = telephony.Line1Number;
            return PhoneNumberHelper.GetChineseMainlandPhoneNumber11(value);
        }
    }
}
using Android.Content;
using Android.Telephony;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ITelephonyService"/>
    internal sealed class PlatformTelephonyServiceImpl : TelephonyServiceImpl
    {
        public PlatformTelephonyServiceImpl(IPermissions p) : base(p)
        {
        }

        TelephonyManager? telephonyManager;

        public TelephonyManager TelephonyManager
        {
            get
            {
                if (telephonyManager == null)
                    telephonyManager = XEPlatform.AppContext.GetSystemService<TelephonyManager>(Context.TelephonyService);
                return telephonyManager;
            }
        }

        protected override string? PlatformGetPhoneNumber()
        {
            var value = TelephonyManager.Line1Number;
            return PhoneNumberHelper.GetChineseMainlandPhoneNumber(value);
        }
    }
}
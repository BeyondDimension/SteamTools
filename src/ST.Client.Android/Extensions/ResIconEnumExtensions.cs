using Android.Widget;
using System.Application;
using System.Application.UI;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ResIconEnumExtensions
    {
        public static int? ToImageResource(this ResIcon icon)
        {
            var resId = icon switch
            {
                ResIcon.AvaterDefault => Resource.Drawable.avater_default,
                ResIcon.AccountBox => Resource.Drawable.baseline_account_box_black_24,
                ResIcon.Info => Resource.Drawable.baseline_info_black_24,
                ResIcon.Person => Resource.Drawable.baseline_person_black_24,
                ResIcon.PlatformPhone => Resource.Drawable.baseline_phone_android_black_24,
                ResIcon.Settings => Resource.Drawable.baseline_settings_black_24,
                ResIcon.SportsEsports => Resource.Drawable.baseline_sports_esports_black_24,
                ResIcon.VerifiedUser => Resource.Drawable.baseline_verified_user_black_24,
                ResIcon.Steam => Resource.Drawable.icon_steam_24,
                ResIcon.Xbox => Resource.Drawable.icon_xbox_24,
                ResIcon.Apple => Resource.Drawable.icon_apple_24,
                ResIcon.QQ => Resource.Drawable.icon_qq_24,
                ResIcon.Phone => Resource.Drawable.icon_phone_24,
                _ => (int?)null,
            };
            return resId;
        }

        public static void SetImageResourceIcon(this ImageView imageView, ResIcon icon)
        {
            var resId = icon.ToImageResource();
            if (resId.HasValue)
            {
                imageView.SetImageResource(resId.Value);
            }
            else
            {
                imageView.SetImageDrawable(null);
            }
        }
    }
}

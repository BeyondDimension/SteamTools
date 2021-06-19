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
                ResIcon.avater_default => Resource.Drawable.avater_default,
                ResIcon.baseline_account_box_black_24 => Resource.Drawable.baseline_account_box_black_24,
                ResIcon.baseline_info_black_24 => Resource.Drawable.baseline_info_black_24,
                ResIcon.baseline_person_black_24 => Resource.Drawable.baseline_person_black_24,
                ResIcon.baseline_phone_black_24 => Resource.Drawable.baseline_phone_android_black_24,
                ResIcon.baseline_settings_black_24 => Resource.Drawable.baseline_settings_black_24,
                ResIcon.baseline_sports_esports_black_24 => Resource.Drawable.baseline_sports_esports_black_24,
                ResIcon.baseline_verified_user_black_24 => Resource.Drawable.baseline_verified_user_black_24,
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

using Android.Util;
using AndroidX.SwipeRefreshLayout.Widget;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.UI
{
    public static class Styles
    {
        public static void InitDefaultStyles(this SwipeRefreshLayout swipeRefreshLayout)
        {
            swipeRefreshLayout.SetColorSchemeColors(new[] { ColorPrimary });
        }

        public static int ColorPrimary
        {
            get
            {
                using TypedValue typedValue = new();
                XEPlatform.CurrentActivity.Theme!.ResolveAttribute(Resource.Attribute.colorPrimary, typedValue, true);
                return typedValue.Data;
            }
        }
    }
}
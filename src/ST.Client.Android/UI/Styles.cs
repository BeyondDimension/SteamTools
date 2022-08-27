using Android.Util;
using AndroidX.SwipeRefreshLayout.Widget;
#if NET6_0_OR_GREATER
using XEPlatform = Microsoft.Maui.ApplicationModel.Platform;
#else
using XEPlatform = Xamarin.Essentials.Platform;
#endif

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
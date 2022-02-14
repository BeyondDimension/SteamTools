using Android.Views;
using AndroidX.Core.Widget;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ScrollViewExtensions
    {
        public static void ScrollToEnd(this NestedScrollView scrollView)
        {
            scrollView.Post(() =>
            {
                scrollView.FullScroll((int)FocusSearchDirection.Down);
            });
        }
    }
}

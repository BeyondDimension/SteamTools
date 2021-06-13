using System;

using Android.App;
using Android.Views;

namespace Xamarin.Android.Design
{
    public delegate Java.Lang.Object OnLayoutItemNotFoundHandler(int resourceId, Type expectedViewType);

    abstract class LayoutBinding
    {
        Activity boundActivity;
        View boundView;
        OnLayoutItemNotFoundHandler onLayoutItemNotFound;

        protected LayoutBinding(Activity activity, OnLayoutItemNotFoundHandler onLayoutItemNotFound = null)
        {
            boundActivity = activity ?? throw new ArgumentNullException(nameof(activity));
            this.onLayoutItemNotFound = onLayoutItemNotFound;
        }

        protected LayoutBinding(View view, OnLayoutItemNotFoundHandler onLayoutItemNotFound = null)
        {
            boundView = view ?? throw new ArgumentNullException(nameof(view));
            this.onLayoutItemNotFound = onLayoutItemNotFound;
        }

        protected T FindView<T>(int resourceId, ref T cachedField) where T : View
        {
            if (cachedField != null)
                return cachedField;

            T ret;
            if (boundActivity != null)
                ret = boundActivity.FindViewById<T>(resourceId);
            else
                ret = boundView.FindViewById<T>(resourceId);

            if (ret == null && onLayoutItemNotFound != null)
                ret = (T)onLayoutItemNotFound(resourceId, typeof(T));

            if (ret == null)
                throw new global::System.InvalidOperationException($"View not found (Resource ID: {resourceId})");

            cachedField = ret;
            return ret;
        }

        Activity EnsureActivity()
        {
            if (boundActivity != null)
                return boundActivity;

            var ret = boundView?.Context as Activity;
            if (ret != null)
                return ret;

            throw new InvalidOperationException("Finding fragments is supported only for Activity instances");
        }

        T __FindFragment<T>(int resourceId, Func<Activity, T> finder, ref T cachedField) where T : Java.Lang.Object
        {
            if (cachedField != null)
                return cachedField;

            var ret = finder(EnsureActivity());
            if (ret == null && onLayoutItemNotFound != null)
                ret = (T)onLayoutItemNotFound(resourceId, typeof(T));

            if (ret == null)
                throw new InvalidOperationException($"Fragment not found (ID: {resourceId}; Type: {typeof(T)})");

            cachedField = ret;
            return ret;
        }
#if __ANDROID_11__
        //protected T FindFragment<T>(int resourceId, global::Android.App.Fragment __ignoreMe, ref T cachedField) where T : global::Android.App.Fragment
        //{
        //    return __FindFragment<T>(resourceId, (activity) => activity.FragmentManager.FindFragmentById<T>(resourceId), ref cachedField);
        //}
#endif  // __ANDROID_11__

#if __HAVE_SUPPORT__
		protected T FindFragment <T> (int resourceId, global::Android.Support.V4.App.Fragment __ignoreMe, ref T cachedField) where T: global::Android.Support.V4.App.Fragment
		{
			return __FindFragment<T> (resourceId, (activity) => activity.FragmentManager.FindFragmentById<T> (resourceId), ref cachedField);
		}
#endif // __HAVE_SUPPORT__

#if __HAVE_ANDROIDX__
        protected T FindFragment<T>(int resourceId, global::AndroidX.Fragment.App.Fragment __ignoreMe, ref T cachedField) where T : global::AndroidX.Fragment.App.Fragment
        {
            return __FindFragment(resourceId, (activity) =>
            {
                if (activity is AndroidX.AppCompat.App.AppCompatActivity activity_)
                {
                    return global::Android.Runtime.Extensions.JavaCast<T>(activity_.SupportFragmentManager.FindFragmentById(resourceId));
                }
                else
                {
                    throw new InvalidCastException(activity.GetType().ToString());
                }
            }, ref cachedField);
        }
#endif // __HAVE_ANDROIDX__
    }
}
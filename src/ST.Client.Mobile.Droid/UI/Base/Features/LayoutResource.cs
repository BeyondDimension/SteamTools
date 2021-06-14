// 通过重写 LayoutResource => Resource.Layout.??? 实现指定布局资源

using Android.App;
using Android.Views;
using AndroidX.AppCompat.App;
using Fragment = AndroidX.Fragment.App.Fragment;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Activities
{
    partial class BaseActivity
    {
        /// <summary>
        /// 当前 <see cref="AppCompatActivity"/> 指定布局资源
        /// </summary>
        protected abstract int? LayoutResource { get; }

        internal static void SetContentView(Activity activity, int? layoutResID)
        {
            if (layoutResID.HasValue) activity.SetContentView(layoutResID.Value);
        }
    }

    partial class BaseActivity<TViewBinding, TViewModel>
    {
        /// <summary>
        /// 当前 <see cref="AppCompatActivity"/> 指定布局资源
        /// </summary>
        protected abstract int? LayoutResource { get; }
    }
}

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    partial class BaseFragment
    {
        /// <summary>
        /// 当前 <see cref="Fragment"/> 指定布局资源
        /// </summary>
        protected abstract int? LayoutResource { get; }

        internal static View? CreateView(int? layoutResID, LayoutInflater inflater, ViewGroup container)
        {
            if (!layoutResID.HasValue) return null;
            var view = inflater.Inflate(layoutResID.Value, container, false);
            if (view == null)
                throw new ArgumentOutOfRangeException(nameof(view), layoutResID.Value, null);
            return view;
        }
    }

    partial class BaseFragment<TViewBinding, TViewModel>
    {
        /// <summary>
        /// 当前 <see cref="Fragment"/> 指定布局资源
        /// </summary>
        protected abstract int? LayoutResource { get; }
    }
}
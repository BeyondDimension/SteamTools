using Android.Views;
using AndroidX.Fragment.App;
using System.Application.UI.Internals;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace System.Application.UI.Fragments
{
    /// <summary>
    /// 片段(Fragment)
    /// </summary>
    public interface IFragment : View.IOnClickListener
    {
        /// <inheritdoc cref="Fragment.Activity"/>
        FragmentActivity? Activity { get; }

        Fragment Fragment { get; }

        View View { get; set; }

        /// <summary>
        /// 当前 <see cref="Fragment"/> 指定布局资源
        /// </summary>
        int? LayoutResource { get; }

        void OnCreateView(View view);

        /// <summary>
        /// 控件的点击事件
        /// <para>根据返回值决定是否不调用 <see cref="Fragment.Activity"/> 中的 <see cref="View.IOnClickListener.OnClick(View?)"/></para>
        /// <para>默认返回值 <see langword="false"/></para>
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        new bool OnClick(View view);

        /// <summary>
        /// 是否在显示当前 Fragment 时启用亮色状态栏
        /// 默认值：<see langword="null"/>
        /// 亮色：<see langword="true"/>
        /// 暗色：<see langword="false"/>
        /// </summary>
        bool? IsLightStatusBar { get; }

        protected static void ChangeStatusBar(IFragment f)
        {
            var isLightStatusBar = f.IsLightStatusBar;
            if (isLightStatusBar.HasValue)
            {
                var activity = f.Activity;
                if (activity != null)
                {
                    if (isLightStatusBar.Value && DarkModeUtil.IsDarkModeQ)
                    {
                        isLightStatusBar = false;
                    }
                    StatusBarUtil.SetWindowLightStatusBar(activity, isLightStatusBar.Value);
                }
            }
        }

        protected static View? OnCreateView(IFragment f, LayoutInflater inflater, ViewGroup container)
        {
            if (!f.LayoutResource.HasValue) return null;
            var view = inflater.Inflate(f.LayoutResource.Value, container, false);
            if (view == null)
                throw new ArgumentOutOfRangeException(nameof(view), f.LayoutResource.Value, null);
            f.View = view;
            if (f is IToolbar toolbar)
            {
                toolbar.BindingToolbar();
            }
            f.OnCreateView(view);
            return view;
        }

        protected static void OnClick(IFragment f, View? view)
        {
            if (view != null)
            {
                var stopCall = f.OnClick(view);
                if (!stopCall && f.Activity is View.IOnClickListener listener)
                {
                    listener.OnClick(view);
                }
            }
        }

        protected static T? FindViewById<T>(IFragment f, int id) where T : View
            => f.View.FindViewById<T>(id);

        protected static View? FindViewById(IFragment f, int id) => f.View.FindViewById(id);
    }

    partial class BaseFragment : IFragment
    {
        Fragment IFragment.Fragment => this;

        int? IFragment.LayoutResource => LayoutResource;

        void IFragment.OnCreateView(View view) => OnCreateView(view);

        bool IFragment.OnClick(View view) => OnClick(view);

        bool? IFragment.IsLightStatusBar => IsLightStatusBar;
    }

    partial class BaseDialogFragment : IFragment
    {
        Fragment IFragment.Fragment => this;

        int? IFragment.LayoutResource => LayoutResource;

        void IFragment.OnCreateView(View view) => OnCreateView(view);

        bool IFragment.OnClick(View view) => OnClick(view);

        bool? IFragment.IsLightStatusBar => IsLightStatusBar;
    }

    partial class BaseBottomSheetDialogFragment : IFragment
    {
        Fragment IFragment.Fragment => this;

        int? IFragment.LayoutResource => LayoutResource;

        void IFragment.OnCreateView(View view) => OnCreateView(view);

        bool IFragment.OnClick(View view) => OnClick(view);

        bool? IFragment.IsLightStatusBar => IsLightStatusBar;
    }
}
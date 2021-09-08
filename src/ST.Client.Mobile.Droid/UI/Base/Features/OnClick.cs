// 通过重写 OnClick(View? view) 实现控件点击时逻辑
// 通过 SetOnClickListener(view1, view2, ...) 绑定控件点击事件

using Android.Views;
using System.Application.UI.Activities;
using System.Collections.Generic;
using System.Linq;
using Fragment = AndroidX.Fragment.App.Fragment;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Activities
{
    partial class BaseActivity : View.IOnClickListener
    {
        /// <summary>
        /// 控件的点击事件
        /// </summary>
        /// <param name="view"></param>
        protected virtual void OnClick(View view)
        {
        }

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view == null) return;
            OnClick(view);
        }

        internal static void SetOnClickListener(View.IOnClickListener listener, List<View>? clickViews, IEnumerable<View> views)
        {
            foreach (var view in views)
            {
                view.SetOnClickListener(listener);
            }
            if (clickViews == null) clickViews = new();
            clickViews.AddRange(views);
        }

        internal static void ClearOnClickListener(ref List<View>? clickViews)
        {
            if (clickViews != null)
            {
                foreach (var item in clickViews)
                {
                    item.SetOnClickListener(null);
                }
                clickViews = null;
            }
        }

        List<View>? clickViews;
        protected void SetOnClickListener(IEnumerable<View> views) => SetOnClickListener(this, clickViews, views);
        protected void SetOnClickListener(params View[] views) => SetOnClickListener(views.AsEnumerable());
        void ClearOnClickListener() => ClearOnClickListener(ref clickViews);
    }

    partial class BaseActivity<TViewBinding, TViewModel> : View.IOnClickListener
    {
        /// <summary>
        /// 控件的点击事件
        /// </summary>
        /// <param name="view"></param>
        protected virtual void OnClick(View view)
        {
        }

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view == null) return;
            OnClick(view);
        }

        List<View>? clickViews;
        protected void SetOnClickListener(IEnumerable<View> views) => BaseActivity.SetOnClickListener(this, clickViews, views);
        protected void SetOnClickListener(params View[] views) => SetOnClickListener(views.AsEnumerable());
        void ClearOnClickListener() => BaseActivity.ClearOnClickListener(ref clickViews);
    }
}

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    partial class BaseFragment : View.IOnClickListener
    {
        internal static void OnClick(Fragment fragment, View view, Func<View, bool> onClick)
        {
            var stopCall = onClick(view);
            if (!stopCall && fragment.Activity is View.IOnClickListener listener)
            {
                listener.OnClick(view);
            }
        }

        /// <summary>
        /// 控件的点击事件
        /// <para>根据返回值决定是否不调用 <see cref="Fragment.Activity"/> 中的 <see cref="View.IOnClickListener.OnClick(View?)"/></para>
        /// <para>默认返回值 <see langword="false"/></para>
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual bool OnClick(View view) => false;

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view == null) return;
            OnClick(this, view, OnClick);
        }

        List<View>? clickViews;
        protected void SetOnClickListener(IEnumerable<View> views) => BaseActivity.SetOnClickListener(this, clickViews, views);
        protected void SetOnClickListener(params View[] views) => SetOnClickListener(views.AsEnumerable());
        void ClearOnClickListener() => BaseActivity.ClearOnClickListener(ref clickViews);
    }

    partial class BaseFragment<TViewBinding, TViewModel> : View.IOnClickListener
    {
        /// <summary>
        /// 控件的点击事件
        /// <para>根据返回值决定是否不调用 <see cref="Fragment.Activity"/> 中的 <see cref="View.IOnClickListener.OnClick(View?)"/></para>
        /// <para>默认返回值 <see langword="false"/></para>
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual bool OnClick(View view) => false;

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view == null) return;
            BaseFragment.OnClick(this, view, OnClick);
        }

        List<View>? clickViews;
        protected void SetOnClickListener(IEnumerable<View> views) => BaseActivity.SetOnClickListener(this, clickViews, views);
        protected void SetOnClickListener(params View[] views) => SetOnClickListener(views.AsEnumerable());
        void ClearOnClickListener() => BaseActivity.ClearOnClickListener(ref clickViews);
    }
}
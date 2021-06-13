using Android.Views;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.UI.Activities
{
    partial class BaseActivity : View.IOnClickListener
    {
        /// <summary>
        /// 控件的点击事件
        /// </summary>
        /// <param name="view"></param>
        public virtual void OnClick(View? view)
        {
        }

        List<View>? clickViews;
        protected void SetOnClickListener(IEnumerable<View> views)
        {
            foreach (var view in views)
            {
                view.SetOnClickListener(this);
            }
            if (clickViews == null) clickViews = new();
            clickViews.AddRange(views);
        }
        protected void SetOnClickListener(params View[] views) => SetOnClickListener(views.AsEnumerable());

        void ClearOnClickListener()
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
    }

    partial class BaseActivity<TViewBinding, TViewModel> : View.IOnClickListener
    {
        /// <summary>
        /// 控件的点击事件
        /// </summary>
        /// <param name="view"></param>
        public virtual void OnClick(View? view)
        {
        }

        List<View>? clickViews;
        protected void SetOnClickListener(IEnumerable<View> views)
        {
            foreach (var view in views)
            {
                view.SetOnClickListener(this);
            }
            if (clickViews == null) clickViews = new();
            clickViews.AddRange(views);
        }
        protected void SetOnClickListener(params View[] views) => SetOnClickListener(views.AsEnumerable());

        void ClearOnClickListener()
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
    }
}
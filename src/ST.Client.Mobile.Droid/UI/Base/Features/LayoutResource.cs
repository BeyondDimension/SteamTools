namespace System.Application.UI.Activities
{
    partial class BaseActivity
    {
        /// <summary>
        /// 当前 <see cref="AppCompatActivity"/> 指定布局资源
        /// </summary>
        protected abstract int? LayoutResource { get; }

        void SetContentView()
        {
            var layoutResID = LayoutResource;
            if (layoutResID.HasValue) SetContentView(layoutResID.Value);
        }
    }

    partial class BaseActivity<TViewBinding, TViewModel>
    {
        /// <summary>
        /// 当前 <see cref="AppCompatActivity"/> 指定布局资源
        /// </summary>
        protected abstract int? LayoutResource { get; }

        void SetContentView()
        {
            var layoutResID = LayoutResource;
            if (layoutResID.HasValue) SetContentView(layoutResID.Value);
        }
    }
}
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using AndroidX.AppCompat.App;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 当前应用的 <see cref="AppCompatActivity"/> 基类
    /// </summary>
    public abstract partial class BaseActivity : AppCompatActivity, View.IOnClickListener
    {
        /// <summary>
        /// 当前 <see cref="AppCompatActivity"/> 指定布局资源
        /// </summary>
        protected abstract int? LayoutResource { get; }

        protected virtual void ViewBinding()
        {
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (LayoutResource.HasValue)
            {
                // 如果此处抛异常，绑定 xxx BottomNavigationView xx XML，解决方案：清理项目，重新生成
                SetContentView(LayoutResource.Value);
                //var isDarkModeQ = DarkModeUtil.IsDarkModeQ;
                //if (isDarkModeQ)
                //{
                //    // 安卓10深色模式，使用全局浅色状态栏
                //    StatusBarUtil.SetWindowLightStatusBar(this, false);
                //}
                ViewBinding();
                BindingToolbar();
                BindingBtnBack();
            }
        }

        /// <summary>
        /// 按 Back 键是否模拟 Home 键按下效果，也就是回到 Launcher，默认值 <see langword="false"/>
        /// </summary>
        protected virtual bool BackToHome { get; }

        protected bool mIsLoading;

        protected virtual View? LoadingView { get; }

        public virtual bool IsLoading
        {
            get => mIsLoading;
            set
            {
                mIsLoading = value;
                var loadingView = LoadingView;
                if (loadingView != null)
                {
                    loadingView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }
        }

        public override void OnBackPressed()
        {
            if (mIsLoading)
            {
                return;
            }
            if (BackToHome)
            {
                GoToPlatformPages.MockHomePressed(this);
                return;
            }
            base.OnBackPressed();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected virtual WebView? WebView { get; set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (WebView.Destroy_()) WebView = null;
        }

        /// <summary>
        /// 控件的点击事件
        /// </summary>
        /// <param name="view"></param>
        protected virtual void OnClick(View view)
        {
        }

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view != null) OnClick(view);
        }

        protected View GetContentView()
        {
            var content = FindViewById(Android.Resource.Id.Content);
            if (content == null) throw new NullReferenceException(nameof(content));
            var contentChild0 = ((ViewGroup)content).GetChildAt(0);
            if (contentChild0 == null) throw new NullReferenceException(nameof(contentChild0));
            return contentChild0;
        }
    }

    /// <inheritdoc cref="BaseActivity"/>
    public abstract class BaseActivity<TStartupParameters> : BaseActivity
    {
    }
}
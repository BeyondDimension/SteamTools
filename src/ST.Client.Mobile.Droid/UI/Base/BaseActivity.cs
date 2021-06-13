using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using ReactiveUI.AndroidX;
using System.Application.UI.ViewModels;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 当前应用的 <see cref="AppCompatActivity"/> 基类
    /// </summary>
    public abstract partial class BaseActivity : AppCompatActivity, View.IOnClickListener
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (!MainApplication.IsAllowStart(this)) return;
            SetContentView();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            XEPlatform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            if (BackToHome)
            {
                GoToPlatformPages.MockHomePressed(this);
                return;
            }
            base.OnBackPressed();
        }

        protected override void OnDestroy()
        {
            ClearOnClickListener();
            base.OnDestroy();
        }
    }

    /// <inheritdoc cref="BaseActivity"/>
    public abstract partial class BaseActivity<TViewBinding> : BaseActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            OnViewBinding();
        }
    }

    /// <inheritdoc cref="BaseActivity"/>
    public abstract partial class BaseActivity<TViewBinding, TViewModel> : ReactiveAppCompatActivity<TViewModel>
        where TViewModel : ViewModelBase
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (!MainApplication.IsAllowStart(this)) return;
            SetContentView();
            OnViewBinding();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            XEPlatform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            if (BackToHome)
            {
                GoToPlatformPages.MockHomePressed(this);
                return;
            }
            base.OnBackPressed();
        }

        protected override void OnDestroy()
        {
            ClearOnClickListener();
            base.OnDestroy();
        }
    }
}
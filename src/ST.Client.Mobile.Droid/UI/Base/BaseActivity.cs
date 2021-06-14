using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using ReactiveUI.AndroidX;
using System.Application.UI.ViewModels;
using XEPlatform = Xamarin.Essentials.Platform;

// ReSharper disable once CheckNamespace
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
            SetContentView(this, LayoutResource);
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
    public abstract partial class BaseActivity<TViewBinding> : BaseActivity where TViewBinding : class
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            OnCreateViewBinding();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            binding = null;
        }
    }

    /// <inheritdoc cref="BaseActivity"/>
    public abstract partial class BaseActivity<TViewBinding, TViewModel> : ReactiveAppCompatActivity<TViewModel> where TViewBinding : class
        where TViewModel : ViewModelBase
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (!MainApplication.IsAllowStart(this)) return;
            BaseActivity.SetContentView(this, LayoutResource);
            OnCreateViewBinding();
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
            binding = null;
        }
    }
}
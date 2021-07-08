using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using ReactiveUI;
using ReactiveUI.AndroidX;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Reactive.Disposables;
using XEPlatform = Xamarin.Essentials.Platform;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Activities
{
    /// <summary>
    /// 当前应用的 <see cref="AppCompatActivity"/> 基类
    /// <list type="bullet">
    ///     <item>Activity(<see href="https://developer.android.google.cn/guide/components/activities/intro-activities"/>)</item>
    ///     <item>Activity 类是 Android 应用的关键组件，而 Activity 的启动和组合方式则是该平台应用模型的基本组成部分。</item>
    ///     <item>在编程范式中，应用是通过 main() 方法启动的，而 Android 系统与此不同，</item>
    ///     <item>它会调用与其生命周期特定阶段相对应的特定回调方法来启动 Activity 实例中的代码。</item>
    /// </list>
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
            if (HandleOnBackPressed?.Invoke() ?? false)
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

        protected override void OnDestroy()
        {
            ClearOnClickListener();
            if (navController != null) navController = null;
            base.OnDestroy();
        }
    }

    /// <inheritdoc cref="BaseActivity"/>
    public abstract partial class BaseActivity<TViewBinding> : BaseActivity, IDisposableHolder
        where TViewBinding : class
    {
        readonly CompositeDisposable disposables = new();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => disposables;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            OnCreateViewBinding();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            disposables.Dispose();
            binding = null;
        }
    }

    /// <summary>
    /// 当前应用的 <see cref="ReactiveAppCompatActivity"/>(ReactiveUI) 基类
    /// <list type="bullet">
    ///     <item>ReactiveAppCompatActivity(<see href="https://github.com/reactiveui/ReactiveUI/blob/main/src/ReactiveUI.AndroidX/ReactiveAppCompatActivity.cs"/>)</item>
    ///     <item>Activity(<see href="https://developer.android.google.cn/guide/components/activities/intro-activities"/>)</item>
    ///     <item>Activity 类是 Android 应用的关键组件，而 Activity 的启动和组合方式则是该平台应用模型的基本组成部分。</item>
    ///     <item>在编程范式中，应用是通过 main() 方法启动的，而 Android 系统与此不同，</item>
    ///     <item>它会调用与其生命周期特定阶段相对应的特定回调方法来启动 Activity 实例中的代码。</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TViewBinding"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract partial class BaseActivity<TViewBinding, TViewModel> : ReactiveAppCompatActivity<TViewModel>, IReadOnlyViewFor<TViewModel>, IDisposableHolder
        where TViewBinding : class
        where TViewModel : ViewModelBase
    {
        readonly CompositeDisposable disposables = new();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => disposables;

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
            if (HandleOnBackPressed?.Invoke() ?? false)
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

        protected override void OnDestroy()
        {
            ClearOnClickListener();
            if (navController != null) navController = null;
            base.OnDestroy();
            disposables.Dispose();
            binding = null;
        }
    }
}
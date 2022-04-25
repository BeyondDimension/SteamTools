using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.Controllers;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Controllers
{
    /// <summary>
    /// MVC 模式，View 为 TViewBinding，将逻辑代码放入 Controller 中实现页面在 Activity 或 Fragment 上托管
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract class ControllerBase<TViewModel> : IDisposableHolder where TViewModel : ViewModelBase
    {
        readonly IHost host;

        public Context Context => host.Context;

        public FragmentActivity Activity => host.Activity;

        public bool IsActivity => host.HostType == ViewHostType.Activity;

        public bool IsFragment => host.HostType == ViewHostType.Fragment;

        public TViewModel ViewModel => host.ViewModel!;

        public ControllerBase(IHost host)
        {
            this.host = host;
        }

        public abstract void OnCreate();

        public virtual void OnResume() { }

        public virtual void OnDestroy() { }

        public virtual bool OnClick(View view) => false;

        public virtual TViewModel? OnCreateViewModel() => null;

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => host.CompositeDisposable;

        void IDisposable.Dispose() => host.Dispose();

        public interface IHost : IViewHost, IDisposableHolder, IReadOnlyViewFor<TViewModel>
        {
            void SetOnClickListener(params View[] views);

            void SetOnClickListener(IEnumerable<View> views);
        }

        protected virtual void SetSupportActionBarWithNavigationClick(bool displayHomeAsUpEnabled = false)
        {
            if (Activity is AppCompatActivity activity)
            {
                var toolbar = activity.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    activity.SetSupportActionBarWithNavigationClick(
                        toolbar, displayHomeAsUpEnabled);
                }
            }
        }

        protected virtual void SetOnClickListener(params View[] views) => host.SetOnClickListener(views);

        protected virtual void SetOnClickListener(IEnumerable<View> views) => host.SetOnClickListener(views);
    }

    /// <inheritdoc cref="ControllerBase{TViewModel}"/>
    public abstract class ControllerBase<TViewBinding, TViewModel> : ControllerBase<TViewModel>
        where TViewBinding : class
        where TViewModel : ViewModelBase
    {
        protected readonly TViewBinding binding;

        protected ControllerBase(IHost host, TViewBinding binding) : base(host)
        {
            this.binding = binding;
        }
    }
}

namespace System.Application.UI.Activities
{
    /// <inheritdoc cref="ControllerBase{TViewModel}"/>
    public abstract class BaseMvcActivity<TViewBinding, TViewModel, TController> : BaseActivity<TViewBinding, TViewModel>, ControllerBase<TViewModel>.IHost
        where TViewBinding : class
        where TViewModel : ViewModelBase
        where TController : ControllerBase<TViewBinding, TViewModel>
    {

        TController? mController;

        public TController Controller => mController.ThrowIsNull(nameof(mController));

        void ControllerBase<TViewModel>.IHost.SetOnClickListener(params View[] views) => SetOnClickListener(views);

        void ControllerBase<TViewModel>.IHost.SetOnClickListener(IEnumerable<View> views) => SetOnClickListener(views);

        protected override void OnCreateViewBinding()
        {
            base.OnCreateViewBinding();

            mController = (TController)Activator.CreateInstance(typeof(TController), this, binding);
        }

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            Controller.OnCreate();
        }

        protected override void OnClick(View view)
        {
            if (Controller.OnClick(view)) return;

            base.OnClick(view);
        }

        protected override TViewModel? OnCreateViewModel() => Controller.OnCreateViewModel() ?? base.OnCreateViewModel();

        protected override void OnResume()
        {
            base.OnResume();

            Controller.OnResume();
        }

        protected override void OnDestroy()
        {
            mController?.OnDestroy();
            mController = null;
            base.OnDestroy();
        }
    }
}

namespace System.Application.UI.Fragments
{
    /// <inheritdoc cref="ControllerBase{TViewModel}"/>
    public abstract class BaseMvcFragment<TViewBinding, TViewModel, TController> : BaseFragment<TViewBinding, TViewModel>, ControllerBase<TViewModel>.IHost
        where TViewBinding : class
        where TViewModel : ViewModelBase
        where TController : ControllerBase<TViewBinding, TViewModel>
    {
        TController? mController;

        public TController Controller => mController.ThrowIsNull(nameof(mController));

        void ControllerBase<TViewModel>.IHost.SetOnClickListener(params View[] views) => SetOnClickListener(views);

        void ControllerBase<TViewModel>.IHost.SetOnClickListener(IEnumerable<View> views) => SetOnClickListener(views);

        public override View? OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            mController = (TController)Activator.CreateInstance(typeof(TController), this, binding);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            Controller.OnCreate();
        }

        protected override bool OnClick(View view)
        {
            if (Controller.OnClick(view)) return true;

            return base.OnClick(view);
        }

        protected override TViewModel? OnCreateViewModel() => Controller.OnCreateViewModel() ?? base.OnCreateViewModel();

        public override void OnResume()
        {
            base.OnResume();

            Controller.OnResume();
        }

        public override void OnDestroy()
        {
            mController?.OnDestroy();
            mController = null;
            base.OnDestroy();
        }
    }
}
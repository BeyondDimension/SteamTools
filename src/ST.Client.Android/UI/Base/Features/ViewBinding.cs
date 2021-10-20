// 通过泛型指定 ViewBinding

using Android.Views;
using System.Application.UI.Activities;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Activities
{
    partial class BaseActivity<TViewBinding> : BaseActivity
    {
        protected TViewBinding? binding;

        internal static TViewBinding CreateViewBinding(object activityOrView)
        {
            var binding = (TViewBinding)Activator.CreateInstance(typeof(TViewBinding), new[] { activityOrView, null });
            return binding;
        }

        protected virtual void OnCreateViewBinding() => binding = CreateViewBinding(this);
    }

    partial class BaseActivity<TViewBinding, TViewModel>
    {
        protected TViewBinding? binding;

        protected virtual void OnCreateViewBinding() => binding = BaseActivity<TViewBinding>.CreateViewBinding(this);
    }
}

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    partial class BaseFragment<TViewBinding>
    {
        protected TViewBinding? binding;

        public override void OnCreateView(View view) => binding = BaseActivity<TViewBinding>.CreateViewBinding(view);
    }

    partial class BaseFragment<TViewBinding, TViewModel>
    {
        protected TViewBinding? binding;

        public virtual void OnCreateView(View view) => binding = BaseActivity<TViewBinding>.CreateViewBinding(view);
    }
}
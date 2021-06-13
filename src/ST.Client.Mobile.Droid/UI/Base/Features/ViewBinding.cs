namespace System.Application.UI.Activities
{
    partial class BaseActivity<TViewBinding> : BaseActivity
    {
        protected TViewBinding? binding;

        protected virtual void OnCreateViewBinding()
        {
            binding = (TViewBinding)Activator.CreateInstance(typeof(TViewBinding), new[] { this, null });
        }
    }

    partial class BaseActivity<TViewBinding, TViewModel>
    {
        protected TViewBinding? binding;

        protected virtual void OnCreateViewBinding()
        {
            binding = (TViewBinding)Activator.CreateInstance(typeof(TViewBinding), new[] { this, null });
        }
    }
}
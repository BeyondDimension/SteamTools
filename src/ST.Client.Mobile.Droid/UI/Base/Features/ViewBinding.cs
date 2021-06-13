namespace System.Application.UI.Activities
{
    partial class BaseActivity<TViewBinding> : BaseActivity
    {
        protected TViewBinding? binding;

        protected virtual void OnViewBinding()
        {
            binding = (TViewBinding)Activator.CreateInstance(typeof(TViewBinding), this);
        }
    }

    partial class BaseActivity<TViewBinding, TViewModel>
    {
        protected TViewBinding? binding;

        protected virtual void OnViewBinding()
        {
            binding = (TViewBinding)Activator.CreateInstance(typeof(TViewBinding), this);
        }
    }
}
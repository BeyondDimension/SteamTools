using Android.Views;
using AndroidX.Navigation;
using System.Application.UI.Activities.Internals;
using Fragment = AndroidX.Fragment.App.Fragment;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Activities
{
    partial class BaseActivity : IHostNavController
    {
        protected NavController? navController;

        NavController? IHostNavController.NavController => navController;
    }

    partial class BaseActivity<TViewBinding, TViewModel> : IHostNavController
    {
        protected NavController? navController;

        NavController? IHostNavController.NavController => navController;
    }

    public static partial class BaseActivityExtensions
    {
        public static NavController? GetNavController(this Fragment fragment, View? view = null)
        {
            NavController? navController = null;
            if (fragment.Activity is IHostNavController host)
            {
                navController = host.NavController;
            }
            if (navController == null && view != null)
            {
                navController = Navigation.FindNavController(view);
            }
            return navController;
        }
    }
}

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Activities.Internals
{
    public interface IHostNavController
    {
        NavController? NavController { get; }
    }
}
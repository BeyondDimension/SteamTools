using Android.Views;
using AndroidX.Navigation;
using System.Application.UI.Activities.Internals;
using CharSequence = Java.Lang.ICharSequence;
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
}

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Activities.Internals
{
    public interface IHostNavController
    {
        NavController? NavController { get; }
    }
}

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class BaseUIExtensions
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

        static NavDestination? FindNode(this IHostNavController host, int resId)
        {
            if (host.NavController == null) return null;
            var navNode = host.NavController.Graph.FindNode(resId);
            return navNode;
        }

        public static void SetNavigationGraphTitle(this IHostNavController host, int resId, CharSequence value)
        {
            var navNode = host.FindNode(resId);
            if (navNode == null) return;
            navNode.LabelFormatted = value;
        }

        public static void SetNavigationGraphTitle(this IHostNavController host, int resId, string value)
        {
            var navNode = host.FindNode(resId);
            if (navNode == null) return;
            navNode.Label = value;
        }
    }
}
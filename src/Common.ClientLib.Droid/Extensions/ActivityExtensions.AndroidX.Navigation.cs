using Android.App;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using AndroidX.Navigation.Fragment;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class ActivityExtensions
    {
        #region 导航相关 AndroidX.Navigation

        /// <summary>
        /// 在 Activity 中 即时获取由 AndroidX.Navigation 管理的当前 Fragment
        /// </summary>
        /// <typeparam name="TFragment"></typeparam>
        /// <param name="activity"></param>
        /// <returns></returns>
        [return: MaybeNull]
        public static TFragment GetPrimaryNavigationFragment<TFragment>(
            this FragmentActivity activity)
            where TFragment : Fragment
        {
            var fragments = activity.SupportFragmentManager.Fragments;
            if (fragments != null)
            {
                foreach (var fragment in fragments)
                {
                    if (fragment is NavHostFragment navHostFragment)
                    {
                        var primaryNavigationFragment = navHostFragment
                            .ChildFragmentManager.PrimaryNavigationFragment;
                        if (primaryNavigationFragment is TFragment t)
                        {
                            return t;
                        }
                    }
                }
            }
            return null;
        }

        [return: MaybeNull]
        public static TFragment GetNavigationFragment<TFragment>(
           this FragmentActivity activity)
           where TFragment : Fragment
        {
            var fragments = activity.SupportFragmentManager.Fragments;
            if (fragments != null)
            {
                foreach (var fragment in fragments)
                {
                    if (fragment is NavHostFragment navHostFragment)
                    {
                        var fragments2 = navHostFragment
                            .ChildFragmentManager.Fragments;
                        foreach (var fragment1 in fragments2)
                        {
                            if (fragment1 is TFragment t)
                            {
                                return t;
                            }
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
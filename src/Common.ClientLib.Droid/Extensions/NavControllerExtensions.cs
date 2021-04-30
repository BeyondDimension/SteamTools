using AndroidX.Annotations;
using AndroidX.Navigation;
using R = System.Common.R;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// <see cref="NavController"/> 扩展
    /// </summary>
    public static class NavControllerExtensions
    {
        /// <summary>
        /// 导航跳转到 res\navigation\xx_navigation.xml 中指定的 Id 对应的 Fragment
        /// </summary>
        /// <param name="navController"></param>
        /// <param name="resId"></param>
        /// <param name="destinationId"></param>
        /// <param name="inclusive"></param>
        /// <param name="isReplace">跳转的行为是否为替换，如果是则不能通过后退返回之前的页面</param>
        public static void Navigate(this NavController navController,
            [IdRes] int resId,
            [IdRes] int? destinationId,
            bool inclusive)
        {
            var builder = new NavOptions.Builder()
                .SetLaunchSingleTop(true)
                .SetEnterAnim(R.anim.slide_in_right)
                .SetExitAnim(R.anim.slide_out_left)
                .SetPopEnterAnim(R.anim.slide_in_left)
                .SetPopExitAnim(R.anim.slide_out_right);
            if (destinationId.HasValue)
            {
                builder.SetPopUpTo(destinationId.Value, inclusive);
            }
            navController.Navigate(resId, null, builder.Build());
        }

        /// <inheritdoc cref="Navigate(NavController, int, int?, bool)"/>
        public static void Navigate(this NavController navController,
            [IdRes] int resId,
            bool isReplace = false)
        {
            if (isReplace)
            {
                int destinationId = navController.CurrentDestination.Id;
                Navigate(navController, resId, destinationId, true);
            }
            else
            {
                Navigate(navController, resId, null, default);
            }
        }
    }
}
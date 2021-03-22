using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// 对 <see cref="View"/> 类的扩展函数集
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Set the background to a given Drawable, or remove the background. If the background has padding, this View's padding is set to the background's padding. However, when a background is removed, this View's padding isn't touched. If setting the padding is desired, please use setPadding(int, int, int, int).
        /// <para>https://developer.android.google.cn/reference/android/view/View#setBackground(android.graphics.drawable.Drawable)</para>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="background">The Drawable to use as the background, or null to remove the background</param>
        public static void SetBackground(this View view, Drawable? background)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                view.Background = background;
            }
            else
            {
#pragma warning disable CS0618 // 类型或成员已过时
                view.SetBackgroundDrawable(background);
#pragma warning restore CS0618 // 类型或成员已过时
            }
        }
    }
}
using Android.Graphics.Drawables;
using Android.Widget;
using AndroidX.Core.Widget;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// <see cref="CompoundButton"/> 扩展
    /// </summary>
    public static partial class CompoundButtonExtensions
    {
        public static Drawable? GetButtonDrawableCompat(this CompoundButton button) => CompoundButtonCompat.GetButtonDrawable(button);
    }
}

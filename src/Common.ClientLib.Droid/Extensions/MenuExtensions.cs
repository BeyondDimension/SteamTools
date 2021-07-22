using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.View.Menu;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class MenuExtensions
    {
        public static MenuBuilder? SetOptionalIconsVisible(this IMenu? menu)
        {
            if (menu is MenuBuilder menuBuilder)
            {
                menuBuilder.SetOptionalIconsVisible(true);
                var iconMarginLeftPx = menuBuilder.Context.DpToPxInt32(7);
                foreach (var item in menuBuilder.VisibleItems)
                {
                    if (item.Icon != null)
                    {
                        item.SetIcon(new InsetDrawable(item.Icon, iconMarginLeftPx, 0, 0, 0));
                    }
                }
                return menuBuilder;
            }
            return null;
        }
    }
}
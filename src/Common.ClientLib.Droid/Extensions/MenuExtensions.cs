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

        public static void SetMenuTitle<TEnum>(this MenuBuilder? menuBuilder, Func<TEnum, string> getString, Func<int, TEnum> menuIdResToEnum) where TEnum : Enum
        {
            if (menuBuilder == null) return;
            for (int i = 0; i < menuBuilder.Size(); i++)
            {
                var item = menuBuilder.GetItem(i);
                var actionItem = menuIdResToEnum(item.ItemId);
                if (actionItem.IsDefined())
                {
                    item.SetTitle(getString(actionItem));
                }
            }
        }
    }
}
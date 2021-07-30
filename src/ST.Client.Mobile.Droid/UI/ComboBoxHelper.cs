using Android.Content;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Dialog;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ListPopupWindow = AndroidX.AppCompat.Widget.ListPopupWindow;

namespace System.Application.UI
{
    /// <summary>
    /// 实现类似 WPF/UWP ComboBox | HTML Select 的列表框助手类
    /// <para>https://docs.microsoft.com/zh-cn/windows/apps/design/controls/combo-box</para>
    /// </summary>
    public static class ComboBoxHelper
    {
        /// <summary>
        /// 使用弹窗(<see cref="MaterialAlertDialogBuilder"/>)的实现
        /// </summary>
        /// <param name="context"></param>
        /// <param name="items"></param>
        /// <param name="click"></param>
        public static void Dialog(Context context, string[] items, Action<string> click)
        {
            new MaterialAlertDialogBuilder(context).SetItems(items, (_, e) =>
            {
                if (e.Which >= 0 && e.Which < items.Length)
                {
                    var item = items[e.Which];
                    click(item);
                }
            }).Show();
        }

        /// <summary>
        /// 使用弹出菜单(<see cref="ListPopupWindow"/>)的实现
        /// </summary>
        /// <param name="context"></param>
        /// <param name="items"></param>
        /// <param name="click"></param>
        /// <param name="anchorView">弹出的描点</param>
        public static ListPopupWindowWrapper Popup(Context context, string[] items, Action<string> click, View anchorView)
        {
            // https://material.io/components/menus/android#dropdown-menus

            ListPopupWindow listPopupWindow = new(context, null, Resource.Attribute.listPopupWindowStyle);

            // Set button as the list popup's anchor
            listPopupWindow.AnchorView = anchorView;

            var items_ = items.ToJavaObjectList();
            var adapter = new ArrayAdapter(context, Resource.Layout.list_popup_window_item, items_);
            listPopupWindow.SetAdapter(adapter);

            ListPopupWindowWrapper wrapper = new(items, listPopupWindow, adapter, click);
            listPopupWindow.SetOnItemClickListener(wrapper);

            return wrapper;
        }


        public sealed class ListPopupWindowWrapper : Java.Lang.Object, AdapterView.IOnItemClickListener
        {
            public ListPopupWindow Window { get; }

            public ArrayAdapter Adapter { get; }

            string[] items;
            public string[] Items
            {
                get => items;
                set
                {
                    if (items.SequenceEqual(value)) return;
                    Adapter.Clear();
                    Adapter.AddAll((ICollection)value);
                    Adapter.NotifyDataSetChanged();
                    items = value;
                }
            }

            public Action<string> ItemClick { get; }

            public ListPopupWindowWrapper(string[] items, ListPopupWindow listPopupWindow, ArrayAdapter adapter, Action<string> click)
            {
                this.items = items;
                Window = listPopupWindow;
                Adapter = adapter;
                ItemClick = click;
            }

            void AdapterView.IOnItemClickListener.OnItemClick(AdapterView? parent, View? view, int position, long id)
            {
                if (position >= 0 && position < Items.Length)
                {
                    var item = Items[position];
                    ItemClick(item);
                }
                Window.Dismiss();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Window.Dispose();
                    Adapter.Dispose();
                }
                base.Dispose(disposing);
            }

            public void Show() => Window.Show();
        }
    }
}
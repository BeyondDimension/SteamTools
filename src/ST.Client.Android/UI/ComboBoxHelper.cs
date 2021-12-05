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
        public static ListPopupWindowWrapper<T> Popup<T>(Context context, IEnumerable<T> items, Action<T> click, View anchorView)
        {
            // https://material.io/components/menus/android#dropdown-menus

            ListPopupWindow listPopupWindow = new(context, null, Resource.Attribute.listPopupWindowStyle);

            // Set button as the list popup's anchor
            listPopupWindow.AnchorView = anchorView;

            var adapter = CreateArrayAdapter(context, Resource.Layout.list_popup_window_item, items);
            listPopupWindow.SetAdapter(adapter);

            ListPopupWindowWrapper<T> wrapper = new(items, listPopupWindow, adapter, click);
            listPopupWindow.SetOnItemClickListener(wrapper);

            return wrapper;
        }


        public sealed class ListPopupWindowWrapper<T> : Java.Lang.Object, AdapterView.IOnItemClickListener
        {
            public ListPopupWindow Window { get; }

            public ArrayAdapter Adapter { get; }

            IEnumerable<T> items;
            public IEnumerable<T> Items
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

            public Action<T> ItemClick { get; }

            public ListPopupWindowWrapper(IEnumerable<T> items, ListPopupWindow listPopupWindow, ArrayAdapter adapter, Action<T> click)
            {
                this.items = items;
                Window = listPopupWindow;
                Adapter = adapter;
                ItemClick = click;
            }

            void AdapterView.IOnItemClickListener.OnItemClick(AdapterView? parent, View? view, int position, long id)
            {
                if (position >= 0 && position < items.Count())
                {
                    var item = items.ElementAt(position);
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

        public static ArrayAdapter<T> CreateArrayAdapter<T>(Context context, int resource, IEnumerable<T>? items = null)
        {
            var adapter = items == null ?
                new ArrayAdapter<T>(context, resource, 0) :
                new ArrayAdapter<T>(context, resource, 0, items.ToJavaList());
            return adapter;
        }

        /// <summary>
        /// 使用 MaterialAutoCompleteTextView 的实现
        /// https://material.io/components/menus/android#exposed-dropdown-menus
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ArrayAdapter<T> CreateArrayAdapter<T>(AutoCompleteTextView textView)
        {
            var adapter = CreateArrayAdapter<T>(textView.Context!, Resource.Layout.list_item);
            textView.Adapter = adapter;
            return adapter;
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using System.IO;

namespace System.Application.UI
{
    public interface INotifyIcon : INotifyIcon<ContextMenu>
    {
        public static INotifyIcon Instance => DI.Get<INotifyIcon>();

        internal sealed class UIFrameworkHelper : IUIFrameworkHelper
        {
            public Stream OpenAsset(Uri uri)
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                return assets.Open(uri);
            }

            sealed class ActivatedWrapper
            {
                readonly MenuItem menuItem;

                public ActivatedWrapper(MenuItem menuItem)
                {
                    this.menuItem = menuItem;
                }

                public void Activated() => menuItem.Command?.Execute(null);
            }

            public void ForEachMenuItems(ContextMenu menu, Action<(object menuItem, string header, Action activated)> action)
            {
                foreach (MenuItem menuItem in menu.Items)
                {
                    var header = menuItem.Header?.ToString() ?? string.Empty;
                    var activated = new ActivatedWrapper(menuItem);
                    action((menuItem, header, activated.Activated));
                }
            }
        }
    }
}
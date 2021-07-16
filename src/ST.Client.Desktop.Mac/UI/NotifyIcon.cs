#if MONO_MAC
using MonoMac.AppKit;
#elif XAMARIN_MAC
using AppKit;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;

namespace System.Application.UI
{
    /// <summary>
    /// Represents a Notification Tray Icon (on a OSxMac StatusBarItem)
    /// </summary>
    public class NotifyIcon<TContextMenu> : INotifyIcon<TContextMenu>
    {
        NSStatusItem? _item;

        [NotNull, DisallowNull] // C# 8 not null
        NSStatusItem? StatusBarItem
        {
            get => _item ?? throw new ArgumentNullException(nameof(_item));
            set
            {
                _item = value;
                UpdateMenu();
            }
        }

        public event EventHandler<EventArgs>? Click;
        public event EventHandler<EventArgs>? DoubleClick;
        public event EventHandler<EventArgs>? RightClick;

        /// <summary>
        /// Updates the Tray Menu Item settings, ToolTip, ContextMenu, Image etc on MacOSX
        /// </summary>
        void UpdateMenu()
        {
            if (_item != null)
            {
                var f = DI.Get<INotifyIcon<TContextMenu>.IUIFrameworkHelper>();

                _item.Image = NSImage.FromStream(f.OpenAsset(new Uri(IconPath)));
                _item.ToolTip = ToolTipText;
                if (StatusBarItem.Menu == null)
                {
                    StatusBarItem.Menu = new NSMenu();
                }
                else
                {
                    StatusBarItem.Menu.RemoveAllItems();
                }

                if (_menu == null) throw new ArgumentNullException(nameof(_menu));

                f.ForEachMenuItems(_menu, item =>
                {
                    NSMenuItem menuItem = new NSMenuItem(item.header);
                    menuItem.Activated += (_, _) => { item.activated(); };
                    StatusBarItem.Menu.AddItem(menuItem);
                });

                StatusBarItem.DoubleClick += (s, e) => { DoubleClick?.Invoke(this, new EventArgs()); };
            }
        }

        /// <summary>
        /// Gets or sets the icon for the notify icon. Either a file system path
        /// or a <c>resm:</c> manifest resource path can be specified.
        /// </summary>
        string _iconPath = "";

        public string IconPath { get => _iconPath; set { _iconPath = value; UpdateMenu(); } }

        string _toolTip = "";

        /// <summary>
        /// Gets or sets the tooltip text for the notify icon.
        /// </summary>
        public string ToolTipText { get => _toolTip; set { _toolTip = value; UpdateMenu(); } }

        TContextMenu? _menu;

        /// <summary>
        /// Gets or sets the context menu for the notify icon.
        /// </summary>
        public TContextMenu? ContextMenu
        {
            get => _menu;
            set
            {
                _menu = value;
                UpdateMenu();
            }
        }

        /// <summary>
        /// Gets or sets if the notify icon is visible in the
        /// taskbar notification area or not.
        /// </summary>
        public bool Visible { get; set; }

        public void Remove()
        {
            StatusBarItem?.Dispose();
        }

        void Init()
        {
            var systemStatusBar = NSStatusBar.SystemStatusBar;
            StatusBarItem = systemStatusBar.CreateStatusItem(30);
            StatusBarItem.ToolTip = ToolTipText;
        }

        /// <summary>
        /// Creates a new <c>NotifyIcon</c> instance and sets up some
        /// required resources.
        /// </summary>

        public NotifyIcon()
        {
            MainThread2.BeginInvokeOnMainThread(Init,
                DispatcherPriorityCompat.MaxValue);
        }
    }
}
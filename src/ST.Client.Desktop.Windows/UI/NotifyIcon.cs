using System.Application.Models;
using System.Application.UI.Views;
using System.Collections.Generic;
using System.Drawing;

namespace System.Application.UI
{
    /// <summary>
    /// Represents a taskbar notification area icon (aka "tray icon") on Windows.
    /// </summary>
    public class NotifyIcon<TContextMenu> : INotifyIcon<TContextMenu>
    {
        const string TAG = "NotifyIcon";
        readonly INotifyIconWindow? _window = null;
        readonly int _uID = 0;
        static int _nextUID = 0;
        bool _iconAdded = false;
        string _iconPath = string.Empty;
        Icon? _icon = null;
        string _toolTipText = string.Empty;
        bool _visible = false;
        bool _doubleClick = false;

        public event EventHandler<EventArgs>? Click;
        public event EventHandler<EventArgs>? DoubleClick;
        public event EventHandler<EventArgs>? RightClick;

        /// <summary>
        /// Gets or sets the icon for the notify icon. Either a file system path
        /// or a <c>resm:</c> manifest resource path can be specified.
        /// </summary>
        public string IconPath
        {
            get => _iconPath;
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        _icon = null;
                        _iconPath = string.Empty;
                        return;
                    }

                    // Check if path is a file system or resource path
                    if (value.StartsWith("resm:") || value.StartsWith("avares://"))
                    {
                        var f = DI.Get<INotifyIcon<TContextMenu>.IUIFrameworkHelper>();

                        // Resource path
                        _icon = new Icon(f.OpenAsset(new Uri(value)));
                    }
                    else
                    {
                        // File system path
                        _icon = new Icon(value);
                    }
                    _iconPath = value;
                }
                catch (Exception e)
                {
                    Log.Error(TAG, e, "Set IconPath Fail.");
                    _icon = null;
                    _iconPath = string.Empty;
                }
                finally
                {
                    UpdateIcon();
                }
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text for the notify icon.
        /// </summary>
        public string ToolTipText
        {
            get => _toolTipText;
            set
            {
                if (_toolTipText != value)
                {
                    _toolTipText = value;
                }
                UpdateIcon();
            }
        }

        /// <summary>
        /// Gets or sets the context menu for the notify icon.
        /// </summary>
        public TContextMenu? ContextMenu { get; set; }

        /// <summary>
        /// Gets or sets if the notify icon is visible in the
        /// taskbar notification area or not.
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                }
                UpdateIcon();
            }
        }

        /// <summary>
        /// Creates a new <c>NotifyIcon</c> instance and sets up some
        /// required resources.
        /// </summary>
        public NotifyIcon()
        {
            _uID = ++_nextUID;

            var notifyIconWindow = DI.Get_Nullable<INotifyIconWindow<TContextMenu>>();
            if (notifyIconWindow != null)
            {
                notifyIconWindow.Initialize(this);
                _window = notifyIconWindow;
            }
            else
            {
                _window = new NotifyIconWindow(this);
            }
        }

        ~NotifyIcon()
        {
            UpdateIcon(remove: true);
        }

        /// <summary>
        /// Shows, hides or removes the notify icon based on the set properties and parameters.
        /// </summary>
        /// <param name="remove">If set to true, the notify icon will be removed.</param>
        void UpdateIcon(bool remove = false)
        {
            if (_window == null) throw new ArgumentNullException(nameof(_window));
            UnmanagedMethods.NOTIFYICONDATA iconData = new UnmanagedMethods.NOTIFYICONDATA()
            {
                hWnd = _window.Handle,
                uID = _uID,
                uFlags = UnmanagedMethods.NIF.TIP | UnmanagedMethods.NIF.MESSAGE,
                uCallbackMessage = (int)UnmanagedMethods.CustomWindowsMessage.WM_TRAYMOUSE,
                hIcon = IntPtr.Zero,
                szTip = ToolTipText
            };

            if (!remove && _icon != null && Visible)
            {
                iconData.uFlags |= UnmanagedMethods.NIF.ICON;
                iconData.hIcon = _icon.Handle;

                if (!_iconAdded)
                {
                    _ = UnmanagedMethods.Shell_NotifyIcon(UnmanagedMethods.NIM.ADD, iconData);
                    _iconAdded = true;
                }
                else
                {
                    _ = UnmanagedMethods.Shell_NotifyIcon(UnmanagedMethods.NIM.MODIFY, iconData);
                }
            }
            else
            {
                _ = UnmanagedMethods.Shell_NotifyIcon(UnmanagedMethods.NIM.DELETE, iconData);
                _iconAdded = false;
            }
        }

        /// <summary>
        /// Removes the notify icon from the taskbar notification area.
        /// </summary>
        public void Remove()
        {
            UpdateIcon(remove: true);
        }

        /// <summary>
        /// If available, displays the notification icon's context menu.
        /// </summary>
        void ShowContextMenu()
        {
            if (ContextMenu != null)
            {
                // Since we can't use the Avalonia ContextMenu directly due to shortcomings
                // regrading its positioning, we'll create a native context menu instead.
                // This dictionary will map the menu item IDs which we'll need for the native
                // menu to the MenuItems of the provided Avalonia ContextMenu.
                Dictionary<uint, Action> contextItemLookup = new Dictionary<uint, Action>();

                // Create a native (Win32) popup menu as the notify icon's context menu.
                IntPtr popupMenu = UnmanagedMethods.CreatePopupMenu();

                uint i = 1;

                var f = DI.Get<INotifyIcon<TContextMenu>.IUIFrameworkHelper>();

                f.ForEachMenuItems(ContextMenu, item =>
                {
                    // Add items to the native context menu by simply reusing
                    // the information provided within the Avalonia ContextMenu.
                    _ = UnmanagedMethods.AppendMenu(popupMenu, UnmanagedMethods.MenuFlags.MF_STRING, i, item.header);

                    // Add the mapping so that we can find the selected item later
                    contextItemLookup.Add(i, item.activated);
                    i++;
                });

                if (_window == null) throw new ArgumentNullException(nameof(_window));

                // To display a context menu for a notification icon, the current window
                // must be the foreground window before the application calls TrackPopupMenu
                // or TrackPopupMenuEx.Otherwise, the menu will not disappear when the user
                // clicks outside of the menu or the window that created the menu (if it is
                // visible). If the current window is a child window, you must set the
                // (top-level) parent window as the foreground window.
                _ = UnmanagedMethods.SetForegroundWindow(_window.Handle);

                // Get the mouse cursor position
                _ = UnmanagedMethods.GetCursorPos(out UnmanagedMethods.POINT pt);

                // Now display the context menu and block until we get a result
                uint commandId = UnmanagedMethods.TrackPopupMenuEx(
                    popupMenu,
                    UnmanagedMethods.UFLAGS.TPM_BOTTOMALIGN |
                    UnmanagedMethods.UFLAGS.TPM_RIGHTALIGN |
                    UnmanagedMethods.UFLAGS.TPM_NONOTIFY |
                    UnmanagedMethods.UFLAGS.TPM_RETURNCMD,
                    pt.X, pt.Y, _window.Handle, IntPtr.Zero);

                // If we have a result, execute the corresponding command
                if (commandId != 0)
                {
                    contextItemLookup[commandId]();
                }
            }
        }

        /// <summary>
        /// Handles the NotifyIcon-specific window messages sent by the notification icon.
        /// </summary>
        public void WndProc(uint msg, IntPtr wParam, IntPtr lParam)
        {
            //Log.Debug(TAG, "WndProc: MSG={0}, wParam={1}, lParam={2}",
            //    ((UnmanagedMethods.CustomWindowsMessage)msg).ToString(),
            //    ((UnmanagedMethods.WindowsMessage)wParam.ToInt32()).ToString(),
            //    ((UnmanagedMethods.WindowsMessage)lParam.ToInt32()).ToString());

            // We only care about tray icon messages
            if (msg != (uint)UnmanagedMethods.CustomWindowsMessage.WM_TRAYMOUSE)
                return;

            // Determine the type of message and call the matching event handlers
            switch (lParam.ToInt32())
            {
                case (int)UnmanagedMethods.WindowsMessage.WM_LBUTTONUP:
                    if (!_doubleClick)
                    {
                        Click?.Invoke(this, new EventArgs());
                    }
                    _doubleClick = false;
                    break;

                case (int)UnmanagedMethods.WindowsMessage.WM_LBUTTONDBLCLK:
                    DoubleClick?.Invoke(this, new EventArgs());
                    _doubleClick = true;
                    break;

                case (int)UnmanagedMethods.WindowsMessage.WM_RBUTTONUP:
                    MouseHook.GetCursorPos(out var pointInt);
                    RightClick?.Invoke(this, new MouseEventArgs(pointInt));
                    ShowContextMenu();
                    break;

                default:
                    break;
            }
        }

        public static IntPtr? WndProc(NotifyIcon<TContextMenu> notifyIcon, uint msg, IntPtr wParam, IntPtr lParam)
        {
            //Log.Info(TAG, "WndProc called on NotifyIcon helper window: MSG = {0}",
            //     ((UnmanagedMethods.WindowsMessage)msg).ToString());

            switch (msg)
            {
                case (uint)UnmanagedMethods.CustomWindowsMessage.WM_TRAYMOUSE:
                    // Forward WM_TRAYMOUSE messages to the tray icon's window procedure
                    notifyIcon.WndProc(msg, wParam, lParam);
                    break;
            }

            return null;
        }

        /// <summary>
        /// A native Win32 helper window encapsulation for dealing with the window
        /// messages sent by the notification icon.
        /// </summary>
        sealed class NotifyIconWindow : NativeWindow, INotifyIconWindow
        {
            readonly NotifyIcon<TContextMenu> _notifyIcon;

            public NotifyIconWindow(NotifyIcon<TContextMenu> notifyIcon) : base()
            {
                _notifyIcon = notifyIcon;
            }

            /// <summary>
            /// This function will receive all the system window messages relevant to our window.
            /// </summary>
            protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                var value = NotifyIcon<TContextMenu>.WndProc(_notifyIcon, msg, wParam, lParam);
                return value ?? base.WndProc(hWnd, msg, wParam, lParam);
            }
        }
    }
}
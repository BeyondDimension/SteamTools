// https://github.com/AvaloniaUI/Avalonia/issues/2649#issuecomment-620720914

using System.IO;

namespace System.Application.UI
{
    /// <summary>
    /// Represents a taskbar notification area icon (aka "tray icon") on Windows,
    /// and similar task panel notification icons on Linux and Mac.
    /// <para><see cref="https://github.com/sqrldev/SQRLDotNetClient/blob/master/SQRLDotNetClientUI/Models/INotifyIcon.cs"/></para>
    /// </summary>
    public interface INotifyIcon<TContextMenu>
    {
        /// <summary>
        /// Gets or sets the icon for the notify icon. Either a file system path
        /// or a <c>resm:</c> manifest resource path can be specified.
        /// </summary>
        string IconPath { get; set; }

        /// <summary>
        /// Gets or sets the tooltip text for the notify icon.
        /// </summary>
        string ToolTipText { get; set; }

        /// <summary>
        /// Gets or sets the context- (right-click)-menu for the notify icon.
        /// </summary>
        TContextMenu? ContextMenu { get; set; }

        /// <summary>
        /// Gets or sets if the notify icon is visible in the
        /// taskbar notification area or not.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Removes the notify icon from the taskbar notification area.
        /// </summary>
        void Remove();

        /// <summary>
        /// This event is raised when a user clicks on the notification icon.
        /// </summary>
        event EventHandler<EventArgs> Click;

        /// <summary>
        /// This event is raised when a user doubleclicks on the notification icon.
        /// </summary>
        event EventHandler<EventArgs> DoubleClick;

        /// <summary>
        /// This event is raised when a user right-clicks on the notification icon.
        /// </summary>
        event EventHandler<EventArgs> RightClick;

        public interface IUIFrameworkHelper
        {
            Stream OpenAsset(Uri uri);

            void ForEachMenuItems(TContextMenu menu, Action<(object menuItem, string header, Action activated)> action);
        }
    }
}
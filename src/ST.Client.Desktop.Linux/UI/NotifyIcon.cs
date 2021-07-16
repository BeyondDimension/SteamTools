using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using EtoFormsApplication = Eto.Forms.Application;
using EtoPlatform = Eto.Platform;

namespace System.Application.UI
{
    public class NotifyIcon<TContextMenu> : INotifyIcon<TContextMenu>
    {
        public event EventHandler<EventArgs>? Click;
        public event EventHandler<EventArgs>? DoubleClick;
        public event EventHandler<EventArgs>? RightClick;

        LinuxTrayIcon<TContextMenu>? lti;

        internal LinuxTrayIcon<TContextMenu>? LtiProp
        {
            get => lti;
            set => lti = value;
        }

        public Task? TrayIconTask { get; private set; }

        CancellationTokenSource? canTok;

        /// <summary>
        /// Gets or sets the icon for the notify icon. Either a file system path
        /// or a <c>resm:</c> manifest resource path can be specified.
        /// </summary>
        string _iconPath = "";

        public string IconPath
        {
            get => _iconPath;
            set
            {
                _iconPath = value;
                UpdateMenu();
            }
        }

        string _toolTip = "";

        /// <summary>
        /// Gets or sets the tooltip text for the notify icon.
        /// </summary>
        public string ToolTipText
        {
            get => _toolTip;
            set
            {
                _toolTip = value;
                UpdateMenu();
            }
        }

        TContextMenu? _menu;

        /// <summary>
        /// Gets or sets the context menu for the notify icon.
        /// </summary>
        public TContextMenu? ContextMenu
        {
            get => _menu; set
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
            MainThread2.BeginInvokeOnMainThread(() =>
            {
                lti?._tray?.Hide();
                canTok?.Cancel();
            });
        }

        void UpdateMenu()
        {
            if (!string.IsNullOrEmpty(IconPath) && !string.IsNullOrEmpty(ToolTipText) && ContextMenu != null)
            {
                canTok = new CancellationTokenSource();
                MainThread2.BeginInvokeOnMainThread(() =>
                {
                    // Because of the way that Linux works this needs to run on its own Thread.
                    TrayIconTask = Task.Factory.StartNew(() =>
                    {
                        new EtoFormsApplication(EtoPlatform.Detect)
                        .Run(LtiProp =
                            new LinuxTrayIcon<TContextMenu>(ToolTipText, IconPath, ContextMenu));
                    }
               , canTok.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                }, DispatcherPriorityCompat.MaxValue);
            }
        }
    }
}
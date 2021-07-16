using Eto.Drawing;
using Eto.Forms;

namespace System.Application.UI
{
    /// <summary>
    /// This Implements a Linux GTK3 Tray Icon
    /// </summary>
    internal sealed class LinuxTrayIcon<TContextMenu> : Form
    {
        public TrayIndicator? _tray;
        readonly bool _startup = true;

        /// <summary>
        /// Everything has to be sent in on the constructor since things do not auto-refresh / update this is a limitation
        /// </summary>
        /// <param name="tooTip"></param>
        /// <param name="iconPath"></param>
        /// <param name="menu"></param>
        public LinuxTrayIcon(string tooTip, string iconPath, TContextMenu menu)
        {
            MainThread2.BeginInvokeOnMainThread(() =>
            {
                var ctxMnu = new ContextMenu();

                var f = DI.Get<INotifyIcon<TContextMenu>.IUIFrameworkHelper>();

                f.ForEachMenuItems(menu, item =>
                {
                    ButtonMenuItem bmi = new ButtonMenuItem
                    {
                        Text = item.header,
                        Command = new Command((s, e) =>
                        {
                            MainThread2.BeginInvokeOnMainThread(() =>
                            {
                                item.activated();
                            });
                        })
                    };
                    ctxMnu.Items.Add(bmi);
                });

                ClientSize = new Size(200, 200);

                var image = new Icon(f.OpenAsset(new Uri(iconPath)));

                _tray = new TrayIndicator
                {
                    //Image = Icon.FromResource(iconPath.Replace("resm:", "")),
                    Image = image,
                    Menu = ctxMnu,
                    Title = tooTip
                };

                _tray.Show();
                _tray.Visible = true;
            });
        }

        protected override void OnShown(EventArgs e)
        {
            if (_startup)
            {
                Visible = false;
            }
        }

        protected override void OnUnLoad(EventArgs e)
        {
            base.OnUnLoad(e);
            _tray?.Hide();
        }
    }
}
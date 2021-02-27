//using AppKit;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace System.Application.UI
//{
//    /// <summary>
//    /// Represents a Notification Tray Icon (on a OSxMac StatusBarItem) 
//    /// </summary>
//    public class NotifyIcon<TContextMenu> : INotifyIcon<TContextMenu>
//    {
//        NSStatusItem _item;
//        NSStatusItem statusBarItem
//        {
//            get => _item; set
//            {
//                _item = value;
//                UpdateMenu();
//            }
//        }

//        public event EventHandler<EventArgs>? Click;
//        public event EventHandler<EventArgs>? DoubleClick;
//        public event EventHandler<EventArgs>? RightClick;
//    }
//}

using Livet;
using MetroTrilithon.Lifetime;
using SteamTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SteamTools.Services
{
    public class WindowService : NotificationObject, IDisposableHolder
    {
        public static WindowService Current { get; } = new WindowService();

        private readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();

        /// <summary>
        /// 获取为当前主窗口提供的数据。
        /// </summary>
        public MainWindowViewModelBase MainWindow { get; private set; }


        public void Initialize()
        {
            this.MainWindow = new MainWindowViewModel();

            //KanColleClient.Current.Subscribe(nameof(KanColleClient.IsStarted), this.UpdateMode).AddTo(this);
        }

        //public void ClearZoomFactor()
        //{
        //	this.kanColleWindow?.Messenger.Raise(new InteractionMessage { MessageKey = "WebBrowser.Zoom" });
        //}

        //public void SetLocationLeft()
        //{
        //	this.kanColleWindow?.Messenger.Raise(new SetWindowLocationMessage { MessageKey = "Window.Location", Left = 0.0 });
        //}


        public Window GetMainWindow()
        {
            return new MainWindow { DataContext = this.MainWindow, };
        }


        //private void UpdateMode()
        //{
        //	this.Mode = KanColleClient.Current.IsStarted
        //		? KanColleClient.Current.IsInSortie ? WindowServiceMode.InSortie : WindowServiceMode.Started
        //		: WindowServiceMode.NotStarted;
        //}

        #region disposable members

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.compositeDisposable.Dispose();
        }

        #endregion
    }
}

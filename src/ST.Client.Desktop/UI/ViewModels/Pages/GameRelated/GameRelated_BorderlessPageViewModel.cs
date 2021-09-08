using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Collections.ObjectModel;

namespace System.Application.UI.ViewModels
{
    public class GameRelated_BorderlessPageViewModel : ViewModelBase
    {
        private readonly ISystemWindowApiService windowApi = DI.Get<ISystemWindowApiService>();
        #region SelectWindow 变更通知 

        private HandleWindow? _SelectWindow;

        public HandleWindow? SelectWindow
        {
            get { return this._SelectWindow; }
            set
            {
                if (this._SelectWindow != value)
                {
                    this._SelectWindow = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        private ObservableCollection<HandleWindow> _WindowList = new();
        public ObservableCollection<HandleWindow> WindowList
        {
            get { return this._WindowList; }
            set
            {
                if (this._WindowList != value)
                {
                    this._WindowList = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public void Cross_MouseDown()
        {
            windowApi.GetMoveMouseDownWindow((window) =>
            {
                SelectWindow = window;
                WindowList.Add(window);
            });
        }


        public void BorderlessWindow_Click()
        {
            if (SelectWindow == null)
            {
                return;
            }
            windowApi.BorderlessWindow(SelectWindow);
        }

        public void MaximizeWindow_Click()
        {
            if (SelectWindow == null)
            {
                return;
            }
            windowApi.MaximizeWindow(SelectWindow);
        }

        public void NormalWindow_Click()
        {
            if (SelectWindow == null)
            {
                return;
            }
            windowApi.NormalWindow(SelectWindow);
        }

        public void WindowKill_Click()
        {
            if (SelectWindow == null)
            {
                return;
            }
            SelectWindow.Kill();
        }

        public void ShowWindow_Click()
        {
            if (SelectWindow == null)
            {
                return;
            }
            windowApi.ShowWindow(SelectWindow);
        }

        public void HideWindow_Click()
        {
            if (SelectWindow == null)
            {
                return;
            }
            windowApi.HideWindow(SelectWindow);
        }

        public void ToWallerpaperWindow_Click()
        {
            if (SelectWindow == null)
            {
                return;
            }
            windowApi.ToWallerpaperWindow(SelectWindow);
        }

        public void ResetWallerpaper_Click()
        {
            windowApi.ResetWallerpaper();
        }
    }
}
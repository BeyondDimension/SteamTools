using Livet;
using MetroTrilithon.Lifetime;
using MetroTrilithon.Mvvm;
using SteamTools.Models;
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
        private MainWindowViewModel mainWindow;
        private AchievementWindowViewModel achievementWindow;
        private readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();

        /// <summary>
        /// 获取为当前主窗口提供的数据。
        /// </summary>
        public MainWindowViewModelBase MainWindow { get; private set; }

        public void Initialize(int appid = 0)
        {
            if (appid < 1)
            {
                this.mainWindow = new MainWindowViewModel(true);
                this.MainWindow = mainWindow;
            }
            else
            {
                this.achievementWindow = new AchievementWindowViewModel(true, appid);
                this.MainWindow = achievementWindow;
            }
        }

        public Window GetMainWindow()
        {
            if (this.MainWindow == this.mainWindow)
            {
                this.MainWindow
                           .Subscribe(nameof(MainWindowViewModel.SelectedItem), 
                           () => this.MainWindow.StatusBar = (this.MainWindow as MainWindowViewModel).SelectedItem)
                           .AddTo(this);
                return new MainWindow { DataContext = this.MainWindow, };
            }
            if (this.MainWindow == this.achievementWindow)
            {
                return new AchievementWindow { DataContext = this.MainWindow, };
            }
            throw new InvalidOperationException();
        }

        public bool ShowDialogWindow(string content)
        {
            return ShowDialogWindow(content, ProductInfo.Title);
        }

        public bool ShowDialogWindow(string content, string title)
        {
            var dialog = new DialogWindowViewModel
            {
                Content = content,
                Title = title
            };
            var window = new MessageDialog { DataContext = dialog };
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
            dialog.Topmost = true;
            dialog.Activate();
            dialog.Topmost = false;
            return dialog.DialogResult;
        }

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

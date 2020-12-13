using SteamTool.Model;
using SteamTools.Services;
using SteamTools.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SteamTools.ViewModels
{
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class TaskBarViewModel : Livet.ViewModel
    {
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        var mainWindow = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                        mainWindow.IsVisible = true;
                    }
                };
            }
        }

        public void ShowUrl()
        {
            //var mainWindow = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            //mainWindow.IsVisible = !mainWindow.IsVisible;

            if (SteamConnectService.Current.IsConnectToSteam)
            {
                Process.Start(SteamConnectService.Current.CurrentSteamUser.ProfileUrl);
            }
            else 
            {
                Process.Start(Const.MY_PROFILE_URL);
            }
        }

        /// <summary>
        /// 关闭软件
        /// </summary>
        public void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 跳转对应TabItem
        /// </summary>
        public void NavigateTabItem(string index)
        {
            var mainWindow = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            mainWindow.IsVisible = true;
            int.TryParse(index, out int i);
            if (i < 0)
                mainWindow.SelectedItem = mainWindow.SystemTabItems[Math.Abs(i) - 1];
            else
                mainWindow.SelectedItem = mainWindow.TabItems[i];

            TaskbarService.Current.Taskbar.TrayPopup.Visibility = Visibility.Collapsed;
        }
    }
}

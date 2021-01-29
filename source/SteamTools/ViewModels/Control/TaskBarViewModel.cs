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
                        var mainWindow = (WindowService.Current.MainWindow as MainWindowViewModel);
                        mainWindow.IsVisible = true;
                    }
                };
            }
        }

        public void ShowUrl()
        {
            if (SteamConnectService.Current.IsConnectToSteam)
            {
                Process.Start(new ProcessStartInfo { FileName = SteamConnectService.Current.CurrentSteamUser.ProfileUrl, UseShellExecute = true });
            }
            else
            {
                Process.Start(new ProcessStartInfo { FileName = Const.MY_PROFILE_URL, UseShellExecute = true });
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
            var mainWindow = (WindowService.Current.MainWindow as MainWindowViewModel);
            mainWindow.IsVisible = true;
            //int.TryParse(index, out int i);
            var sysTab = mainWindow.SystemTabItems.FirstOrDefault(f => f.Name == index);
            if (sysTab != null)
                mainWindow.SelectedItem = sysTab;
            else
                mainWindow.SelectedItem = mainWindow.TabItems.First(f => f.Name == index);
            TaskbarService.Current.Taskbar.TrayPopup.Visibility = Visibility.Collapsed;
        }
    }
}

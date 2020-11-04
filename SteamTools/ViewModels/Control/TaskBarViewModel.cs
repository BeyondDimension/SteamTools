using SteamTools.Win32;
using System;
using System.Collections.Generic;
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
                        mainWindow.Visible = !mainWindow.Visible;
                    }
                };
            }
        }

        public void ShowWindow()
        { 
            var mainWindow = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            mainWindow.Visible = !mainWindow.Visible;
        }

        /// <summary>
        /// 关闭软件
        /// </summary>
        public void ExitApplication()
        {
            Application.Current.Shutdown();
        }
    }
}

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
        /// <summary>
        /// 如果窗口没显示，就显示窗口
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        App.Current.MainWindow = App.Current.MainWindow ?? new MainWindow();
                        App.Current.MainWindow.Show();
                        //App.Current.MainWindow.Focus();
                        FlashTaskBar.FlashWindow(new WindowInteropHelper(App.Current.MainWindow).Handle);
                    }
                };
            }
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

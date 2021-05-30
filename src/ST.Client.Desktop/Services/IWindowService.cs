using ReactiveUI;
using System;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services
{
    public interface IWindowService
    {
        public static IWindowService Instance => DI.Get<IWindowService>();

        /// <summary>
        /// 获取为当前主窗口提供的数据。
        /// </summary>
        WindowViewModel MainWindow { get; }
        TaskBarWindowViewModel? TaskBarWindow { get; }

        void Init();

        void InitUnlockAchievement(int appid);

        /// <summary>
        /// 打开托盘菜单窗口
        /// </summary>
        void ShowTaskBarWindow(int x = 0, int y = 0);
        //Window GetMainWindow()
    }
}

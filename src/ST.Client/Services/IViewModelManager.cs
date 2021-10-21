using ReactiveUI;
using System;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IViewModelManager : IDisposableHolder, IDisposable
    {
        static IViewModelManager Instance => DI.Get<IViewModelManager>();

        /// <summary>
        /// 获取为当前主窗口提供的数据
        /// </summary>
        WindowViewModel MainWindow { get; }

        TaskBarWindowViewModel? TaskBarWindow { get; }

        void InitViewModels();

        void InitUnlockAchievement(int appid);

        /// <summary>
        /// 打开托盘菜单窗口
        /// </summary>
        void ShowTaskBarWindow(int x = 0, int y = 0);

        /// <inheritdoc cref="ShowTaskBarWindow(int, int)"/>
        void ShowTaskBarWindow(Point point) => ShowTaskBarWindow(point.X, point.Y);

        T GetMainPageViewModel<T>() where T : TabItemViewModel
        {
            if (MainWindow is MainWindowViewModel mainWindowViewModel)
            {
                return mainWindowViewModel.GetTabItemVM<T>();
            }
            throw new NotSupportedException();
        }
    }
}

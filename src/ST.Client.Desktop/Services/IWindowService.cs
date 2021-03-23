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
        WindowViewModel MainWindow { get;}

        void Initialize(int appid = 0);

        //Window GetMainWindow()
    }
}

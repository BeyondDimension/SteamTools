using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Application.UI.ViewModels;

namespace System.Application.Services
{
    /// <summary>
    /// 窗口服务
    /// </summary>
    public interface IWindowManager
    {
        public static IWindowManager Instance => DI.Get<IWindowManager>();

        /// <summary>
        /// 显示一个窗口
        /// </summary>
        /// <typeparam name="TWindowViewModel"></typeparam>
        /// <param name="customWindow"></param>
        /// <param name="viewModel"></param>
        /// <param name="title"></param>
        /// <param name="resizeMode"></param>
        /// <param name="isDialog"></param>
        /// <returns></returns>
        Task Show<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = false,
            bool isParent = true)
            where TWindowViewModel : PageViewModel, new();

        /// <inheritdoc cref="Show{TWindowViewModel}(CustomWindow, TWindowViewModel?, string, ResizeMode, bool, bool)"/>
        Task Show(Type typeWindowViewModel,
            CustomWindow customWindow,
            PageViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = false,
            bool isParent = true);

        /// <summary>
        /// 显示一个弹窗，返回 <see langword="true"/> 确定(仅当ViewModel继承自<see cref="DialogWindowViewModel"/>时生效)，<see langword="false"/> 取消
        /// </summary>
        /// <typeparam name="TWindowViewModel"></typeparam>
        /// <param name="customWindow"></param>
        /// <param name="viewModel"></param>
        /// <param name="title"></param>
        /// <param name="resizeMode"></param>
        /// <param name="isDialog"></param>
        /// <returns></returns>
        Task<bool> ShowDialog<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = true,
            bool isParent = true)
            where TWindowViewModel : PageViewModel, new();


        /// <inheritdoc cref="ShowDialog{TWindowViewModel}(CustomWindow, TWindowViewModel?, string, ResizeMode, bool, bool)"/>
        Task ShowDialog(Type typeWindowViewModel,
            CustomWindow customWindow,
            PageViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = true);

        /// <summary>
        /// 根据视图模型关闭窗口
        /// </summary>
        /// <param name="vm"></param>
        void CloseWindow(PageViewModel vm)
        {

        }

        /// <summary>
        /// 获取视图模型对应的窗口是否显示
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        bool IsVisibleWindow(PageViewModel vm)
        {
            return default;
        }

        /// <summary>
        /// 根据视图模型隐藏窗口
        /// </summary>
        /// <param name="vm"></param>
        void HideWindow(PageViewModel vm)
        {
        }

        /// <summary>
        /// 根据视图模型显示窗口
        /// </summary>
        /// <param name="vm"></param>
        void ShowWindow(PageViewModel vm)
        {

        }

        /// <summary>
        /// 执行当前路由后退操作
        /// </summary>
        void Pop()
        {

        }
    }
}

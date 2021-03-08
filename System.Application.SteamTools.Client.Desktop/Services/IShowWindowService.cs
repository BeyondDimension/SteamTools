using System.Application.UI.ViewModels;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services
{
    /// <summary>
    /// 显示窗口服务
    /// </summary>
    public interface IShowWindowService
    {
        public static IShowWindowService Instance => DI.Get<IShowWindowService>();

        /// <summary>
        /// 显示一个窗口
        /// </summary>
        /// <typeparam name="TWindowViewModel"></typeparam>
        /// <param name="customWindow"></param>
        /// <param name="title"></param>
        /// <param name="viewModel"></param>
        /// <param name="resizeMode"></param>
        /// <returns></returns>
        Task Show<TWindowViewModel>(
            CustomWindow customWindow,
            string title,
            TWindowViewModel? viewModel = null,
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize)
            where TWindowViewModel : WindowViewModel, new();

        /// <summary>
        /// 显示一个弹窗，返回 <see langword="true"/> 确定，<see langword="false"/> 取消
        /// </summary>
        /// <typeparam name="TDialogWindowViewModel"></typeparam>
        /// <param name="customWindow"></param>
        /// <param name="title"></param>
        /// <param name="viewModel"></param>
        /// <param name="resizeMode"></param>
        /// <returns></returns>
        Task<bool> ShowDialog<TDialogWindowViewModel>(
            CustomWindow customWindow,
            string title,
            TDialogWindowViewModel? viewModel = null,
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize)
            where TDialogWindowViewModel : DialogWindowViewModel, new();
    }
}
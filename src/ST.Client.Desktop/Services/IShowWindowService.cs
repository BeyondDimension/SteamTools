using System.Application.UI.ViewModels;
using System.Threading.Tasks;
using System.Windows;
#if __MOBILE__
using WindowViewModel = System.Application.UI.ViewModels.PageViewModel;
#endif

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
        /// <param name="viewModel"></param>
        /// <param name="title"></param>
        /// <param name="resizeMode"></param>
        /// <param name="isDialog"></param>
        /// <returns></returns>
        Task Show<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize,
            bool isDialog = false,
            bool isParent = true)
            where TWindowViewModel : WindowViewModel, new();

        /// <inheritdoc cref="Show{TWindowViewModel}(CustomWindow, TWindowViewModel?, string, ResizeModeCompat, bool)"/>
        Task Show(Type typeWindowViewModel,
            CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize,
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
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize,
            bool isDialog = true)
            where TWindowViewModel : WindowViewModel, new();


        /// <inheritdoc cref="ShowDialog{TWindowViewModel}(CustomWindow, TWindowViewModel?, string, ResizeModeCompat, bool)"/>
        Task ShowDialog(Type typeWindowViewModel,
            CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize,
            bool isDialog = true);


#if !__MOBILE__
        void CloseWindow(WindowViewModel vm);
        void HideWindow(WindowViewModel vm);
        void ShowWindow(WindowViewModel vm);
#else
        void Pop();
#endif
    }
}
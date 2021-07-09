using System.Application.Models;
using System.Runtime.Versioning;

namespace System.Application.Services
{
    public interface ISystemWindowApiService
    {
        /// <summary>
        /// 拖拽指针获取目标窗口
        /// </summary>
        /// <param name="action">目标窗口回调</param>
        [SupportedOSPlatform("Windows")]
        void GetMoveMouseDownWindow(Action<HandleWindow> action);

        /// <summary>
        /// 将传入窗口设置为无边框窗口化
        /// </summary>
        /// <param name="window"></param>
        [SupportedOSPlatform("Windows")]
        void BorderlessWindow(HandleWindow window);

        /// <summary>
        /// 最大化窗口
        /// </summary>
        /// <param name="window"></param>
        [SupportedOSPlatform("Windows")]
        void MaximizeWindow(HandleWindow window);

        /// <summary>
        /// 默认窗口
        /// </summary>
        /// <param name="window"></param>
        [SupportedOSPlatform("Windows")]
        void NormalWindow(HandleWindow window);

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="window"></param>
        [SupportedOSPlatform("Windows")]
        void ShowWindow(HandleWindow window);

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="window"></param>
        [SupportedOSPlatform("Windows")]
        void HideWindow(HandleWindow window);

        /// <summary>
        /// 窗口移至壁纸层
        /// </summary>
        /// <param name="window"></param>
        [SupportedOSPlatform("Windows")]
        void ToWallerpaperWindow(HandleWindow window);

        /// <summary>
        /// 刷新壁纸
        /// </summary>
        /// <param name="window"></param>
        [SupportedOSPlatform("Windows")]
        void ResetWallerpaper();

        [SupportedOSPlatform("Windows")]
        void SetActiveWindow(HandleWindow window);
    }
}
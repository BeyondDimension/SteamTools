using System.Application.Models;
using System.Runtime.Versioning;

namespace System.Application.Services
{
    /// <summary>
    /// 平台 Window API 服务
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    public interface INativeWindowApiService
    {
        static INativeWindowApiService? Instance => DI.Get_Nullable<INativeWindowApiService>();

        /// <summary>
        /// 拖拽指针获取目标窗口
        /// </summary>
        /// <param name="action">目标窗口回调</param>
        void GetMoveMouseDownWindow(Action<NativeWindowModel> action);

        /// <summary>
        /// 将传入窗口设置为无边框窗口化
        /// </summary>
        /// <param name="window"></param>
        void BorderlessWindow(NativeWindowModel window);

        /// <summary>
        /// 最大化窗口
        /// </summary>
        /// <param name="window"></param>
        void MaximizeWindow(NativeWindowModel window);

        /// <summary>
        /// 默认窗口
        /// </summary>
        /// <param name="window"></param>
        void NormalWindow(NativeWindowModel window);

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="window"></param>
        void ShowWindow(NativeWindowModel window);

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="window"></param>
        void HideWindow(NativeWindowModel window);

        /// <summary>
        /// 窗口移至壁纸层
        /// </summary>
        /// <param name="window"></param>
        void ToWallerpaperWindow(NativeWindowModel window);

        /// <summary>
        /// 获取桌面壁纸图片路径
        /// </summary>
        /// <returns></returns>
        string? GetWallerpaperImagePath();

        /// <summary>
        /// 刷新桌面壁纸
        /// </summary>
        /// <param name="window"></param>
        void ResetWallerpaper();

        void SetParentWindow(IntPtr source, IntPtr dest);

        void SetActiveWindow(NativeWindowModel window);

        /// <summary>
        /// 设置窗口点击穿透
        /// </summary>
        /// <param name="window"></param>
        void SetWindowPenetrate(IntPtr dest);

        /// <summary>
        /// 设置缩略图到指定窗口句柄
        /// </summary>
        IntPtr SetDesktopBackgroundToWindow(IntPtr dest, int width, int height);

        void BackgroundUpdate(IntPtr dest, int width, int height);

        void ReleaseBackground(IntPtr dest);
    }
}
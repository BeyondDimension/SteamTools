using ResizeMode = System.Int32;

namespace System.Application.Services
{
    public interface IDesktopPlatformService
    {
        void SetResizeMode(IntPtr hWnd, ResizeMode value);

        public const ResizeMode ResizeMode_NoResize = 0;
        public const ResizeMode ResizeMode_CanMinimize = 1;
        public const ResizeMode ResizeMode_CanResize = 2;
        public const ResizeMode ResizeMode_CanResizeWithGrip = 3;
    }
}
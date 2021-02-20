using System.Diagnostics;
using System.Windows;

namespace System.Application.Services.Implementation
{
    public class DesktopPlatformServiceImpl : IDesktopPlatformService
    {
        public virtual void SetResizeMode(IntPtr hWnd, ResizeMode value)
        {
        }

        public virtual string[] GetCPUName()
        {
            return new[] { string.Empty };
        }

        public virtual string[] GetGPUName()
        {
            return new[] { string.Empty };
        }

        public virtual bool IsWindowsServer()
        {
            return false;
        }

        public virtual string GetOSVersion()
        {
            return Environment.OSVersion.VersionString;
        }

        protected virtual bool Is64Bit_(Process process)
        {
            return false;
        }

        public bool Is64Bit(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;
            return Is64Bit_(process);
        }

        public virtual bool IsAdministrator()
        {
            return false;
        }

        public virtual void Enable_Fluent_Design_System_Style_Blur(IntPtr hWnd)
        {
        }

        public virtual string NETFrameworkDescription => "";
    }
}
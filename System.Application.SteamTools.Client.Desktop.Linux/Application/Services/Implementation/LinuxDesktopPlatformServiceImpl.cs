using System.Application.Models;
using System.Diagnostics;

namespace System.Application.Services.Implementation
{
    internal sealed class LinuxDesktopPlatformServiceImpl : IDesktopPlatformService
    {
        public void SetResizeMode(IntPtr hWnd, int value)
        {
        }

        public string GetCommandLineArgs(Process process)
        {
            return string.Empty;
        }

        public string? GetFileName(TextReaderProvider provider)
        {
            return null;
        }
    }
}
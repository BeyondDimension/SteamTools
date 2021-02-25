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

        public void SetBootAutoStart(bool isAutoStart, string name)
        {

        }

        public string? GetSteamDirPath()
        {
            return null;
        }

        public string? GetSteamProgramPath()
        {
            return null;
        }

        public string GetLastSteamLoginUserName() => string.Empty;
    }
}
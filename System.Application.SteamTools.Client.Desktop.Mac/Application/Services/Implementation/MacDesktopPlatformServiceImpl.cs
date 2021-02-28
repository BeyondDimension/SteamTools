using System.Application.Models;
using System.Diagnostics;

namespace System.Application.Services.Implementation
{
    internal sealed class MacDesktopPlatformServiceImpl : IDesktopPlatformService
    {
        public void SetResizeMode(IntPtr hWnd, int value)
        {
        }

        public string GetCommandLineArgs(Process process)
        {
            return string.Empty;
        }

        void IDesktopPlatformService.StartProcess(string name, string filePath)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"-a \"{name}\" \"{filePath}\"",
            });
            p?.Close();
        }

        public string? GetFileName(TextReaderProvider provider)
        {
            switch (provider)
            {
                case TextReaderProvider.VSCode:
                    return "Visual Studio Code";
                case TextReaderProvider.Notepad:
                case TextReaderProvider.NotepadPlusPlus:
                    return "TextEdit";
                default:
                    return null;
            }
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
using System.Application.Models;
using System.Diagnostics;
using System.IO;

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

        public const string kate = "kate";
        public const string vi = "vi";

        public string? GetFileName(TextReaderProvider provider)
        {
            return vi;
        }

        public void SetBootAutoStart(bool isAutoStart, string name)
        {
        }

        public string? GetSteamDirPath()
        {
            var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(rootPath, "Steam");
        }

        public string? GetSteamProgramPath()
        {
            return "/usr/bin/steam";
        }

        public string GetLastSteamLoginUserName() => string.Empty;

        public void SetCurrentUser(string userName)
        {
        }

        static string GetMachineSecretKey()
        {
            var filePath = "/etc/machine-id";
            return File.ReadAllText(filePath);
        }

        static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey = IDesktopPlatformService.GetMachineSecretKey(GetMachineSecretKey);

        public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;

        public bool? IsLightOrDarkTheme => null;

        public void SetLightOrDarkThemeFollowingSystem(bool enable)
        {

        }
    }
}
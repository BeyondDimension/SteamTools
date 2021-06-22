using System.Application.Models;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace System.Application.Services.Implementation
{
    internal sealed partial class LinuxDesktopPlatformServiceImpl : IDesktopPlatformService
    {
        public void SetResizeMode(IntPtr hWnd, ResizeModeCompat value)
        {
        }

        public string GetCommandLineArgs(Process process)
        {
            return string.Empty;
        }

        public void OpenFolder(string dirPath)
        {
        }

        public const string kate = "kate";
        public const string vi = "vi";

        public string? GetFileName(TextReaderProvider provider)
        {
            return vi;
        }

        public void SetSystemSessionEnding(Action action)
        {

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

        public Process StartAsInvoker(string fileName)
        {
            return Process.Start(fileName);
        }

        public Process? GetProcessByPortOccupy(ushort port, bool isTCPorUDP = true)
        {
            return null;
        }

        public bool IsAdministrator => false;

        public void UnelevatedProcessStart(string cmdArgs)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
using System.Diagnostics;

namespace System.Application.Services.Implementation
{
    partial class WindowsDesktopPlatformServiceImpl
    {
        #region rundll32 command list
        public const string OpenDesktopIconsSettingsCommandArguments = "shell32.dll,Control_RunDLL desk.cpl,,0";
        public const string OpenGameControllersCommandArguments = "shell32.dll,Control_RunDLL joy.cpl";
        #endregion

        public void OpenDesktopIconsSettings()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = OpenDesktopIconsSettingsCommandArguments,
                UseShellExecute = true,
            });
        }

        public void OpenGameControllers()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = OpenGameControllersCommandArguments,
                UseShellExecute = true,
            });
        }
    }
}
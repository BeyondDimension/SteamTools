#if MONO_MAC
using MonoMac.Foundation;
#elif XAMARIN_MAC
using Foundation;
#endif
using System.Application.Models;
using System.Diagnostics;
using System.IO;

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
            var value = string.Format(
                "{0}Users{0}{1}{0}Library{0}Application Support{0}Steam",
                Path.DirectorySeparatorChar,
                Environment.UserName);
            return value;
        }

        public string? GetSteamProgramPath()
        {
            return null;
        }

        public string GetLastSteamLoginUserName() => string.Empty;

        public void SetCurrentUser(string userName)
        {
        }

        static string GetMachineSecretKey()
        {
            return string.Empty;
        }

        static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey = IDesktopPlatformService.GetMachineSecretKey(GetMachineSecretKey);

        public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;

        public bool? IsLightOrDarkTheme
        {
            get
            {
                try
                {
                    var value = NSUserDefaults.StandardUserDefaults.StringForKey("AppleInterfaceStyle");
                    switch (value)
                    {
                        case "Light":
                            return true;
                        case "Dark":
                            return false;
                    }
                }
                catch
                {
                }
                return null;
            }
        }

        public void SetLightOrDarkThemeFollowingSystem(bool enable)
        {
        }
    }
}
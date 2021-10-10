using System.Application.Internals;
using System.Application.Models;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services.Implementation
{
    internal sealed partial class LinuxDesktopPlatformServiceImpl : IDesktopPlatformService
    {
        public const string xdg = "xdg-open";
        public const string vi = "vi";
        public const string VSC = "code";

        public void SetResizeMode(IntPtr hWnd, ResizeModeCompat value)
        {
        }

        ValueTask IPlatformService.AdminShellAsync(string shell, bool admin) => UnixHelper.RunShellAsync(shell, admin);

        public string GetCommandLineArgs(Process process)
        {
            return string.Empty;
        }

        public void OpenFolderByDirectoryPath(DirectoryInfo info)
        {
            Process2.StartPath(xdg, info.FullName);
        }

        public void OpenFolderSelectFilePath(FileInfo info)
        {
            if (info.DirectoryName == null) return;
            Process2.StartPath(xdg, info.DirectoryName);
        }

        public string? GetFileName(TextReaderProvider provider) => provider switch
        {
            TextReaderProvider.VSCode => VSC,
            TextReaderProvider.Notepad => xdg,
            _ => null,
        };

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

        static readonly Lazy<string> _SteamProgramPath = new(() => string.Format("{0}usr{0}bin{0}steam", Path.DirectorySeparatorChar));

        public string? GetSteamProgramPath() => _SteamProgramPath.Value;

        public string GetLastSteamLoginUserName() => string.Empty;

        public string? GetRegistryVdfPath()
        {
            var value = string.Format(
                    "{1}{0}.steam{0}registry.vdf",
                    Path.DirectorySeparatorChar,
                   Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            return value;
        }

        public void SetCurrentUser(string userName)
        {
            try
            {
                var registryVdfPath = GetRegistryVdfPath();
                if (!string.IsNullOrWhiteSpace(registryVdfPath) && File.Exists(registryVdfPath))
                {
                    dynamic v = VdfHelper.Read(registryVdfPath);
                    var autoLoginUser = v.Value.HKCU.Software.Valve.Steam.AutoLoginUser;
                    if (autoLoginUser != null)
                    {
                        var oldStr = $"\t\t\t\t\t\"AutoLoginUser\"\t\t\"{autoLoginUser}\"\n";
                        var newStr = $"\t\t\t\t\t\"AutoLoginUser\"\t\t\"{userName}\"\n";
                        VdfHelper.UpdateValueByReplaceNoPattern(registryVdfPath, oldStr, newStr);
                    }
                    else
                    {
                        Log.Error("SetCurrentUser", "UpdateAuthorizedAutoLoginUser Fail(0). AutoLoginUser IsNull");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("SetCurrentUser", e, "UpdateAuthorizedAutoLoginUser Fail(0).");
            }
        }

        static string GetMachineSecretKey()
        {
            var filePath = string.Format("{0}etc{0}machine-id", Path.DirectorySeparatorChar);
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

        public void UnelevatedProcessStart(string cmdArgs)
        {
            throw new PlatformNotSupportedException();
        }

        public bool SetAsSystemProxy(bool state, IPAddress? ip, int port)
        {
            var shellContent = new StringBuilder();
            if (state)
            {
                var hasIpAndProt = ip != null && port >= 0;
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy mode 'manual'");
                if (hasIpAndProt)
                {
                    shellContent.AppendLine($"gsettings set org.gnome.system.proxy.http host '{ip}'");
                    shellContent.AppendLine($"gsettings set org.gnome.system.proxy.http port {port}");
                    shellContent.AppendLine($"gsettings set org.gnome.system.proxy.https host '{ip}'");
                    shellContent.AppendLine($"gsettings set org.gnome.system.proxy.https port {port}");
                }
            }
            else
            {
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy mode 'none'");
            }
            ((IPlatformService)this).RunShell(shellContent.ToString(), false);
            return true;
        }
    }
}
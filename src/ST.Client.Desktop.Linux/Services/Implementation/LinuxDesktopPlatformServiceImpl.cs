using System.Application.Models;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services.Implementation
{
    internal sealed partial class LinuxDesktopPlatformServiceImpl : IDesktopPlatformService
    {
        public void SetResizeMode(IntPtr hWnd, ResizeModeCompat value)
        {
        }
        async ValueTask IPlatformService.AdminShellAsync(string shell, bool admin)
        {
            var file = new FileInfo(Path.Combine(IOPath.AppDataDirectory, $@"{(admin ? "sudo" : "")}shell.sh"));

            if (file.Exists)
                file.Delete();
            var scriptContent = new StringBuilder();
            if (admin)
            {
                TextBoxWindowViewModel vm = new()
                {
                    Title = AppResources.MacSudoPasswordTips,
                    InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
                    Description = $"sudo {shell}",
                };
                await TextBoxWindowViewModel.ShowDialogAsync(vm);
                if (string.IsNullOrWhiteSpace(vm.Value))
                    return;
                scriptContent.AppendLine($"echo \"{vm.Value}\" | sudo -S {shell}");
            }
            else
            {
                scriptContent.AppendLine(shell);
            }
            using (var stream = file.CreateText())
            {
                stream.Write(scriptContent);
                stream.Flush();
                stream.Close();
            }
            using (var p = new Process())
            {
                p.StartInfo.FileName = "/bin/bash";
                p.StartInfo.Arguments = $"\"{file.FullName}\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Exited += (object? _, EventArgs _) =>
                {
                    if (file.Exists)
                        file.Delete();
                    if (p.ExitCode != 0)
                    {
                        ((IPlatformService)this).AdminShell(shell);
                    }
                };
                p.Start();
                var ret = p.StandardOutput.ReadToEnd();
                p.Kill();
                if (file.Exists)
                    file.Delete();
            }
        }
        public string GetCommandLineArgs(Process process)
        {
            return string.Empty;
        }

        public const string xdg = "xdg-open";

        public void OpenFolderByDirectoryPath(DirectoryInfo info)
        {
            Process2.StartPath(xdg, info.FullName);
        }

        public void OpenFolderSelectFilePath(FileInfo info)
        {
            if (info.DirectoryName == null) return;
            Process2.StartPath(xdg, info.DirectoryName);
        }

        public const string vi = "vi";
        const string VSC = "code";

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

        public string? GetSteamProgramPath()
        {
            return "/usr/bin/steam";
        }

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
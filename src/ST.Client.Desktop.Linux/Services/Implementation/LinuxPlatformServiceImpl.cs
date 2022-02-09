using System.Application.Models;
using System.Application.Properties;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed partial class LinuxPlatformServiceImpl : IPlatformService
    {
        const string TAG = "LinuxPlatformS";
        public const string xdg = "xdg-open";
        public const string vi = "vi";
        public const string VSC = "code";
        /// <summary>
        /// 临时 保存用户系统密码
        /// </summary> 
        public string SystemUserPassword { get; set; } = string.Empty;
        public LinuxPlatformServiceImpl()
        {
            // 平台服务依赖关系过于复杂，在构造函数中不得注入任何服务，由函数中延时加载调用服务
        }

        public void SetResizeMode(IntPtr hWnd, ResizeMode value)
        {
        }

        ValueTask IPlatformService.RunShellAsync(string script, bool admin) => UnixHelper.RunShellAsync(script, admin);

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
                        autoLoginUser = userName;
                        VdfHelper.Write(registryVdfPath, v);
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

        static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey = IPlatformService.GetMachineSecretKey(GetMachineSecretKey);

        public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;

        public bool? IsLightOrDarkTheme => null;

        public void SetLightOrDarkThemeFollowingSystem(bool enable)
        {
        }

        //public Process? GetProcessByPortOccupy(ushort port, bool isTCPorUDP = true)
        //{
        //    return null;
        //}

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

        public void SystemLock(int waitSecond = 30)
        {
            throw new NotImplementedException();
        }

        public async void SystemShutdown(int waitSecond = 30)
        {
            await Task.Delay(waitSecond);
            RunShell($"echo \"{SystemUserPassword}\" | sudo shutdown -h now");
        }

        public async void SystemSleep(int waitSecond = 30)
        {
            await Task.Delay(waitSecond);

            RunShell($"echo \"{SystemUserPassword}\" | sudo sh -c \" echo mem > /sys/pwoer/state\"");
            //await ((IPlatformService)this).RunShellAsync("sudo sh -c \" echo mem > /sys/pwoer/state\"", false);

        }

        public async void SystemHibernate(int waitSecond = 30)
        {

            await Task.Delay(waitSecond);
            RunShell($"echo \"{SystemUserPassword}\" | sudo sh -c \" echo disk > /sys/pwoer/state\"");
        }
        private string RunShell(string shell)
        {
            try
            {
                using var p = new Process();
                p.StartInfo.FileName = UnixHelper.BinBash;
                p.StartInfo.Arguments = "";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.StandardInput.WriteLine(shell);
                //p.StandardInput.WriteLine(SystemUserPassword);
                p.StandardInput.Close();
                string result = p.StandardOutput.ReadToEnd();
                p.StandardOutput.Close();
                p.WaitForExit();
                p.Close();
                p.Dispose();
                return result;
            }
            catch (Exception e)
            {
                e.LogAndShowT(TAG);
            }
            return string.Empty;
        }

        public async void TryGetSystemUserPassword(sbyte retry_count = 4)
        {
            if (string.IsNullOrWhiteSpace(SystemUserPassword))
            {
                TextBoxWindowViewModel vm = new()
                {
                    Title = AppResources.LinuxSudoTips,
                    InputType = TextBoxWindowViewModel.TextBoxInputType.Password
                };
                var pwd = await TextBoxWindowViewModel.ShowDialogAsync(vm);
                if (!string.IsNullOrWhiteSpace(pwd))
                {
                    if (!string.IsNullOrWhiteSpace(RunShell($"echo \"{pwd}\" | sudo -S sh -c \"sudo -n true\"")))
                    {
                        SystemUserPassword = pwd;
                    }
                    else
                    {
                        //密码错误重试3次
                        retry_count--;
                        if (retry_count > 0)
                            TryGetSystemUserPassword(retry_count);
                        else
                        {
                            vm.Title = AppResources.LocalAuth_ProtectionAuth_PasswordError;
                            vm.InputType = TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText;
                            vm.Show();
                        }
                    }
                }
            }
        }

        public string DefaultHostsContent => SR.hosts.Replace("${USERNAME}", Environment.UserName);
    }
}
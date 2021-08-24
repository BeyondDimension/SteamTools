#if MONO_MAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif XAMARIN_MAC
using AppKit;
using Foundation;
#endif
using System.Application.Models;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services.Implementation
{
    internal sealed partial class MacDesktopPlatformServiceImpl : IDesktopPlatformService
    {
        public void SetResizeMode(IntPtr hWnd, ResizeModeCompat value)
        {
        }

        public string GetCommandLineArgs(Process process)
        {
            return string.Empty;
        }

        string[] IPlatformService.GetMacNetworksetup()
        {
            using var p = new Process();
            p.StartInfo.FileName = "networksetup";
            p.StartInfo.Arguments = "-listallnetworkservices";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            var ret = p.StandardOutput.ReadToEnd().Replace("An asterisk (*) denotes that a network service is disabled.", "");
            p.Kill();
            return ret.Split("\n");
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
            using var p = new Process();
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
            //var pInfo = new ProcessStartInfo
            //{
            //    FileName = "/bin/bash",
            //    Arguments = $"{file.FullName}"
            //};
            ////var pInfo = new ProcessStartInfo
            ////{
            ////    FileName = Path.Combine(IOPath.AppDataDirectory, $@"sudoShell"),
            ////    Arguments = $"-p '{file.FullName}'"
            ////};
            //pInfo.UseShellExecute = false;
            //var p = Process.Start(pInfo);

            //if (p == null) throw new FileNotFoundException("Shell");
            //p.Exited += (object? _, EventArgs _) =>
            //{
            //    if (p.ExitCode != 0)
            //    {
            //        ((IPlatformService)this).AdminShell(shell);
            //    }
            //    else
            //    {
            //        if (file.Exists)
            //            file.Delete();
            //    }
            //}; 
            //p.Close();
            //if (file.Exists)
            //    file.Delete();
#if MONO_MAC
#elif XAMARIN_MAC
            //var edithost = new NSAppleScript($"do shell script \"'${shell}'\" with administrator privileges");
            //var state = edithost.CompileAndReturnError(out var error); 
            //return state;
            //var pInfo = new ProcessStartInfo
            //{
            //    FileName = "sudo",
            //    Arguments = $"security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain {filePath}",
            //};
            //pInfo.UseShellExecute = true;
            //var p = Process.Start(pInfo);
            //p.Close(); 
#endif
        }

        public static void OpenFile(string appName, string filePath) => NSWorkspace.SharedWorkspace.OpenFile(filePath, appName);

        void IDesktopPlatformService.StartProcess(string name, string filePath)
        {
            //if (name == TextEdit || name == VSC)
            //{
            //    // https://developer.apple.com/documentation/appkit/nsworkspace
            //    OpenFile(filePath, name);
            //    return;
            //}

            var pInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"-a \"{name}\" \"{filePath}\"",
            };
            pInfo.UseShellExecute = true;
            var p = Process.Start(pInfo);
            if (p == null) throw new FileNotFoundException(name);
            p.Close();
        }

        Process? IDesktopPlatformService.OpenProcess(string name, string? arguments)
        {
            var p = IDesktopPlatformService.Instance.OpenProcess(name, $"\"{arguments}\"", true);
            if (p == null) throw new FileNotFoundException(name);
            p.Close();
            return p;
        }

        public void SetSystemSessionEnding(Action action)
        {

        }

        public void OpenFolder(string dirPath)
        {
            if (IOPath.IsDirectory(dirPath))
            {
                NSWorkspace.SharedWorkspace.SelectFile(null, dirPath);
            }
            else
            {
                NSWorkspace.SharedWorkspace.ActivateFileViewer(new[] { new NSUrl(dirPath, isDir: false) });
            }
        }

        const string TextEdit = "TextEdit";
        const string VSC = "Visual Studio Code";

        public string? GetFileName(TextReaderProvider provider) => provider switch
        {
            TextReaderProvider.VSCode => VSC,
            TextReaderProvider.Notepad => TextEdit,
            _ => null,
        };

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
            var value = string.Format(
                 "{0}Applications{0}Steam.app{0}Contents{0}MacOS{0}steam_osx",
                 Path.DirectorySeparatorChar);
            return value;
        }

        public string GetLastSteamLoginUserName() => string.Empty;
        public string? GetRegistryVdfPath() {
            var value = string.Format(
                    "{0}Users{0}{1}{0}Library{0}Application Support{0}Steam{0}registry.vdf",
                    Path.DirectorySeparatorChar,
                    Environment.UserName);
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
                        Log.Error("SetCurrentUser","UpdateAuthorizedAutoLoginUser Fail(0). AutoLoginUser IsNull");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("SetCurrentUser", e, "UpdateAuthorizedAutoLoginUser Fail(0)."); 
            } 

        }

        #region MachineKey

        // https://blog.csdn.net/lipingqingqing/article/details/8843606
        // https://forums.xamarin.com/discussion/54210/iokit-framework
        // https://gist.github.com/chamons/82ab06f5e83d2cb10193

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern IntPtr IOServiceMatching(string s);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator, uint options);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern int IOObjectRelease(uint o);

        static string GetIOPlatformExpertDevice(string keyName)
        {
            var value = string.Empty;
            var platformExpert = IOServiceGetMatchingService(0, IOServiceMatching("IOPlatformExpertDevice"));
            if (platformExpert != 0)
            {
                var key = (NSString)keyName;
                var valueIntPtr = IORegistryEntryCreateCFProperty(platformExpert, key.Handle, IntPtr.Zero, 0);
                if (valueIntPtr != IntPtr.Zero)
                {
                    value = NSString.FromHandle(valueIntPtr) ?? value;
                }
                IOObjectRelease(platformExpert);
            }

            return value;
        }

        static string GetSerialNumber() => GetIOPlatformExpertDevice("IOPlatformSerialNumber");

#if DEBUG
        public static string GetPlatformUUID() => GetIOPlatformExpertDevice("IOPlatformUUID");
#endif

        static string GetMachineSecretKey()
        {
            var value = GetSerialNumber();
            return value;
            //return DeviceInfo.Model + DeviceInfo.Name;
        }

        #endregion

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
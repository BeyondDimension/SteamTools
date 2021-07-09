#if MONO_MAC
using MonoMac.Foundation;
#elif XAMARIN_MAC
using Foundation;
#endif
using System.Application.Models;
using System.Application.UI.Resx;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        bool IPlatformService.AdminShell(string shell)
        {
            Threading.Tasks.Task.Run(async () =>
            {
                var file = new FileInfo(Path.Combine(IOPath.AppDataDirectory, $@"sudoShell.js"));
                if (file.Exists)
                    file.Delete();
                var vm = new UI.ViewModels.PasswordWindowViewModel();
                vm.Title = AppResources.MacSudoPasswordTips;
                await IShowWindowService.Instance.ShowDialog(CustomWindow.Password, vm, string.Empty, ResizeModeCompat.CanResize);
                var scriptContent = new Text.StringBuilder();
                scriptContent.AppendLine($"#!/bin/bash -e");
                scriptContent.AppendLine($"echo \"{vm.Password}\" | sudo -S {shell} | exit");
                using (var stream = file.CreateText())
                {
                    stream.Write(scriptContent);
                    stream.Flush();
                    stream.Dispose();
                }
                var pInfo = new ProcessStartInfo
                {
                    FileName = $"/bin/bash",
                    Arguments = file.FullName
                };
                //pInfo.UseShellExecute = true;
                var p = Process.Start(pInfo);
                if (p == null) throw new FileNotFoundException("Shell");
                p.Close(); 
                p.Exited += (object? _, EventArgs _) =>
                {

                    if (p.ExitCode != 0)
                        IPlatformService.Instance.AdminShell(shell);
                    else {
                        if (file.Exists)
                            file.Delete();
                    }
                };
                if (file.Exists)
                    file.Delete();
            }).ContinueWith(s => s.Dispose());
            return true;
#if MONO_MAC
            //return false;
#elif XAMARIN_MAC
            var edithost = new NSAppleScript($"do shell script \"'${shell}'\" with administrator privileges");
            var state = edithost.CompileAndReturnError(out var error); 
            return state;
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
        void IDesktopPlatformService.StartProcess(string name, string filePath)
        {
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


        void IDesktopPlatformService.OpenProcess(string name)
        {
            var pInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"{name}",
            };
            pInfo.UseShellExecute = true;
            var p = Process.Start(pInfo);
            if (p == null) throw new FileNotFoundException(name);
            p.Close();
        }

        void IDesktopPlatformService.OpenProcess(string name, string arguments)
        {
            var pInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"{name} \"{arguments}\"",
            };
            pInfo.UseShellExecute = true;
            var p = Process.Start(pInfo);
            if (p == null) throw new FileNotFoundException(name);
            p.Close();
        }
        public void SetSystemSessionEnding(Action action)
        {

        }

        public void OpenFolder(string dirPath)
        {

        }

        public string? GetFileName(TextReaderProvider provider)
        {
            switch (provider)
            {
                case TextReaderProvider.VSCode:
                    return "Visual Studio Code";
                case TextReaderProvider.Notepad:
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
            var value = string.Format(
                 "{0}{1}{0}Applications{0}Steam.app",
                 Path.DirectorySeparatorChar,
                Environment.UserName);
            return value;
        }

        public string GetLastSteamLoginUserName() => string.Empty;

        public void SetCurrentUser(string userName)
        {
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
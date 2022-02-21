#if MONO_MAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif XAMARIN_MAC
using AppKit;
using Foundation;
#endif
using System.Application.Models;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed partial class MacPlatformServiceImpl : IPlatformService
    {
        const string TAG = "MacPlatformS";
        public const string TextEdit = "TextEdit";
        public const string VSC = "Visual Studio Code";

        public MacPlatformServiceImpl()
        {
            // 平台服务依赖关系过于复杂，在构造函数中不得注入任何服务，由函数中延时加载调用服务
        }

        public void SetResizeMode(IntPtr hWnd, ResizeMode value)
        {
        }

        public string GetCommandLineArgs(Process process)
        {
            return string.Empty;
        }

        string[] IPlatformService.GetMacNetworkSetup()
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

        ValueTask IPlatformService.RunShellAsync(string script, bool admin) => UnixHelper.RunShellAsync(script, admin);

        public static void OpenFile(string appName, string filePath) => NSWorkspace.SharedWorkspace.OpenFile(filePath, appName);

        public void SetSystemSessionEnding(Action action)
        {

        }

        public void OpenFolderByDirectoryPath(DirectoryInfo info)
        {
            //NSWorkspace.SharedWorkspace.SelectFile(string.Empty, info.FullName);
            NSWorkspace.SharedWorkspace.ActivateFileViewer(new[] { new NSUrl(info.FullName, isDir: true) });
        }

        public void OpenFolderSelectFilePath(FileInfo info)
        {
            NSWorkspace.SharedWorkspace.ActivateFileViewer(new[] { new NSUrl(info.FullName, isDir: false) });
        }

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
        public string? GetRegistryVdfPath()
        {
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
                _ = IOObjectRelease(platformExpert);
            }

            return value;
        }

        static string GetSerialNumber() => GetIOPlatformExpertDevice("IOPlatformSerialNumber");

#if DEBUG
        static string GetPlatformUUID() => GetIOPlatformExpertDevice("IOPlatformUUID");
#endif

        static string GetMachineSecretKey()
        {
            var value = GetSerialNumber();
            return value;
            //return DeviceInfo.Model + DeviceInfo.Name;
        }

        #endregion

        static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey = IPlatformService.GetMachineSecretKey(GetMachineSecretKey);

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
            IPlatformService @this = this;
            var stringList = @this.GetMacNetworkSetup();
            var shellContent = new StringBuilder();
            foreach (var item in stringList)
            {
                if (item.Trim().Length > 0)
                {
                    if (state)
                    {
                        shellContent.AppendLine($"networksetup -setwebproxy '{item}' '{ip}' {port}");
                        shellContent.AppendLine($"networksetup -setwebproxystate '{item}' on");
                        shellContent.AppendLine($"networksetup -setsecurewebproxy '{item}' '{ip}' {port}");
                        shellContent.AppendLine($"networksetup -setsecurewebproxystate '{item}' on");
                    }
                    else
                    {
                        shellContent.AppendLine($"networksetup -setwebproxystate '{item}' off");
                        shellContent.AppendLine($"networksetup -setsecurewebproxystate '{item}' off");
                    }
                }
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
            RunOsaScript("tell application \"Finder\" to shut down");
        }

        public async void SystemSleep(int waitSecond = 30)
        {
            await Task.Delay(waitSecond);
            RunOsaScript("tell application \"Finder\" to sleep");
        }

        public void SystemHibernate(int waitSecond = 30)
        {
            throw new NotImplementedException();
        }

        private string RunOsaScript(string shell)
        {
            try
            {
                using var p = new Process();
                p.StartInfo.FileName = "osascript";
                p.StartInfo.Arguments = $" -e '{shell}";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string result = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Close();
                return result;
            }
            catch (Exception e)
            {
                e.LogAndShowT(TAG);
            }
            return string.Empty;
        }
    }
}
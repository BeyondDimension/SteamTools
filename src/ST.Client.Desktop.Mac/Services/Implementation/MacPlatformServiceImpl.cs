#if MONO_MAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.Security;
#elif XAMARIN_MAC
using AppKit;
using Foundation;
using Security;
#endif
using System.Application.Models;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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

        internal static bool IsCertificateInstalled(X509Certificate2 certificate2)
        {
#if MONO_MAC
            using var p = new Process();
            p.StartInfo.FileName = "security";
            p.StartInfo.Arguments = $" verify-cert -c \"{IReverseProxyService.Instance.CertificateManager.GetCerFilePathGeneratedWhenNoFileExists()}\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            var returnStr = p.StandardOutput.ReadToEnd().TrimEnd();
            p.Kill();
            return returnStr.Contains("...certificate verification successful.", StringComparison.OrdinalIgnoreCase);
#elif XAMARIN_MAC
            bool result = false;
            var scer = new SecCertificate(cer);
            var addCertificate = new SecRecord(scer);
            var cerTrust = SecKeyChain.QueryAsRecord(addCertificate, out var t2code);
            if (cerTrust != SecStatusCode.ItemNotFound)
            {
                using (var trust = new SecTrust(cerTrust, null))
                {
                    trust.SetPolicy(policy);
                    trust.SetAnchorCertificates(fcollection);
                    result=trust.Evaluate(out var error);
                    Toast.Show(error.Description);
                }
            }
            return result;
#endif

        }

        bool IPlatformService.IsCertificateInstalled(X509Certificate2 certificate2) => IsCertificateInstalled(certificate2);

        ValueTask IPlatformService.RunShellAsync(string script, bool admin) => RunShellAsync(script, admin);

        async ValueTask<bool?> IPlatformService.TrustRootCertificate(string filePath)
        {
            var script = $"security add-trusted-cert -d -r trustRoot -k /Users/{Environment.UserName}/Library/Keychains/login.keychain-db \\\"{filePath}\\\"";
            TextBoxWindowViewModel vm = new()
            {
                Title = AppResources.MacTrustRootCertificateTips,
                InputType = TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText,
                Description = AppResources.MacTrustRootCertificateTips,
            };
            var scriptContent = $"osascript -e 'tell app \"Terminal\" to do script \"sudo -S {script}\"'";
            var msg = UnixHelper.RunShell(scriptContent.ToString());
            if (await TextBoxWindowViewModel.ShowDialogAsync(vm) == null)
                return null;
            if (!string.IsNullOrWhiteSpace(msg))
            {
                Toast.Show(msg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 尝试删除证书
        /// </summary>
        /// <param name="certificate2">要删除的证书</param>
        async void IPlatformService.RemoveCertificate(X509Certificate2 certificate2)
        {
#if MONO_MAC
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadWrite);
                store.Remove(certificate2);
            }
            catch
            {
                //出现错误尝试命令删除
                await RunShellAsync($"security delete-certificate -Z {certificate2.GetCertHashString()}", true);
            }
#elif XAMARIN_MAC
            var itemCertificate = new SecRecord(new SecCertificate(certificate2));
            var cers = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
            if (code != SecStatusCode.ItemNotFound)
            {
                var rcode = SecKeyChain.Remove(cers);
                if (rcode != SecStatusCode.Success && rcode != SecStatusCode.ItemNotFound)
                {
                    await RunShellAsync($"security delete-certificate -Z {certificate2.GetCertHashString()}", true);
                }
            }
            //using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadOnly);
            //var lisrts = store.Certificates.Find(X509FindType.FindByIssuerName, IHttpProxyService.RootCertificateName, false);
            //foreach (var item in lisrts)
            //{
            //    var ces2 = new SecCertificate(item);
            //    var itemCertificate = new SecRecord(ces2);
            //    var cers = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
            //    if (code != SecStatusCode.ItemNotFound)
            //    {
            //        var rcode = SecKeyChain.Remove(cers);
            //        if (rcode != SecStatusCode.Success && rcode != SecStatusCode.ItemNotFound)
            //        {
            //            await RunShellAsync($"security delete-certificate -Z {item.GetCertHashString()}", true);
            //        }
            //    }
            //}
#endif

        }

        static async ValueTask RunShellAsync(string script, bool admin)
        {
            var scriptContent = new StringBuilder();
            if (admin)
            {
                TextBoxWindowViewModel vm = new()
                {
                    Title = AppResources.MacSudoPasswordTips,
                    InputType = TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText,
                    Description = $"sudo {script}",
                };
                if (await TextBoxWindowViewModel.ShowDialogAsync(vm) == null)
                    return;
                scriptContent.AppendLine($"osascript -e 'tell app \"Terminal\" to do script \"sudo -S {script}\"'");
            }
            else
                scriptContent.AppendLine(script);
            var msg = UnixHelper.RunShell(scriptContent.ToString());
            if (!string.IsNullOrWhiteSpace(msg))
                Toast.Show(msg);
        }

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
            var value = $"{IOPath.UnixDirectorySeparatorCharAsString}Users{IOPath.UnixDirectorySeparatorCharAsString}{Environment.UserName}{IOPath.UnixDirectorySeparatorCharAsString}Library{IOPath.UnixDirectorySeparatorCharAsString}Application Support{IOPath.UnixDirectorySeparatorCharAsString}Steam";
            return value;
        }

        string? IPlatformService.GetSteamDynamicLinkLibraryPath()
        {
            var value = $"{IOPath.UnixDirectorySeparatorCharAsString}Users{IOPath.UnixDirectorySeparatorCharAsString}{Environment.UserName}{IOPath.UnixDirectorySeparatorCharAsString}Library{IOPath.UnixDirectorySeparatorCharAsString}Application Support{IOPath.UnixDirectorySeparatorCharAsString}Steam{IOPath.UnixDirectorySeparatorCharAsString}Steam.AppBundle{IOPath.UnixDirectorySeparatorCharAsString}Steam{IOPath.UnixDirectorySeparatorCharAsString}Contents{IOPath.UnixDirectorySeparatorCharAsString}MacOS";
            return value;
        }

        const string OSXSteamProgramPath = $"{IOPath.UnixDirectorySeparatorCharAsString}Applications{IOPath.UnixDirectorySeparatorCharAsString}Steam.app{IOPath.UnixDirectorySeparatorCharAsString}Contents{IOPath.UnixDirectorySeparatorCharAsString}MacOS{IOPath.UnixDirectorySeparatorCharAsString}steam_osx";

        public string? GetSteamProgramPath() => OSXSteamProgramPath;

        public string GetLastSteamLoginUserName() => string.Empty;

        public string? GetRegistryVdfPath()
        {
            var value = $"{IOPath.UnixDirectorySeparatorCharAsString}Users{IOPath.UnixDirectorySeparatorCharAsString}{Environment.UserName}{IOPath.UnixDirectorySeparatorCharAsString}Library{IOPath.UnixDirectorySeparatorCharAsString}Application Support{IOPath.UnixDirectorySeparatorCharAsString}Steam{IOPath.UnixDirectorySeparatorCharAsString}registry.vdf";
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
                    if (v.Value.HKCU.Software.Valve.Steam.AutoLoginUser != null)
                    {
                        v.Value.HKCU.Software.Valve.Steam.AutoLoginUser = userName;
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

        void IPlatformService.SystemLock(int waitSecond)
        {
            throw new PlatformNotSupportedException();
        }

        async void IPlatformService.SystemShutdown(int waitSecond)
        {
            await Task.Delay(waitSecond);
            RunOsaScript("tell application \"Finder\" to shut down");
        }

        async void IPlatformService.SystemSleep(int waitSecond)
        {
            await Task.Delay(waitSecond);
            RunOsaScript("tell application \"Finder\" to sleep");
        }

        void IPlatformService.SystemHibernate(int waitSecond)
        {
            throw new PlatformNotSupportedException();
        }

        static string RunOsaScript(string shell)
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
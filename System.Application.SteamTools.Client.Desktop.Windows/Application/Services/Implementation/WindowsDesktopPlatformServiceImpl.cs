#pragma warning disable CA1416 // 验证平台兼容性
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System.Application.Models;
using System.Application.UI;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.Versioning;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows")]
    internal sealed partial class WindowsDesktopPlatformServiceImpl : IDesktopPlatformService
    {
        const string TAG = "WindowsDesktopPlatformS";
        const string SteamRegistryPath = @"SOFTWARE\Valve\Steam";

        public string GetCommandLineArgs(Process process)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                      "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
                using var objects = searcher.Get();
                var @object = objects.Cast<ManagementBaseObject>().SingleOrDefault();
                return @object?["CommandLine"]?.ToString() ?? "";
            }
            catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
            {
                // 没有对该进程的安全访问权限。
                return string.Empty;
            }
        }

        static readonly Lazy<string> mHostsFilePath = new(() =>
        {
            return Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");
        });

        public string HostsFilePath => mHostsFilePath.Value;

        static string GetFolderPath(Environment.SpecialFolder folder)
        {
            switch (folder)
            {
                case Environment.SpecialFolder.ProgramFiles:
                    var trimEndMark = " (x86)";
                    var value = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    if (value.EndsWith(trimEndMark, StringComparison.OrdinalIgnoreCase))
                    {
                        var value2 = value.Substring(0, value.Length - trimEndMark.Length);
                        if (Directory.Exists(value2))
                        {
                            return value2;
                        }
                    }
                    return value;
                default:
                    return Environment.GetFolderPath(folder);
            }
        }

        public string? GetFileName(TextReaderProvider provider)
        {
            switch (provider)
            {
                case TextReaderProvider.NotepadPlusPlus:
                    return "notepad++";
                case TextReaderProvider.VSCode:
                    var vsCodePaths = new[] {
                        GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                    }.Distinct().Select(x => Path.Combine(x, "Microsoft VS Code", "Code.exe"));

                    foreach (var vsCodePath in vsCodePaths)
                    {
                        if (File.Exists(vsCodePath))
                        {
                            return vsCodePath;
                        }
                    }

                    return null;
                case TextReaderProvider.Notepad:
                    return "notepad";
                default:
                    return null;
            }
        }

        public void SetBootAutoStart(bool isAutoStart, string name)
        {
            // 开机启动使用 taskschd.msc 实现
            try
            {
                if (isAutoStart)
                {
                    using var td = TaskService.Instance.NewTask();
                    td.RegistrationInfo.Description = name + " System Boot Run";
                    td.Settings.Priority = ProcessPriorityClass.Normal;
                    td.Settings.ExecutionTimeLimit = new TimeSpan(0);
                    td.Settings.AllowHardTerminate = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Triggers.Add(new LogonTrigger());
                    td.Actions.Add(new ExecAction(AppHelper.ProgramName, "-minimized", Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)));
                    if (IsAdministrator)
                        td.Principal.RunLevel = TaskRunLevel.Highest;
                    TaskService.Instance.RootFolder.RegisterTaskDefinition(name, td);
                }
                else
                {
                    TaskService.Instance.RootFolder.DeleteTask(name);
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e,
                    "SetBootAutoStart Fail, isAutoStart: {0}, name: {1}.", isAutoStart, name);
            }
        }

        static string? GetFullPath(string s)
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                return Path.GetFullPath(s);
            }
            return null;
        }

        public string? GetSteamDirPath()
            => GetFullPath(Registry.CurrentUser.Read(SteamRegistryPath, "SteamPath"));

        public string? GetSteamProgramPath()
            => GetFullPath(Registry.CurrentUser.Read(SteamRegistryPath, "SteamExe"));

        public string GetLastSteamLoginUserName()
            => Registry.CurrentUser.Read(SteamRegistryPath, "AutoLoginUser");

        public void SetCurrentUser(string userName)
        {
            Registry.CurrentUser.AddOrUpdate(SteamRegistryPath, "AutoLoginUser", userName, RegistryValueKind.String);
        }

        static string GetMachineSecretKey()
            => Registry.LocalMachine.Read(@"SOFTWARE\Microsoft\Cryptography", "MachineGuid");

        static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey = IDesktopPlatformService.GetMachineSecretKey(GetMachineSecretKey);

        public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;
    }
}
#pragma warning restore CA1416 // 验证平台兼容性
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
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;

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

        /// <summary>
        /// 使用资源管理器打开某个路径
        /// </summary>
        /// <param name="dirPath"></param>
        public void OpenFolder(string dirPath)
        {
            if (File.Exists(dirPath))
            {
                Process.Start("explorer.exe", "/select," + dirPath);
            }
            Process.Start("explorer.exe", dirPath);
        }

        public string? GetFileName(TextReaderProvider provider)
        {
            switch (provider)
            {
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
                var identity = WindowsIdentity.GetCurrent();
                var hasSid = identity.User?.IsAccountSid() ?? false;
                var userId = hasSid ? identity.User!.ToString() : identity.Name;
                var tdName = hasSid ? userId : userId.Replace(Path.DirectorySeparatorChar, '_');
                tdName = $"{name}_{{{tdName}}}";
                if (isAutoStart)
                {
                    using var td = TaskService.Instance.NewTask();
                    td.RegistrationInfo.Description = name + " System Boot Run";
                    td.Settings.Priority = ProcessPriorityClass.Normal;
                    td.Settings.ExecutionTimeLimit = new TimeSpan(0);
                    td.Settings.AllowHardTerminate = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Triggers.Add(new LogonTrigger { UserId = userId });
                    td.Actions.Add(new ExecAction(AppHelper.ProgramName, "-clt c -silence", IOPath.BaseDirectory));
                    if (IsAdministrator)
                        td.Principal.RunLevel = TaskRunLevel.Highest;
                    TaskService.Instance.RootFolder.RegisterTaskDefinition(tdName, td);
                }
                else
                {
                    TaskService.Instance.RootFolder.DeleteTask(name, exceptionOnNotExists: false);
                    TaskService.Instance.RootFolder.DeleteTask(tdName, exceptionOnNotExists: false);
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e,
                    "SetBootAutoStart Fail, isAutoStart: {0}, name: {1}.", isAutoStart, name);
            }
        }

        public void SetSystemSessionEnding(Action action)
        {
            SystemEvents.SessionEnding += (sender, e) =>
            {
                //IDesktopAppService.Instance.CompositeDisposable.Dispose();
                action.Invoke();
            };
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

        public Process StartAsInvoker(string fileName)
        {
            return Process.Start($"/trustlevel:0x20000 \"{fileName}\"");
        }

        public Process? StartAsInvoker(ProcessStartInfo startInfo)
        {
            startInfo.FileName = $"/trustlevel:0x20000 \"{startInfo.FileName}\"";
            return Process.Start(startInfo);
        }

        public Process? GetProcessByPortOccupy(ushort port, bool isTCPorUDP = true)
        {
            try
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    },
                };
                p.Start();
                p.StandardInput.WriteLine($"netstat -ano|findstr \"{port}\"&exit");
                p.StandardInput.AutoFlush = true;
                var reader = p.StandardOutput;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    var lineArray = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (lineArray.Length != 5) continue;
                    if (!lineArray[0].Equals(isTCPorUDP ? "TCP" : "UDP", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!lineArray[3].Equals("LISTENING", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!lineArray[1].EndsWith($":{port}")) continue;
                    if (!ushort.TryParse(lineArray[4], out var pid)) continue;
                    p.Close();
                    return Process.GetProcessById(pid);
                }
                _ = p.WaitForExit(550);
            }
            catch
            {

            }
            return default;
        }
    }
}
#pragma warning restore CA1416 // 验证平台兼容性
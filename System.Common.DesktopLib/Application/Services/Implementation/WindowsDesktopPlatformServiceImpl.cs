#pragma warning disable CA1416 // 验证平台兼容性
#pragma warning disable CA1806 // 不要忽略方法结果
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;

namespace System.Application.Services.Implementation
{
    internal sealed class WindowsDesktopPlatformServiceImpl : DesktopPlatformServiceImpl
    {
        #region 窗口右上角的三个按钮(最小化，最大化，关闭)

        // https://stackoverflow.com/questions/339620/how-do-i-remove-minimize-and-maximize-from-a-resizable-window-in-wpf
        // https://blog.magnusmontin.net/2014/11/30/disabling-or-hiding-the-minimize-maximize-or-close-button-of-a-wpf-window/

        public const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        public const int GWL_STYLE = -16,
                    WS_MAXIMIZEBOX = 0x10000,
                    WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        public extern static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public extern static int SetWindowLong(IntPtr hwnd, int index, int value);

        public const int WS_SYSMENU = 0x80000;

        public static void EnableMinimizeButton(IntPtr hWnd)
        {
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) | WS_MINIMIZEBOX);
        }

        public static void EnableMaximizeButton(IntPtr hWnd)
        {
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) | WS_MAXIMIZEBOX);
        }

        public static void ShowMinimizeAndMaximizeButtons(IntPtr hWnd)
        {
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) | WS_MAXIMIZEBOX | WS_MINIMIZEBOX);
        }

        public static void ShowAllButtons(IntPtr hWnd)
        {
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) | WS_SYSMENU);
        }

        public static void HideMinimizeAndMaximizeButtons(IntPtr hWnd)
        {
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        public static void DisableMinimizeButton(IntPtr hWnd)
        {
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) & ~WS_MINIMIZEBOX);
        }

        public static void DisableMaximizeButton(IntPtr hWnd)
        {
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) & ~WS_MAXIMIZEBOX);
        }

        public override void SetResizeMode(IntPtr hWnd, ResizeMode value)
        {
            switch (value)
            {
                case ResizeMode.NoResize:
                    DeleteMenu(GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                    DeleteMenu(GetSystemMenu(hWnd, false), SC_MINIMIZE, MF_BYCOMMAND);
                    HideMinimizeAndMaximizeButtons(hWnd);
                    break;
                case ResizeMode.CanMinimize:
                    DeleteMenu(GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                    DisableMaximizeButton(hWnd);
                    break;
            }
        }

        #endregion

        #region 硬件信息

        static IEnumerable<string> GetValueByManagementClass_(string managementClassPath, string propertiesName)
        {
            using var managementClass = new ManagementClass(managementClassPath);
            using var instances = managementClass.GetInstances();
            foreach (var instance in instances)
            {
                var value = instance.Properties[propertiesName].Value?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(value)) continue;
                yield return value;
            }
        }

        public static string[] GetValueByManagementClass(string managementClassPath, string propertiesName)
            => new HashSet<string>(GetValueByManagementClass_(managementClassPath, propertiesName)).ToArray();

        public override string[] GetCPUName() => GetValueByManagementClass("Win32_Processor", "Name");

        public override string[] GetGPUName() => GetValueByManagementClass("Win32_VideoController", "Name");

        #endregion

        #region OS

        const int OS_ANYSERVER = 29;

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/bb773795%28v=VS.85%29.aspx?f=255&MSPPError=-2147217396
        /// </summary>
        /// <param name="os"></param>
        /// <returns></returns>
        [DllImport("shlwapi.dll", SetLastError = true, EntryPoint = "#437")]
        static extern bool IsOS(int os);

        static bool IsWindowsServer_() => IsOS(OS_ANYSERVER);

        static readonly Lazy<bool> mIsWindowsServer = new Lazy<bool>(IsWindowsServer_);

        public override bool IsWindowsServer() => mIsWindowsServer.Value;

        static string GetOSVersion_()
        {
            try
            {
                const string subkey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                using var ndpKey = Registry.LocalMachine.OpenSubKey(subkey);
                var productName = ndpKey?.GetValue("ProductName")?.ToString();
                var major = Environment.OSVersion.Version.Major.ToString();
                var minor = Environment.OSVersion.Version.Minor.ToString();
                var build = Environment.OSVersion.Version.Build.ToString();
                var revision = ndpKey?.GetValue("UBR")?.ToString();
                if (string.IsNullOrEmpty(revision))
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                        "kernel32.dll");
                    revision = File.Exists(path)
                        ? FileVersionInfo.GetVersionInfo(path).ProductPrivatePart.ToString()
                        : null;
                }
                var releaseId = ndpKey?.GetValue("ReleaseId")?.ToString();
                if (int.TryParse(releaseId, out var releaseIdInt32) && releaseIdInt32 >= 2009)
                {
                    var displayVersion = ndpKey?.GetValue("DisplayVersion")?.ToString();
                    if (!string.IsNullOrWhiteSpace(displayVersion))
                        releaseId = displayVersion;
                }
                var servicePack = Environment.OSVersion.ServicePack;
                var additional = string.IsNullOrEmpty(servicePack) ? null : " " + servicePack;
                if (releaseId != null)
                    additional += " (" + releaseId + ")";
                return $"{productName} {major}.{minor}.{build}{(string.IsNullOrEmpty(revision) ? null : "." + revision)}{additional}";
            }
            catch (Exception)
            {
                return Environment.OSVersion.VersionString;
            }
        }

        static readonly Lazy<string> mGetOSVersion = new Lazy<string>(GetOSVersion_);

        public override string GetOSVersion() => mGetOSVersion.Value;

        #endregion

        #region Process

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        protected override bool Is64Bit_(Process process)
        {
            // if this method is not available in your version of .NET, use GetNativeSystemInfo via P/Invoke instead
            if (!IsWow64Process(process.Handle, out var isWow64))
                throw new Win32Exception("function fails, the return value is zero. To get extended error information, call GetLastError.");
            return !isWow64;
        }

        #endregion

        #region Administrator

        static readonly Lazy<bool> mIsAdministrator = new Lazy<bool>(IsAdministrator_);

        static bool IsAdministrator_()
            => new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);

        /// <summary>
        /// 是否是管理员权限
        /// </summary>
        /// <returns></returns>
        public override bool IsAdministrator() => mIsAdministrator.Value;

        #endregion

        #region Fluent Design System

        public override void Enable_Fluent_Design_System_Style_Blur(IntPtr hWnd)
        {
            WindowComposition.Set(hWnd, AccentState.ACCENT_ENABLE_BLURBEHIND, AccentFlags.DrawAllBorders);
        }

        internal static class WindowComposition
        {
            [DllImport("user32.dll")]
            internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

            internal static void Set(IntPtr hWnd, AccentState accentState, AccentFlags accentFlags)
            {
                var accent = new AccentPolicy
                {
                    AccentState = accentState,
                    AccentFlags = accentFlags,
                };
                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData
                {
                    Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                    SizeOfData = accentStructSize,
                    Data = accentPtr,
                };
                SetWindowCompositionAttribute(hWnd, ref data);

                Marshal.FreeHGlobal(accentPtr);
            }
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 1,
            ACCENT_ENABLE_GRADIENT = 0,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [Flags]
        internal enum AccentFlags
        {
            DrawLeftBorder = 0x20,
            DrawTopBorder = 0x40,
            DrawRightBorder = 0x80,
            DrawBottomBorder = 0x100,
            DrawTopLeftBorder = (DrawLeftBorder | DrawTopBorder),
            DrawTopRightBorder = (DrawTopBorder | DrawRightBorder),
            DrawBottomLeftBorder = (DrawLeftBorder | DrawBottomBorder),
            DrawBottomRightBorder = (DrawRightBorder | DrawBottomBorder),
            DrawAllBorders = (DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder)
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public AccentFlags AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        internal enum WindowCompositionAttribute
        {
            WCA_UNDEFINED = 0,
            WCA_NCRENDERING_ENABLED = 1,
            WCA_NCRENDERING_POLICY = 2,
            WCA_TRANSITIONS_FORCEDISABLED = 3,
            WCA_ALLOW_NCPAINT = 4,
            WCA_CAPTION_BUTTON_BOUNDS = 5,
            WCA_NONCLIENT_RTL_LAYOUT = 6,
            WCA_FORCE_ICONIC_REPRESENTATION = 7,
            WCA_EXTENDED_FRAME_BOUNDS = 8,
            WCA_HAS_ICONIC_BITMAP = 9,
            WCA_THEME_ATTRIBUTES = 10,
            WCA_NCRENDERING_EXILED = 11,
            WCA_NCADORNMENTINFO = 12,
            WCA_EXCLUDED_FROM_LIVEPREVIEW = 13,
            WCA_VIDEO_OVERLAY_ACTIVE = 14,
            WCA_FORCE_ACTIVEWINDOW_APPEARANCE = 15,
            WCA_DISALLOW_PEEK = 16,
            WCA_CLOAK = 17,
            WCA_CLOAKED = 18,
            WCA_ACCENT_POLICY = 19,
            WCA_FREEZE_REPRESENTATION = 20,
            WCA_EVER_UNCLOAKED = 21,
            WCA_VISUAL_OWNER = 22,
            WCA_LAST = 23
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        #endregion

        #region NETFramework Win32 Registry

        static string GetDescription(int releaseKey) => releaseKey switch
        {
            // https://docs.microsoft.com/zh-cn/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
            378389 => ".NET Framework 4.5",
            378675 => "使用 Windows 8.1 或 Windows Server 2012 R2 安装的 .NET Framework 4.5.1",
            378758 => "安装在 Windows 8、Windows 7 SP1 或 Windows Vista SP2 上的 .NET Framework 4.5.1",
            379893 => ".NET Framework 4.5.2",
            393295 => "随 Windows 10 一起安装的 .NET Framework 4.6",
            393297 => "在所有其他 Windows 操作系统版本上安装的 .NET Framework 4.6",
            394254 => "Windows 10 上安装的 .NET Framework 4.6.1",
            394271 => "在所有其他 Windows 操作系统版本上安装的 .NET Framework 4.6.1",
            394802 => "在 Windows 10 周年更新上安装的 .NET Framework 4.6.2",
            394806 => "在所有其他 Windows 操作系统版本上安装的 .NET Framework 4.6.2",
            460798 => "在 Windows 10 创意者更新上安装的 .NET Framework 4.7",
            460805 => "在所有其他 Windows 操作系统版本上安装的 .NET Framework 4.7",
            461308 => ".NET framework 4.7.1 Windows 10 秋季创意者 Update 上安装",
            461310 => ".NET framework 4.7.1 所有其他 Windows 操作系统版本上安装",
            461808 => "在 Windows 10 2018 年 4 月更新和 Windows Server 版本 1803 上安装的 .NET Framework 4.7.2	",
            461814 => "在除 Windows 10 2018 年 4 月更新和 Windows Server 版本 1803 之外的所有 Windows 操作系统上安装的 .NET Framework 4.7.2",
            528040 => "在 Windows 10 2019 年 5 月更新和 Windows 10 2019 年 11 月更新上安装的 .NET Framework 4.8",
            528372 => "在 Windows 10 2020 年 5 月更新上安装的 .NET Framework 4.8",
            528049 => "在所有其他 Windows 操作系统（包括早期版本的 Windows 10 操作系统）上安装的 .NET Framework 4.8",
            _ => "未知版本" + (releaseKey > 528049 ? "（高于 .NET Framework 4.8）" : ""),
        };

        static readonly Lazy<string> mNETFrameworkDescription = new Lazy<string>(() =>
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";
            using var ndpKey = Registry.LocalMachine.OpenSubKey(subkey);
            var value = ndpKey?.GetValue("Release")?.ToString();
            if (!int.TryParse(value, out var valueInt32)) return string.Empty;
            return GetDescription(valueInt32);
        });

        public override string NETFrameworkDescription => mNETFrameworkDescription.Value;

        #endregion
    }
}
#pragma warning restore CA1416 // 验证平台兼容性
#pragma warning restore CA1806 // 不要忽略方法结果
using System.Diagnostics;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NuGet.Versioning;
using Microsoft.DotNet.Tools.Uninstall.Windows;
using Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo;
using FDELauncher.Properties;
using _ThisAssembly = System.Properties.ThisAssembly;
using R = FDELauncher.Properties.Resources;

namespace FDELauncher;

internal static class Program
{
    const string ExecutiveName = "Steam++";
    static BundleArch matchArch;
    static readonly SemanticVersion runtimeVersion = new(6, 0, 6);
    static readonly SemanticVersion sdkVersion = new(6, 0, 106);

    /// <summary>
    /// 应用程序的主入口点。
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "--query")
            {
                var installed = RegistryQuery.GetAllInstalledBundles().ToArray();
                var text = string.Join(Environment.NewLine, installed.Select(x => $"{x.DisplayName} [{x.Version.Type} {x.Version} {ToString(x.Arch)}]").ToArray());
                MessageBox.Show(text, _ThisAssembly.AssemblyTrademark);
                return 0;
            }
            if (!IsSupportedPlatform(out var error)) return ShowError(error);
            var executivePath = GetExecutivePath();
            if (!File.Exists(executivePath)) return ShowError(R.ExecutiveNotExistsFailure);
            if (!VerificationExecutiveInfo(executivePath)) return ShowError(R.VerificationExecutiveInfoFailure);
            if (IsFrameworkDependentExecutable())
            {
                matchArch = GetArchByPeHeader(executivePath);
                switch (matchArch)
                {
                    case BundleArch.X64:
                        if (!Environment2.Is64BitOperatingSystem) return ShowError(R.ThisAppOnlySupport64BitOS);
                        break;
                    case BundleArch.Arm64:
                        if (RuntimeInformation2.OSArchitecture != Architecture.Arm64) throw new PlatformNotSupportedException();
                        break;
                }
                if (IsRuntimeInstalled(matchArch, runtimeVersion, sdkVersion))
                {
                    Run();
                }
                else
                {
                    ShowRuntimeMissingFailure();
                }
            }
            else
            {
                Run();
            }
            return 0;

            void Run() => StartProcess(executivePath, args);
        }
        catch (Exception ex)
        {
            return ShowError(ex.ToString());
        }
    }

    static int ShowError(string errMsg, int errCode = 0)
    {
        MessageBox.Show(errMsg, _ThisAssembly.AssemblyTrademark, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return errCode;
    }

    static bool IsSupportedPlatform([NotNullWhen(false)] out string? error)
    {
        error = null;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var osVersion = Environment.OSVersion.Version;
            if (osVersion.Major > 6) return true;
            if (osVersion.Major == 6)
            {
                if (osVersion.Minor == 1) // NT 6.1 / Win7 / WinServer 2008 R2
                {
                    if (Environment.OSVersion.ServicePack == "Service Pack 1")
                        return true;
                }
                else if (osVersion.Minor == 2) // NT 6.2 / Win8 / WinServer 2012
                {
                    error = R.NotSupportedWin8PlatformError;
                    return false;
                }
                else if (osVersion.Minor == 3) // NT 6.3 / Win8.1 / WinServer 2012 R2
                {
                    return true;
                }
            }
        }
        error = R.NotSupportedPlatformError;
        return false;
    }

    static string ToString(BundleArch value) => value switch
    {
        BundleArch.X86 => "x86",
        BundleArch.X64 => "x64",
        BundleArch.Arm64 => "Arm64",
        _ => value.ToString(),
    };

    static BundleArch GetArchByPeHeader(string filePath)
    {
        var imageFileHeader = PeHeaderReader.ReadImageFileHeader(filePath);
        var machine = imageFileHeader.Machine;
        return machine switch
        {
            PeHeaderReader.IMAGE_FILE_MACHINE_ARM64 => BundleArch.Arm64,
            PeHeaderReader.IMAGE_FILE_MACHINE_I386 => BundleArch.X86,
            PeHeaderReader.IMAGE_FILE_MACHINE_IA64 or PeHeaderReader.IMAGE_FILE_MACHINE_AMD64 => BundleArch.X64,
            _ => throw new ArgumentOutOfRangeException(nameof(machine), machine, null),
        };
    }

    /// <summary>
    /// 未安装运行时或 SDK 的弹窗提示
    /// </summary>
    static void ShowRuntimeMissingFailure()
    {
        var archStr = ToString(matchArch);
        var runtimeVersionStr = runtimeVersion.ToString();
        var _AspNetCoreRuntime = String2.TryFormat(
                R.AspNetCoreRuntimeFormat2,
                runtimeVersionStr,
                archStr);
        var _NetRuntime = String2.TryFormat(
                R.NetRuntimeFormat2,
                runtimeVersionStr,
                archStr);
        var _Runtime = $"{_AspNetCoreRuntime} {R.And} {_NetRuntime}";
        var text = String2.TryFormat(
            R.RuntimeMissingFailureFormat2,
            _ThisAssembly.AssemblyTrademark,
            _Runtime);
        var result = MessageBox.Show(text, _ThisAssembly.AssemblyTrademark, MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        if (result == DialogResult.Yes)
        {
            const string urlFormat3 = "https://" + "dotnet.microsoft.com/{0}/download/dotnet/{1}.{2}";
            var url = string.Format(urlFormat3, GetLang(), runtimeVersion.Major, runtimeVersion.Minor);
            OpenCoreByProcess(url);
        }
    }

    static string GetLang() => R.GetString(l => l switch
    {
        Language.ChineseSimplified => "zh-cn",
        Language.Japanese => "ja-jp",
        _ => "en-us",
    });

    static bool OpenCoreByProcess(string url)
    {
        return Process2.OpenCoreByProcess(url, OnError);
        static void OnError(Exception e)
        {
            string text;
            if (e is Win32Exception win32Ex)
            {
                text = String2.TryFormat(R.OpenCoreByProcessOnExceptionFormat1, $" 0x{Convert.ToString(win32Ex.NativeErrorCode, 16)}");
            }
            else
            {
                text = String2.TryFormat(R.OpenCoreByProcessOnExceptionFormat1, string.Empty) + Environment.NewLine + e.ToString();
            }
            MessageBox.Show(text, _ThisAssembly.AssemblyTrademark, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 是否已安装运行时或 SDK
    /// </summary>
    /// <returns></returns>
    static bool IsRuntimeInstalled(BundleArch matchArch, SemanticVersion runtimeVersion, SemanticVersion sdkVersion)
    {
        // ArchiSteamFarm 依赖 ASP.NET Core
        // ASP.NET Core 运行时安装程序除 Hosting Bundle 不包含 .NET 运行时 缺少文件 C:\Program Files\dotnet\host\fxr\{version}\hostfxr.dll
        // 缺少 hostfxr.dll 会提示 To run this application, you must install .NET Desktop Runtime
        // https://github.com/dotnet/runtime/blob/v6.0.5/src/native/corehost/apphost/apphost.windows.cpp#L62-L64
        // https://github.com/dotnet/runtime/blob/v6.0.5/src/native/corehost/apphost/apphost.windows.cpp#L106-L113
        var installed = RegistryQuery.GetAllInstalledBundles().Where(m => m.Arch.HasFlags(matchArch)).ToArray();

        var query_sdk = installed.Where(m => m.Version.Type == BundleType.Sdk && m.Version.SemVer >= sdkVersion);
        if (query_sdk.Any()) return true;

        var query_runtime = installed.Where(m => (m.Version.Type == BundleType.Runtime || m.Version.Type == BundleType.WindowsDesktopRuntime) && m.Version.SemVer >= runtimeVersion);
        if (!query_runtime.Any()) return false;

        var query_aspnetruntime = installed.Where(m => m.Version.Type == BundleType.AspNetRuntime && m.Version.SemVer >= runtimeVersion);
        if (!query_aspnetruntime.Any()) return false;

        return true;
    }

    /// <summary>
    /// 获取主程序执行文件路径
    /// </summary>
    /// <returns></returns>
    static string GetExecutivePath()
    {
        const string ExecutiveNameWithExtension = $"{ExecutiveName}.exe";
        var executivePath = Path.Combine(Application.StartupPath, ExecutiveNameWithExtension);
        return executivePath;
    }

    /// <summary>
    /// 验证执行文件信息
    /// </summary>
    /// <param name="executivePath"></param>
    /// <returns></returns>
    static bool VerificationExecutiveInfo(string executivePath)
    {
        const string OLD_AssemblyProduct = "SteamTools";
        const string OLD_AssemblyTrademark = "Steam++";
        const string OLD_AssemblyDescription = "「Steam++ 工具箱」是一个开源跨平台的多功能游戏工具箱。";
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(executivePath);
        var result = (fileVersionInfo.Comments == _ThisAssembly.AssemblyDescription || fileVersionInfo.Comments == OLD_AssemblyDescription) &&
                    fileVersionInfo.CompanyName == _ThisAssembly.AssemblyCompany &&
                    (fileVersionInfo.FileDescription == _ThisAssembly.AssemblyTrademark || fileVersionInfo.FileDescription == OLD_AssemblyTrademark) &&
                    (fileVersionInfo.LegalTrademarks == _ThisAssembly.AssemblyTrademark || fileVersionInfo.LegalTrademarks == OLD_AssemblyTrademark) &&
                    fileVersionInfo.LegalCopyright == _ThisAssembly.AssemblyCopyright &&
                    (fileVersionInfo.ProductName == _ThisAssembly.AssemblyProduct || fileVersionInfo.ProductName == OLD_AssemblyProduct);
        return result;
    }

    static bool IsFrameworkDependentExecutable()
    {
        return true;
        //var dllPath = new[] {
        //    Application.StartupPath,
        //    "Bin",
        //    "coreclr.dll",
        //}.Aggregate(Path.Combine);
        //return !File.Exists(dllPath);
    }

    static void StartProcess(string executivePath, string[] args)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = executivePath,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
        });
    }
}
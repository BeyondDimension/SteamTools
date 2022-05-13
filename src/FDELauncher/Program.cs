using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Forms;
using NuGet.Versioning;
using Microsoft.DotNet.Tools.Uninstall.Windows;
using Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo;
using _ThisAssembly = System.Properties.ThisAssembly;
using R = FDELauncher.Properties.Resources;
using FDELauncher.Properties;

namespace FDELauncher;

internal static class Program
{
    const string ExecutiveName = "Steam++";
    static BundleArch matchArch;
    static readonly SemanticVersion runtimeVersion = new(6, 0, 5);
    static readonly SemanticVersion sdkVersion = new(6, 0, 105);

    /// <summary>
    /// 应用程序的主入口点。
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            var executivePath = GetExecutivePath();
            if (File.Exists(executivePath))
            {
                if (VerificationExecutiveInfo(executivePath))
                {
                    if (IsFrameworkDependentExecutable())
                    {
                        matchArch = GetArchByPeHeader(executivePath);
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

                    void Run() => StartProcess(executivePath, args);
                }
                else
                {
                    MessageBox.Show(R.VerificationExecutiveInfoFailure, _ThisAssembly.AssemblyTrademark, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(R.ExecutiveNotExistsFailure, _ThisAssembly.AssemblyTrademark, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), _ThisAssembly.AssemblyTrademark, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
        var text = String2.TryFormat(R.RuntimeMissingFailureFormat2,
            _ThisAssembly.AssemblyTrademark,
            String2.TryFormat(R.AspNetCoreRuntimeFormat2,
                runtimeVersion.ToString(),
                ToString(matchArch)));
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
        var installed = RegistryQuery.GetAllInstalledBundles();
        var query = from m in installed
                    where m.Arch.HasFlags(matchArch) &&
                    ((m.Version.Type == BundleType.AspNetRuntime && m.Version.SemVer >= runtimeVersion) ||
                    (m.Version.Type == BundleType.Sdk && m.Version.SemVer >= sdkVersion))
                    select m;
        return query.Any();
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
        var dllPath = new[] {
            Application.StartupPath,
            "Bin",
            "coreclr.dll",
        }.Aggregate(Path.Combine);
        return !File.Exists(dllPath);
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
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
using System.Application;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using static System.Application.Utils;
using static System.ProjectPathUtil;

namespace System;

public static class DigitalSignUtil
{
    const string pfx_filename = "steampp.net_code_sign.pfx";
    const string timestamp_url = "http://timestamp.digicert.com";

    /// <summary>
    /// .dll .exe
    /// </summary>
    public static readonly string[] bin_extensions = new[] { ".dll", ".exe" };

    /// <summary>
    /// https://docs.microsoft.com/zh-cn/dotnet/framework/tools/signtool-exe
    /// </summary>
    public static readonly string signtool_exe = SearchByWindowsKits("signtool");

    /// <summary>
    /// 文件是否经过了数字签名
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool IsDigitalSigned(string filePath)
    {
        //var runspaceConfiguration = RunspaceConfiguration.Create();
        //using var runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
        //runspace.Open();
        //using var pipeline = runspace.CreatePipeline();
        //pipeline.Commands.AddScript("Get-AuthenticodeSignature \"" + filePath + "\"");
        //var results = pipeline.Invoke();
        //runspace.Close();
        //var signature = results[0].BaseObject as Signature;
        // https://github.com/PowerShell/PowerShell/blob/v7.2.4/src/Microsoft.PowerShell.Security/security/SignatureCommands.cs#L269-L282
        var signature = SignatureHelper.GetSignature(filePath, null);
        return signature != null && signature.SignerCertificate != null && (signature.Status != SignatureStatus.NotSigned);
    }

    static void GetAllFiles(string path, List<string> list, params string[] extensions)
    {
        Console.WriteLine(path);

        var files = Directory.GetFiles(path);

        foreach (var item in files)
        {
            var ex = Path.GetExtension(item);
            if (extensions.Contains(ex, StringComparer.OrdinalIgnoreCase))
            {
                list.Add(item);
            }
        }

        var dirs = Directory.GetDirectories(path);
        foreach (var item in dirs)
        {
            GetAllFiles(item, list, extensions);
        }
    }

    /// <summary>
    /// 根据路径去递归获取所有文件，传入扩展名进行过滤
    /// </summary>
    /// <param name="path"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static List<string> GetAllFiles(string path, params string[] extensions)
    {
        var list = new List<string>();
        GetAllFiles(path, list, extensions);
        return list;
    }

    static string SearchByWindowsKits(string name)
    {
        var exeFileName = $"{name}.exe";
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        path = Path.Combine(path, "Windows Kits", "10", "bin");
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists) throw new DirectoryNotFoundException(path);
        var arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
        var query = from m in dirInfo.GetDirectories()
                    let v = TryParse(m.Name)
                    where v != null
                    orderby v descending
                    select KeyValuePair.Create(v, m);
        static Version? TryParse(string? input) => Version.TryParse(input, out var version) ? version : null;
        Dictionary<Version, DirectoryInfo> dict = new(query);
        if (dict.Count == 0) throw new FileNotFoundException(exeFileName);
        foreach (var item in dict)
        {
            path = Path.Combine(item.Value.FullName, arch, exeFileName);
            if (File.Exists(path)) return path;
        }
        throw new DirectoryNotFoundException(path);
    }

    /// <summary>
    /// 程序集数字签名
    /// </summary>
    /// <param name="pwd">证书密码</param>
    /// <param name="path">程序集所在路径</param>
    /// <param name="extensions">扩展名过滤</param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void Signature(string pwd, string path, params string[] extensions)
    {
        var allFiles = GetAllFiles(path, extensions.Any_Nullable() ? extensions : bin_extensions);
        Signature(pwd, allFiles);
    }

    public static void Signature(string pwd, IEnumerable<string> allFiles)
    {
        var pfxFileName = Path.Combine(projPath, pfx_filename);
        if (!File.Exists(pfxFileName)) throw new FileNotFoundException(pfxFileName);

        const string value = $"sign /v /f \"{{0}}\" /p \"{{1}}\" /tr {timestamp_url} ";

        var s = new StringBuilder();

        s.AppendFormat(value, pfxFileName, pwd);

        var hasValue = false;

        foreach (var item in allFiles)
        {
            if (!IsDigitalSigned(item))
            {
                hasValue = true;
                s.AppendFormat("\"{0}\"", item);
                s.Append(" ");
            }
        }

        if (!hasValue)
        {
            Console.WriteLine("无需要签名的文件");
            return;
        }

        var signtool_arguments = s.ToString().TrimEnd();

        Process? p = null;
        try
        {
            p = new Process();
            p.StartInfo.FileName = signtool_exe;
            p.StartInfo.Arguments = signtool_arguments;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            while (true)
            {
                var line = p.StandardOutput.ReadLine();
                if (line == null) break;
                Console.WriteLine(line);
            }
            p.WaitForExit();
        }
        finally
        {
            p?.Dispose();
        }
    }

    public static void Handler(string pwd, bool dev, DeploymentMode d, bool endWriteOK = true)
    {
        var configuration = GetConfiguration(dev, isLower: false);
        var pubPath = d switch
        {
            DeploymentMode.SCD => projPath + string.Format(DirPublishWinX64_, configuration),
            DeploymentMode.FDE => projPath + string.Format(DirPublishWinX64_FDE_, configuration),
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null),
        };

        try
        {
            Signature(pwd, pubPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}

#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// 项目工具类
/// </summary>
[Mobius2(
"""
Properties\ProjectUtils
src\ProjectUtils.cs
""")]
public static partial class ProjectUtils
{
    /// <summary>
    /// 当前项目绝对路径
    /// </summary>
    public static readonly string ProjPath;

    static ProjectUtils()
    {
        ProjPath = GetProjectPath();
        // https://docs.github.com/en/actions/learn-github-actions/variables#default-environment-variables
        isCI = bool.TryParse(Environment.GetEnvironmentVariable("CI"), out var result) && result;
    }

    /// <summary>
    /// 获取当前项目绝对路径(.sln文件所在目录)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetProjectPath(string? path = null)
    {
        path ??=
#if NET46_OR_GREATER || NETCOREAPP
        AppContext.BaseDirectory;
#else
        AppDomain.CurrentDomain.BaseDirectory;
#endif
        try
        {
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1003 // Symbols should be spaced correctly
            if (!
#pragma warning restore SA1003 // Symbols should be spaced correctly
#pragma warning restore IDE0079 // 请删除不必要的忽略
#if NET35
            Directory.GetFiles
#else
                Directory.EnumerateFiles
#endif
                (path, "*.sln").Any())
            {
                var parent = Directory.GetParent(path);
                if (parent == null)
                    return string.Empty;
                return GetProjectPath(parent.FullName);
            }
        }
        catch
        {
            return string.Empty;
        }
        return path;
    }

#if !APP_HOST
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    /// <summary>
    /// 当前目标框架 TFM
    /// </summary>
    public static readonly string tfm =
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
        $"net{Environment.Version.Major}.{Environment.Version.Minor}{tfm_}";

    /// <summary>
    /// 当前目标框架 TFM 后缀
    /// </summary>
    public const string tfm_ =
#if WINDOWS
    "-windows10.0.19041.0";
#elif LINUX
    "";
#elif MACCATALYST
    "-maccatalyst";
#elif MACOS
    "-macos";
#else
    "";
#endif
#endif

    static readonly bool isCI;

    public static bool IsCI() => isCI;
}

[AttributeUsage(AttributeTargets.All, Inherited = false)]
sealed class Mobius2Attribute : Attribute
{
    public Mobius2Attribute()
    {

    }

    public Mobius2Attribute(string str)
    {

    }

    public bool Obsolete { get; set; }
}

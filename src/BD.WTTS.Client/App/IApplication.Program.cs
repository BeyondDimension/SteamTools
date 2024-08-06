// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
#if DEBUG
    private static string? _ProgramPath;

    /// <summary>
    /// 当前主程序所在绝对路径
    /// </summary>
    [Obsolete("use var processPath = Environment.ProcessPath; processPath.ThrowIsNull()", true)]
    static string ProgramPath
    {
        get
        {
            _ProgramPath ??= Environment.ProcessPath ?? "";
            return _ProgramPath;
        }
    }
#endif

    [Mobius(
"""
AppConstants.ProgramName
""")]
    private static string? _ProgramName;

    /// <summary>
    /// 获取当前主程序文件名，例如 steam++.exe
    /// </summary>
    [Mobius(
"""
AppConstants.ProgramName
""")]
    static string ProgramName
    {
        get
        {
            _ProgramName ??= GetProgramName();
            return _ProgramName;
        }
    }

    [Mobius(
"""
AppConstants.ProgramName
""")]
    private static string GetProgramName()
    {
        var processPath = Environment.ProcessPath;
        var programName = Path.GetFileName(processPath.ThrowIsNull());
        return programName.ThrowIsNull();
    }
}
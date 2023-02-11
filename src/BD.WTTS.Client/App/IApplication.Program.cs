// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    private static string? _ProgramPath;

    /// <summary>
    /// 当前主程序所在绝对路径
    /// </summary>
    static string ProgramPath
    {
        get
        {
            _ProgramPath ??= Environment.ProcessPath ?? "";
            return _ProgramPath;
        }
    }

    private static string? _ProgramName;

    /// <summary>
    /// 获取当前主程序文件名，例如 steam++.exe
    /// </summary>
    static string ProgramName
    {
        get
        {
            _ProgramName ??= Path.GetFileName(ProgramPath) ?? "";
            return _ProgramName;
        }
    }
}
static partial class Utils
{
    public static readonly string ProjPath;

    static Utils()
    {
        ProjPath = GetProjectPath();
    }

    /// <summary>
    /// 获取当前项目绝对路径(.sln文件所在目录)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static string GetProjectPath(string? path = null)
    {
        path ??=
#if NET46_OR_GREATER || NETCOREAPP
        AppContext.BaseDirectory;
#else
        AppDomain.CurrentDomain.BaseDirectory;
#endif
        if (!Directory.EnumerateFiles(path, "*.sln").Any())
        {
            var parent = Directory.GetParent(path);
            if (parent == null) return string.Empty;
            return GetProjectPath(parent.FullName);
        }
        return path;
    }
}
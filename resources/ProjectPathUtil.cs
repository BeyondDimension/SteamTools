using System.IO;
using System.Linq;

namespace System
{
    public static class ProjectPathUtil
    {
        public static readonly string projPath;

        public const string ProjectDir_AvaloniaApp = "ST.Client.Desktop.Avalonia.App";

        public const string ProjectDir_CoreLib = "Common.CoreLib";

        public const string ProjectDir_ClientDesktop = "ST.Client.Desktop";

        public const string DirPublishWinX86 =
            @"\" + ProjectDir_AvaloniaApp + @"\bin\Release\Publish\win-x86";

        public const string DirPublish =
            @"\" + ProjectDir_AvaloniaApp + @"\bin\Release\Publish";

        static ProjectPathUtil()
        {
            projPath = GetProjectPath();
            if (!Directory.Exists(Path.Combine(projPath, ProjectDir_AvaloniaApp)))
            {
                projPath = GetProjectPathByServerTool();
            }
        }

        /// <summary>
        /// 获取当前项目绝对路径(.sln文件所在目录)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static string GetProjectPath(string? path = null)
        {
            path ??= AppContext.BaseDirectory;
            if (!Directory.GetFiles(path, "*.sln").Any())
            {
                var parent = Directory.GetParent(path);
                if (parent == null) return string.Empty;
                return GetProjectPath(parent.FullName);
            }
            return path;
        }

        /// <summary>
        /// 获取公开客户端项目绝对路径，仅供服务端工具项目中使用
        /// </summary>
        /// <returns></returns>
        static string GetProjectPathByServerTool()
        {
            var projPath_ = GetProjectPath();
            return Path.GetFullPath(Path.Combine(projPath_, "..", "SteamTools"));
        }
    }
}
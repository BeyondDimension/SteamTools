using System;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS;

public static partial class PathHelper
{
    public static bool RecursiveDelete(string baseDir, bool keepFolders, bool throwOnError = false) =>
    RecursiveDelete(new DirectoryInfo(baseDir), keepFolders, throwOnError);

    public static bool RecursiveDelete(DirectoryInfo baseDir, bool keepFolders, bool throwOnError = false)
    {
        if (!baseDir.Exists)
            return true;

        try
        {
            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir, keepFolders);
            }
            var files = baseDir.GetFiles();
            foreach (var file in files)
            {
                if (!file.Exists) continue;
                file.IsReadOnly = false;
                try
                {
                    file.Delete();
                }
                catch (Exception e)
                {
                    if (throwOnError)
                    {
                        Log.Error(nameof(PathHelper), e, "Recursive Delete could not delete file.");
                    }
                }
            }

            if (keepFolders) return true;
            try
            {
                baseDir.Delete();
            }
            catch (Exception e)
            {
                if (throwOnError)
                {
                    Log.Error(nameof(PathHelper), e, "Recursive Delete could not delete folder.");
                }
            }
            return true;
        }
        catch (UnauthorizedAccessException e)
        {
            Log.Error(nameof(PathHelper), e, "RecursiveDelete failed");
            return false;
        }
    }

    /// <summary>
    /// 处理复制的文件或文件夹
    /// </summary>
    /// <param name="fromPath"></param>
    /// <param name="toPath"></param>
    /// <param name="localCachePath"></param>
    /// <param name="reverse">FALSE: Platform -> LoginCache. TRUE: LoginCache -> J••Platform</param>
    public static bool HandleFileOrFolder(string fromPath, string toPath, string localCachePath, bool reverse, string? folderPath = null)
    {
        // Expand, or join localCachePath
        var toFullPath = toPath.Contains('%')
            ? IOPath.ExpandEnvironmentVariables(toPath, folderPath)
            : Path.Combine(localCachePath, toPath);

        // Reverse if necessary. Explained in summary above.
        if (reverse && fromPath.Contains('*'))
        {
            (toPath, fromPath) = (fromPath, toPath); // Reverse
            var wildcard = Path.GetFileName(toPath);
            // Expand, or join localCachePath
            fromPath = fromPath.Contains('%')
                ? IOPath.ExpandEnvironmentVariables(Path.Combine(fromPath, wildcard), folderPath)
                : Path.Combine(localCachePath, fromPath, wildcard);
            toPath = toPath.Replace(wildcard, "");
            toFullPath = IOPath.ExpandEnvironmentVariables(toPath);
        }

        var platformService = IPlatformService.Instance;

        // Handle wildcards
        if (fromPath.Contains('*'))
        {
            var folder = IOPath.ExpandEnvironmentVariables(Path.GetDirectoryName(fromPath) ?? "", folderPath);
            var file = Path.GetFileName(fromPath);

            // Handle "...\\*" folder.
            if (file == "*")
            {
                if (!Directory.Exists(folder))
                    return false;
                if (platformService.CopyFilesRecursive(folder, toFullPath))
                    return true;

                Toast.Show(ToastIcon.Error, AppResources.Error_CopyFileFailed);
                return false;
            }

            // Handle "...\\*.log" or "...\\file_*", etc.
            // This is NOT recursive - Specify folders manually in JSON
            _ = Directory.CreateDirectory(folder);
            foreach (var f in Directory.GetFiles(folder, file))
            {
                if (toFullPath == null)
                    return false;
                if (toFullPath.Contains('*')) toFullPath = Path.GetDirectoryName(toFullPath);
                var fullOutputPath = Path.Combine(toFullPath!, Path.GetFileName(f));
                platformService.CopyFile(f, fullOutputPath);
            }

            return true;
        }

        if (reverse)
            (fromPath, toFullPath) = (toFullPath, fromPath);

        var fullPath = IOPath.ExpandEnvironmentVariables(fromPath, folderPath);
        toFullPath = IOPath.ExpandEnvironmentVariables(toFullPath, folderPath);

        // Is folder? Recursive copy folder
        if (Directory.Exists(fullPath))
        {
            _ = Directory.CreateDirectory(toFullPath);
            if (platformService.CopyFilesRecursive(fullPath, toFullPath))
                return true;

            Toast.Show(ToastIcon.Error, AppResources.Error_CopyFileFailed);
            return false;
        }

        // Is file? Copy file
        if (!File.Exists(fullPath)) return false;
        _ = Directory.CreateDirectory(Path.GetDirectoryName(toFullPath) ?? string.Empty);
        var dest = Path.Combine(Path.GetDirectoryName(toFullPath)!, Path.GetFileName(fullPath));
        platformService.CopyFile(fullPath, dest);
        return true;

    }
}

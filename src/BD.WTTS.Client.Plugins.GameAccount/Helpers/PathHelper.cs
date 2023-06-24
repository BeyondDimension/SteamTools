namespace BD.WTTS;

public static class PathHelper
{
    /// <summary>
    /// 删除路径中的非法字符
    /// </summary>
    public static string? CleanPathIlegalCharacter(string? f)
    {
        if (string.IsNullOrEmpty(f))
            return f;
        var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        var r = new Regex($"[{Regex.Escape(regexSearch)}]");
        return r.Replace(f, "");
    }

    public static string ExpandEnvironmentVariables(string? path, bool noIncludeBasicCheck = false)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var variables = new Dictionary<string, string>
            {
                //{ "%SPP_UserData%", IOPath.CacheDirectory },
                { "%SPP_AppData%", IOPath.AppDataDirectory },
                { "%Documents%", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) },
                { "%Music%", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) },
                { "%Pictures%", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) },
                { "%Videos%", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) },
                { "%StartMenu%", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) },
                { "%StartMenuProgramData%", Environment.ExpandEnvironmentVariables(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs")) },
                { "%StartMenuAppData%", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs") }
            };

        foreach (var (k, v) in variables)
            path = path.Replace(k, v);

        if (!noIncludeBasicCheck)
            path = path.Replace("%Platform_Folder%", "");

        return Environment.ExpandEnvironmentVariables(path);
    }

    public static string RegexSearchFile(string file, string pattern)
    {
        var m = Regex.Match(File.ReadAllText(file), pattern);
        return m.Success ? m.Value : "";
    }

    public static string RegexSearchFolder(string folder, string pattern, string wildcard = "")
    {
        // Foreach file in folder (until match):
        foreach (var f in Directory.GetFiles(folder, wildcard))
        {
            var result = RegexSearchFile(f, pattern);
            if (result == "") continue;
            return result;
        }

        return "";
    }

    public static bool CopyFile(string source, string dest, bool overwrite = true)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(dest))
        {
            Log.Error(nameof(CopyFile), "Failed to copy file! Either path is empty or invalid! From: " + source + ", To: " + dest);
            return false;
        }

        if (!File.Exists(source)) return false;
        if (File.Exists(dest) && !overwrite) return false;

        // Try copy the file normally - This will fail if in use
        var dirName = Path.GetDirectoryName(dest);
        if (!string.IsNullOrWhiteSpace(dirName)) // This could be a file in the working directory, instead of a file in a folder -> No need to create folder if exists.
            Directory.CreateDirectory(dirName);

        try
        {
            File.Copy(source, dest, overwrite);
            return true;
        }
        catch (Exception e)
        {
            // Try another method to copy.
            if (e.HResult == -2147024864) // File in use
            {
                try
                {
                    if (File.Exists(dest)) File.Delete(dest);
                    using var inputFile = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var outputFile = new FileStream(dest, FileMode.Create);
                    var buffer = new byte[0x10000];
                    int bytes;

                    while ((bytes = inputFile.Read(buffer, 0, buffer.Length)) > 0)
                        outputFile.Write(buffer, 0, bytes);
                    return true;
                }
                catch (Exception exception)
                {
                    Log.Error(nameof(CopyFile), "Failed to copy file! From: " + source + ", To: " + dest, exception);
                }
            }
        }
        return false;
    }

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
    /// Recursively copy files and directories
    /// </summary>
    /// <param name="inputFolder">Folder to copy files recursively from</param>
    /// <param name="outputFolder">Destination folder</param>
    /// <param name="overwrite">Whether to overwrite files or not</param>
    /// <param name="throwOnError">When false, error is only logged (default)</param>
    public static bool CopyFilesRecursive(string? inputFolder, string outputFolder, bool overwrite = true, bool throwOnError = false)
    {
        try
        {
            if (string.IsNullOrEmpty(inputFolder)) return false;

            _ = Directory.CreateDirectory(outputFolder);
            outputFolder = outputFolder.EndsWith("\\") ? outputFolder : outputFolder + "\\";
            //Now Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(inputFolder, "*", SearchOption.AllDirectories))
                _ = Directory.CreateDirectory(dirPath.Replace(inputFolder, outputFolder));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(inputFolder, "*.*", SearchOption.AllDirectories))
            {
                var dest = newPath.Replace(inputFolder, outputFolder);
                if (!overwrite && File.Exists(dest)) continue;

                File.Copy(newPath, dest, true);
            }
        }
        catch (Exception e)
        {
            Log.Error(nameof(CopyFilesRecursive), e, $"Failed to CopyFilesRecursive: {inputFolder} -> {outputFolder} (Overwrite {overwrite})");
            if (throwOnError) throw;
            return false;
        }

        return true;
    }

    /// <summary>
    /// 处理复制的文件或文件夹
    /// </summary>
    /// <param name="fromPath"></param>
    /// <param name="toPath"></param>
    /// <param name="localCachePath"></param>
    /// <param name="reverse">FALSE: Platform -> LoginCache. TRUE: LoginCache -> J••Platform</param>
    public static bool HandleFileOrFolder(string fromPath, string toPath, string localCachePath, bool reverse)
    {
        // Expand, or join localCachePath
        var toFullPath = toPath.Contains('%')
            ? ExpandEnvironmentVariables(toPath)
            : Path.Combine(localCachePath, toPath);

        // Reverse if necessary. Explained in summary above.
        if (reverse && fromPath.Contains('*'))
        {
            (toPath, fromPath) = (fromPath, toPath); // Reverse
            var wildcard = Path.GetFileName(toPath);
            // Expand, or join localCachePath
            fromPath = fromPath.Contains('%')
                ? ExpandEnvironmentVariables(Path.Combine(fromPath, wildcard))
                : Path.Combine(localCachePath, fromPath, wildcard);
            toPath = toPath.Replace(wildcard, "");
            toFullPath = toPath;
        }

        // Handle wildcards
        if (fromPath.Contains('*'))
        {
            var folder = ExpandEnvironmentVariables(Path.GetDirectoryName(fromPath) ?? "");
            var file = Path.GetFileName(fromPath);

            // Handle "...\\*" folder.
            if (file == "*")
            {
                if (!Directory.Exists(Path.GetDirectoryName(fromPath))) return false;
                if (CopyFilesRecursive(Path.GetDirectoryName(fromPath), toFullPath)) return true;

                Toast.Show("复制文件失败！");
                return false;
            }

            // Handle "...\\*.log" or "...\\file_*", etc.
            // This is NOT recursive - Specify folders manually in JSON
            _ = Directory.CreateDirectory(folder);
            foreach (var f in Directory.GetFiles(folder, file))
            {
                if (toFullPath == null) return false;
                if (toFullPath.Contains('*')) toFullPath = Path.GetDirectoryName(toFullPath);
                var fullOutputPath = Path.Combine(toFullPath!, Path.GetFileName(f));
                CopyFile(f, fullOutputPath);
            }

            return true;
        }

        if (reverse)
            (fromPath, toFullPath) = (toFullPath, fromPath);

        var fullPath = ExpandEnvironmentVariables(fromPath);
        toFullPath = ExpandEnvironmentVariables(toFullPath);

        // Is folder? Recursive copy folder
        if (Directory.Exists(fullPath))
        {
            _ = Directory.CreateDirectory(toFullPath);
            if (CopyFilesRecursive(fullPath, toFullPath)) return true;

            Toast.Show("复制文件失败！");
            return false;
        }

        // Is file? Copy file
        if (!File.Exists(fullPath)) return false;
        _ = Directory.CreateDirectory(Path.GetDirectoryName(toFullPath) ?? string.Empty);
        var dest = Path.Combine(Path.GetDirectoryName(toFullPath)!, Path.GetFileName(fullPath));
        CopyFile(fullPath, dest);
        return true;

    }

    public static bool IsDirectoryEmpty(string? path)
    {
        if (string.IsNullOrEmpty(path)) return true;
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    /// <summary>
    /// A replacement for File.ReadAllText() that doesn't crash if a file is in use.
    /// </summary>
    /// <param name="f">File to be read</param>
    /// <returns>string of content</returns>
    public static string ReadAllText(string f)
    {
        using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var tr = new StreamReader(fs);
        return tr.ReadToEnd();
    }

    public static bool DeleteFile(string path, bool throwErr = false)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(nameof(DeleteFile), $"Could not delete ({Marshal.GetExceptionForHR(e.HResult)?.Message}): {path}");
            if (throwErr)
                throw;
            return false;
        }
    }
}

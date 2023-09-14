// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    void IPCPlatformService.CopyFile(string source, string dest, bool overwrite)
    {
        try
        {
            // 先尝试直接执行
            IOPath.CopyFile(source, dest, overwrite);
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    ipcPlatformService.CopyFile(source, dest, overwrite);
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to CopyFile l26: {source} -> {dest} (Overwrite {overwrite})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                    $"Failed to CopyFile l32: {source} -> {dest} (Overwrite {overwrite})");
            }
        }
    }

    bool IPCPlatformService.CopyFilesRecursive(string? inputFolder, string outputFolder, bool overwrite)
    {
        try
        {
            // 先尝试直接执行
            IOPath.CopyFilesRecursive(inputFolder, outputFolder, overwrite);
            return true;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    ipcPlatformService.CopyFilesRecursive(inputFolder, outputFolder, overwrite);
                    return true;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to CopyFilesRecursive l57: {inputFolder} -> {outputFolder} (Overwrite {overwrite})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                    $"Failed to CopyFilesRecursive l63: {inputFolder} -> {outputFolder} (Overwrite {overwrite})");
            }
        }

        return false;
    }

    string IPCPlatformService.ReadAllText(string path, int? encoding)
    {
        try
        {
            // 先尝试直接执行
            Encoding? encoding_ = encoding.HasValue ? Encoding.GetEncoding(encoding.Value) : null;
            var result = IOPath.ReadAllText(path, encoding_);
            return result;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = ipcPlatformService.ReadAllText(path, encoding);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to ReadAllText l90: ({path}, {encoding})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to ReadAllText l96: ({path}, {encoding})");
            }
        }
        return string.Empty;
    }

    async Task<string> IPCPlatformService.ReadAllTextAsync(string path, int? encoding, CancellationToken cancellationToken)
    {
        try
        {
            // 先尝试直接执行
            Encoding? encoding_ = encoding.HasValue ? Encoding.GetEncoding(encoding.Value) : null;
            var result = await IOPath.ReadAllTextAsync(path, encoding_, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = await ipcPlatformService.ReadAllTextAsync(path, encoding, cancellationToken);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to ReadAllTextAsync l126: ({path}, {encoding})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to ReadAllTextAsync l132: ({path}, {encoding})");
            }
        }
        return string.Empty;
    }

    bool IPCPlatformService.FileTryDelete(string filePath)
    {
        try
        {
            // 先尝试直接执行
            var result = IOPath.FileTryDelete(filePath);
            return result;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = ipcPlatformService.FileTryDelete(filePath);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to FileTryDelete l160: ({filePath})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to FileTryDelete l166: ({filePath})");
            }
        }
        return default;
    }

    #region WriteAll

    bool IPCPlatformService.WriteAllText(string path, string? contents, int? encoding)
    {
        try
        {
            // 先尝试直接执行
            if (encoding.HasValue)
            {
                var encoding_ = Encoding.GetEncoding(encoding.Value);
                File.WriteAllText(path, contents, encoding_);
            }
            else
            {
                File.WriteAllText(path, contents);
            }
            return true;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = ipcPlatformService.WriteAllText(path, contents, encoding);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to WriteAllText l204: ({path}, *, {encoding})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to WriteAllText l210: ({path}, *, {encoding})");
            }
        }
        return default;
    }

    async Task<bool> IPCPlatformService.WriteAllTextAsync(string path, string? contents, int? encoding, CancellationToken cancellationToken)
    {
        try
        {
            // 先尝试直接执行
            if (encoding.HasValue)
            {
                var encoding_ = Encoding.GetEncoding(encoding.Value);
                await File.WriteAllTextAsync(path, contents, encoding_, cancellationToken);
            }
            else
            {
                await File.WriteAllTextAsync(path, contents, cancellationToken);
            }
            return true;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = await ipcPlatformService.WriteAllTextAsync(path, contents, encoding, cancellationToken);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to WriteAllTextAsync l246: ({path}, *, {encoding})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to WriteAllTextAsync l252: ({path}, *, {encoding})");
            }
        }
        return default;
    }

    bool IPCPlatformService.WriteAllLines(string path, IEnumerable<string> contents, int? encoding)
    {
        try
        {
            // 先尝试直接执行
            if (encoding.HasValue)
            {
                var encoding_ = Encoding.GetEncoding(encoding.Value);
                File.WriteAllLines(path, contents, encoding_);
            }
            else
            {
                File.WriteAllLines(path, contents);
            }
            return true;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = ipcPlatformService.WriteAllLines(path, contents, encoding);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to WriteAllLines l288: ({path}, *, {encoding})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to WriteAllLines l294: ({path}, *, {encoding})");
            }
        }
        return default;
    }

    async Task<bool> IPCPlatformService.WriteAllLinesAsync(string path, IEnumerable<string> contents, int? encoding, CancellationToken cancellationToken)
    {
        try
        {
            // 先尝试直接执行
            if (encoding.HasValue)
            {
                var encoding_ = Encoding.GetEncoding(encoding.Value);
                await File.WriteAllLinesAsync(path, contents, encoding_, cancellationToken);
            }
            else
            {
                await File.WriteAllLinesAsync(path, contents, cancellationToken);
            }
            return true;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = await ipcPlatformService.WriteAllLinesAsync(path, contents, encoding, cancellationToken);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to WriteAllLinesAsync l330: ({path}, *, {encoding})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to WriteAllLinesAsync l336: ({path}, *, {encoding})");
            }
        }
        return default;
    }

    bool IPCPlatformService.WriteAllBytes(string path, byte[] bytes)
    {
        try
        {
            // 先尝试直接执行
            File.WriteAllBytes(path, bytes);
            return true;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = ipcPlatformService.WriteAllBytes(path, bytes);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to WriteAllBytes l364: ({path}, {bytes.Length})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to WriteAllBytes l370: ({path}, {bytes.Length})");
            }
        }
        return default;
    }

    async Task<bool> IPCPlatformService.WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken)
    {
        try
        {
            // 先尝试直接执行
            await File.WriteAllBytesAsync(path, bytes, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            if (!IsAdministrator)
            {
                // 通过 IPC 调用管理员服务进程执行
                try
                {
                    var ipcPlatformService = IPCRoot.Instance.GetAwaiter().GetResult();
                    var result = await ipcPlatformService.WriteAllBytesAsync(path, bytes, cancellationToken);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to WriteAllBytesAsync l398: ({path}, {bytes.Length})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                        $"Failed to WriteAllBytesAsync l404: ({path}, *, {bytes.Length})");
            }
        }
        return default;
    }

    #endregion
}
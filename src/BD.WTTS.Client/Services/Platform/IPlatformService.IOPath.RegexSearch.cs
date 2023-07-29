// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    string IPCPlatformService.RegexSearchFile(string filePath, string pattern)
    {
        try
        {
            // 先尝试直接执行
            var result = IOPath.RegexSearchFile(filePath, pattern);
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
                    var result = ipcPlatformService.RegexSearchFile(filePath, pattern);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to RegexSearchFile l27: ({filePath}, {pattern})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                    $"Failed to RegexSearchFile l34: ({filePath}, {pattern})");
            }
            return string.Empty;
        }
    }

    string IPCPlatformService.RegexSearchFolder(string dirPath, string pattern, string wildcard)
    {
        try
        {
            // 先尝试直接执行
            var result = IOPath.RegexSearchFolder(dirPath, pattern, wildcard);
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
                    var result = ipcPlatformService.RegexSearchFolder(dirPath, pattern, wildcard);
                    return result;
                }
                catch (Exception ipcEx)
                {
                    Log.Error(TAG, ipcEx,
                        $"Failed to RegexSearchFolder l61: ({dirPath}, {pattern}, {wildcard})");
                }
            }
            else
            {
                Log.Error(TAG, ex,
                    $"Failed to RegexSearchFolder l68: ({dirPath}, {pattern}, {wildcard})");
            }
            return string.Empty;
        }
    }
}
// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 使用文本阅读器打开文件
    /// </summary>
    /// <param name="filePath"></param>
    void OpenFileByTextReader(string filePath)
    {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        TextReaderProvider? userProvider = null;
        var p = GeneralSettings.TextReaderProvider.Value;
        if (p != null)
        {
            var platform = DeviceInfo2.Platform();
            if (p.ContainsKey(platform))
            {
                var value = p[platform];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (Enum.TryParse<TextReaderProvider>(value, out var enumValue))
                    {
                        userProvider = enumValue;
                    }
                    else
                    {
                        try
                        {
                            Process2.StartPath(value, filePath);
                            return;
                        }
                        catch (Exception e)
                        {
                            Log.Error(TAG, e, "UserSettings OpenFileByTextReader Fail.");
                        }
                    }
                }
            }
        }

        var providers = new List<TextReaderProvider>()
            {
                TextReaderProvider.VSCode,
                TextReaderProvider.Notepad,
            };

        if (userProvider.HasValue)
        {
            providers.Remove(userProvider.Value);
            providers.Insert(0, userProvider.Value);
        }

        foreach (var item in providers)
        {
            if (item == TextReaderProvider.VSCode && !OperatingSystem.IsWindows())
            {
                // 其他平台的 VSCode 打开方式尚未实现
                continue;
            }
            try
            {
                var fileName = GetFileName(item);
                if (fileName == null) continue;
                Process2.StartPath(fileName, filePath);
                return;
            }
            catch (Exception e)
            {
                if (item == TextReaderProvider.Notepad)
                {
                    Log.Error(TAG, e, "OpenFileByTextReader Fail.");
                }
            }
        }
#endif
    }

    /// <summary>
    /// 获取文本阅读器提供商程序文件路径或文件名(如果提供程序已注册环境变量)
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    string? GetFileName(TextReaderProvider provider) => null;
}
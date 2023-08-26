// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
#if !LIB_CLIENT_IPC
    /// <inheritdoc cref="WindowsPlatformServiceImpl.StartProcessRegeditAsync(string, string, int)"/>
#endif
    [SupportedOSPlatform("windows")]

#if !LIB_CLIENT_IPC && WINDOWS
    async
#endif
    Task<byte> StartProcessRegeditCoreIPCAsync(
        string? markKey,
        string? markValue,
        string path,
        int millisecondsDelay)
    {
        const byte Code_Ok = 200;
#if !LIB_CLIENT_IPC && WINDOWS

        switch (markKey)
        {
            case nameof(WindowsPlatformServiceImpl.SetAsSystemProxyAsync):
                if (markValue == bool.TrueString)
                {
                    WindowsPlatformServiceImpl.SetAsSystemProxyStatus = true;
                }
                else if (markValue == bool.FalseString)
                {
                    WindowsPlatformServiceImpl.SetAsSystemProxyStatus = false;
                }
                break;
            case nameof(WindowsPlatformServiceImpl.SetAsSystemPACProxyAsync):
                if (markValue == bool.TrueString)
                {
                    WindowsPlatformServiceImpl.SetAsSystemPACProxyStatus = true;
                }
                else if (markValue == bool.FalseString)
                {
                    WindowsPlatformServiceImpl.SetAsSystemPACProxyStatus = false;
                }
                break;
        }

        await WindowsPlatformServiceImpl.StartProcessRegeditCoreAsync(path, millisecondsDelay);
        return Code_Ok;
#else
        return Task.FromResult(Code_Ok);
#endif
    }

    #region Registry2

    /// <inheritdoc cref="Registry2.ReadRegistryKey(string, RegistryView)"/>
    [SupportedOSPlatform("windows")]
    string? ReadRegistryKey(string encodedPath, RegistryView view = Registry2.DefaultRegistryView)
    {
#if !LIB_CLIENT_IPC && WINDOWS
        var result = WindowsPlatformServiceImpl.ReadRegistryKeyCore(encodedPath, view);
        return result;
#else
        return default;
#endif
    }

    /// <inheritdoc cref="Registry2.SetRegistryKey(string, RegistryView, string?)"/>
    [SupportedOSPlatform("windows")]
    bool SetRegistryKey(string encodedPath, RegistryView view, string? value = null)
    {
#if !LIB_CLIENT_IPC && WINDOWS
        var result = WindowsPlatformServiceImpl.SetRegistryKeyCore(encodedPath, view, value);
        return result;
#else
        return default;
#endif
    }

    /// <inheritdoc cref="Registry2.DeleteRegistryKey(string, RegistryView)"/>
    [SupportedOSPlatform("windows")]
    bool DeleteRegistryKey(string encodedPath, RegistryView view = Registry2.DefaultRegistryView)
    {
#if !LIB_CLIENT_IPC && WINDOWS
        var result = WindowsPlatformServiceImpl.DeleteRegistryKeyCore(encodedPath, view);
        return result;
#else
        return default;
#endif
    }

    #endregion
}

public static partial class IPCPlatformServiceExtensions
{
    #region Registry2

    /// <inheritdoc cref="Registry2.TryReadRegistryKey(string, RegistryView, out object?)"/>
    [SupportedOSPlatform("windows")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadRegistryKey(
        this IPCPlatformService s,
        string encodedPath,
        RegistryView view,
        [NotNullWhen(true)] out string? value)
    {
        value = s.ReadRegistryKey(encodedPath, view);
        return value != null;
    }

    /// <inheritdoc cref="Registry2.TryReadRegistryKey(string, out object?)"/>
    [SupportedOSPlatform("windows")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadRegistryKey(
        this IPCPlatformService s,
        string encodedPath,
        [NotNullWhen(true)] out string? value)
    {
        value = s.ReadRegistryKey(encodedPath);
        return value != null;
    }

    /// <inheritdoc cref="Registry2.SetRegistryKey(string, string?)"/>
    [SupportedOSPlatform("windows")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SetRegistryKey(
        this IPCPlatformService s,
        string encodedPath,
        string? value = null)
    {
        var result = s.SetRegistryKey(encodedPath, Registry2.DefaultRegistryView, value);
        return result;
    }

    #endregion
}
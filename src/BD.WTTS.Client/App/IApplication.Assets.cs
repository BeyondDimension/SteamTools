// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    // 资产资源应由 UI 框架的 App 重写实现，此接口上默认实现仅供参考

    /// <summary>
    /// 根据图标资源键名获取图标资源
    /// </summary>
    /// <param name="iconKey"></param>
    /// <returns></returns>
    string? GetIconSourceByIconKey(string? iconKey) => string.IsNullOrWhiteSpace(iconKey) ? null :
#if WINDOWS || MACCATALYST || MACOS || LINUX
        $"avares://BD.WTTS.Client.Avalonia.App/UI/Assets/Icons/{iconKey}.ico";
#elif IOS || ANDROID
        iconKey;
#endif

    /// <summary>
    /// 根据类型名称获取图标资源键名
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    string GetIconKeyByTypeName(string typeName)
    {
#if WINDOWS || MACCATALYST || MACOS || LINUX
        return typeName;
#elif IOS || ANDROID
        return $"{typeName.ToLowerInvariant()}.png";
#endif
    }
}
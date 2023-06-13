namespace BD.WTTS.Plugins.Abstractions;

partial interface IPlugin
{
    /// <summary>
    /// 插件名，忽略大小写
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 插件版本号，<see cref="Version.TryParse(string?, out Version?)"/> 可能返回 <see langword="false"/>，当字符串中存在符号或字母时
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 插件描述
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// 插件商店地址
    /// </summary>
    string StoreUrl { get; }

    /// <summary>
    /// 插件帮助页面地址
    /// </summary>
    string HelpUrl { get; }

    /// <summary>
    /// 插件设置页面视图类型
    /// </summary>
    Type? SettingsPageViewType { get; }

    /// <summary>
    /// 插件作者名
    /// </summary>
    string? Author { get; }

    /// <summary>
    /// 插件作者的商店地址
    /// </summary>
    string AuthorStoreUrl { get; }

    /// <summary>
    /// 插件的程序集位置
    /// </summary>
    string AssemblyLocation { get; }

    /// <summary>
    /// 获取可存储插件数据的位置
    /// </summary>
    string AppDataDirectory { get; }

    /// <summary>
    /// 获取可以存储临时数据的位置
    /// </summary>
    string CacheDirectory { get; }

    /// <summary>
    /// 是否为官方插件
    /// </summary>
    bool IsOfficial { get; }
}

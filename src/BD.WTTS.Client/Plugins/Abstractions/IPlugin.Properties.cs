namespace BD.WTTS.Plugins.Abstractions;

partial interface IPlugin
{
    /// <summary>
    /// 插件 Id
    /// </summary>
    Guid Id => default;

    /// <summary>
    /// 插件名，忽略大小写
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 插件唯一英文名，与 Id 一样不能有重复的
    /// </summary>
    string UniqueEnglishName { get; }

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

    /// <summary>
    /// 插件显示图标图片资源路径，值为 <see langword="null"/> 时使用默认图片
    /// </summary>
    object? Icon { get; }

    /// <summary>
    /// 插件安装时间
    /// </summary>
    DateTimeOffset InstallTime { get; }

    /// <summary>
    /// 插件发布时间
    /// </summary>
    DateTimeOffset ReleaseTime { get; }

    /// <summary>
    /// 加载当前插件时产生的错误
    /// </summary>
    string? LoadError { get; internal set; }
}

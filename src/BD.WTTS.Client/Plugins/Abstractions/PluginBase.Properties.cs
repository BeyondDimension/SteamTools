namespace BD.WTTS.Plugins.Abstractions;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
partial class PluginBase
{
    string DebuggerDisplay => $"{UniqueEnglishName} v{Version}";

    public override string ToString() => DebuggerDisplay;

    public virtual Guid Id => default;

    public abstract string Name { get; }

    public abstract string UniqueEnglishName { get; }

    public abstract string Version { get; }

    public virtual string StoreUrl =>
        $"https://steampp.net/store/plugins/names/{WebUtility.UrlEncode(UniqueEnglishName.ToLowerInvariant())}";

    public virtual string HelpUrl =>
        $"https://steampp.net/store/plugins/helps/names/{WebUtility.UrlEncode(UniqueEnglishName.ToLowerInvariant())}";

    public virtual Type? SettingsPageViewType { get; }

    /// <summary>
    /// 作者名字符串中是否存在非法字符
    /// </summary>
    /// <param name="author"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsIllegalAuthor(string author)
    {
        if (author.Contains("Steam++", StringComparison.OrdinalIgnoreCase) ||
            author.Contains("Steam", StringComparison.OrdinalIgnoreCase) ||
            author.Contains("steampp", StringComparison.OrdinalIgnoreCase) ||
            author.Contains("Watt", StringComparison.OrdinalIgnoreCase) ||
            author.Contains("WattToolkit", StringComparison.OrdinalIgnoreCase) ||
            author.Contains("\u6B21\u5143\u8D85\u8D8A", StringComparison.OrdinalIgnoreCase) ||
            author.Contains("\u51E1\u661F", StringComparison.OrdinalIgnoreCase) ||
            author.Contains("\u7E41\u661F", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    public string? Author
    {
        get
        {
            if (IsOfficial)
                return Strings.Plugin_OfficialAuthor_.Format(AssemblyInfo.Trademark);
            var author = AuthorOriginalString;
            if (string.IsNullOrWhiteSpace(author))
                return Strings.Plugin_UnknownAuthor;
            if (IsIllegalAuthor(author))
                return Strings.Plugin_IllegalAuthor;
            return author;
        }
    }

    /// <summary>
    /// 插件作者名（由插件程序集重写此属性填写）
    /// </summary>
    protected virtual string? AuthorOriginalString { get; }

    public virtual string AuthorStoreUrl =>
        $"https://steampp.net/store/plugins/authors/{WebUtility.UrlEncode(Author)}";

    public virtual string? Description { get; }

    public abstract string AssemblyLocation { get; }

    public string AppDataDirectory => mAppDataDirectory.Value;

    public string CacheDirectory => mCacheDirectory.Value;

    ///// <summary>
    ///// 官方插件的哈希值 SHA256 数据，可使用 API 调用服务端获取
    ///// </summary>
    //internal static SecurityCriticalOfficialPluginHashDatas OfficialPluginHashDatas { private get; set; }

    //internal readonly record struct SecurityCriticalOfficialPluginHashDatas
    //{
    //    internal SecurityCriticalOfficialPluginHashDatas(ImmutableArray<byte[]> value)
    //    {
    //        StackTrace stackTrace = new();
    //        var isInternalCall = ReflectionHelper.IsInternalCall<SecurityCriticalOfficialPluginHashDatas>(stackTrace);
    //        if (!isInternalCall)
    //            throw new ApplicationException("Disable reflection calls to this constructor.");

    //        Value = value;
    //    }

    //    internal ImmutableArray<byte[]> Value { get; init; }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    internal bool IsDefault() => this == default || Value == default || Value.IsEmpty;
    //}

    //    /// <summary>
    //    /// 是否为嵌入式插件，嵌入在程序安装目录中的
    //    /// </summary>
    //    /// <returns></returns>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    bool IsEmbeddedPlugin()
    //    {
    //        try
    //        {
    //            var rootPath = Path.GetFullPath(Path.Combine(AssemblyLocation,
    //#if DEBUG
    //                "..", "..", "..", "..", ".."
    //#else
    //                "..", "..", ".."
    //#endif
    //                ));
    //            var value = Environment.ProcessPath!.StartsWith(rootPath);
    //            return value;
    //        }
    //        catch
    //        {
    //        }
    //        return false;
    //    }

    public bool IsOfficial
    {
        get // 实时计算，不缓存结果
        {
            //            if (!OfficialPluginHashDatas.IsDefault())
            //            {
            //                try
            //                {
            //                    byte[] hashData;
            //                    using (var fileStream = new FileStream(AssemblyLocation,
            //                        FileMode.Open,
            //                        FileAccess.Read,
            //                        FileShare.ReadWrite | FileShare.Delete))
            //                    {
            //                        hashData = SHA256.HashData(fileStream);
            //                    }
            //                    if (OfficialPluginHashDatas.Value.Contains(hashData))
            //                    {
            //                        return true;
            //                    }
            //                }
            //                catch
            //                {

            //                }
            //            }
            //#if DEBUG
            var thisType = GetType();
            var value = thisType.FullName == "BD.WTTS.Plugins.Plugin" &&
                thisType.Assembly.GetName().Name?.TrimStart("BD.WTTS.Client.Plugins.") switch
                {
                    AssemblyInfo.Accelerator => // IsEmbeddedPlugin() &&
                        UniqueEnglishName == AssemblyInfo.Accelerator &&
                        Id.ToString() == AssemblyInfo.AcceleratorId,
                    AssemblyInfo.ArchiSteamFarmPlus => // IsEmbeddedPlugin() &&
                        UniqueEnglishName == AssemblyInfo.ArchiSteamFarmPlus &&
                        Id.ToString() == AssemblyInfo.ArchiSteamFarmPlusId,
                    AssemblyInfo.Authenticator => // IsEmbeddedPlugin() &&
                        UniqueEnglishName == AssemblyInfo.Authenticator &&
                        Id.ToString() == AssemblyInfo.AuthenticatorId,
                    AssemblyInfo.GameAccount => // IsEmbeddedPlugin() &&
                        UniqueEnglishName == AssemblyInfo.GameAccount &&
                        Id.ToString() == AssemblyInfo.GameAccountId,
                    AssemblyInfo.GameList => // IsEmbeddedPlugin() &&
                        UniqueEnglishName == AssemblyInfo.GameList &&
                        Id.ToString() == AssemblyInfo.GameListId,
                    AssemblyInfo.GameTools => // IsEmbeddedPlugin() &&
                        UniqueEnglishName == AssemblyInfo.GameTools &&
                        Id.ToString() == AssemblyInfo.GameToolsId,
                    AssemblyInfo.SteamIdleCard => // IsEmbeddedPlugin() &&
                        UniqueEnglishName == AssemblyInfo.SteamIdleCard &&
                        Id.ToString() == AssemblyInfo.SteamIdleCardId,
                    _ => false,
                };
            return value;
            //#else
            //            return false;
            //#endif
        }
    }

    public virtual object? Icon => null;

    DateTimeOffset GetInstallTime()
    {
        try
        {
            var location = AssemblyLocation;
            var time = new FileInfo(location).CreationTime;
            return time;
        }
        catch
        {
        }
        return default;
    }

    public DateTimeOffset InstallTime => mInstallTime.Value;

    public DateTimeOffset ReleaseTime { get; internal set; }

    string? loadError;

    string? IPlugin.LoadError
    {
        get => loadError;
        set => loadError = value;
    }
}

partial class PluginBase<TPlugin>
{
    public static TPlugin Instance { get; private set; } = null!;

    public static IPlugin InterfaceInstance => Instance;

    static readonly Lazy<string> mFileVersion = new(() =>
    {
        var assembly = typeof(TPlugin).Assembly;
        string? version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        return version ?? string.Empty;
    });

    public static string FileVersion => mFileVersion.Value;

    readonly Lazy<string> mVersion = new(() =>
    {
        var assembly = typeof(TPlugin).Assembly;
        string? version = null;
        for (int i = 1; string.IsNullOrWhiteSpace(version); i++)
        {
            version = i switch
            {
                2 => assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? string.Empty,
                3 => assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? string.Empty,
                1 => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty,
                _ => null,
            };
            if (version == null) return string.Empty;
        }
        return version;
    });

    public sealed override string Version => mVersion.Value;

    readonly Lazy<string> mAssemblyLocation = new(() =>
    {
        var assemblyLocation = typeof(TPlugin).Assembly.Location;
        return assemblyLocation.ThrowIsNull();
    });

    public sealed override string AssemblyLocation => mAssemblyLocation.Value;
}

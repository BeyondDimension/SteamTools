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

    public virtual string? Author { get; }

    public virtual string AuthorStoreUrl =>
        $"https://steampp.net/store/plugins/authors/{WebUtility.UrlEncode(Author)}";

    public virtual string? Description { get; }

    public abstract string AssemblyLocation { get; }

    public string AppDataDirectory => mAppDataDirectory.Value;

    public string CacheDirectory => mCacheDirectory.Value;

    //static byte[][] OfficialPluginHashDatas => Array.Empty<byte[]>();

    bool? mIsOfficial;

    public bool IsOfficial
    {
        get
        {
            //#if DEBUG
            //switch (Name)
            //{
            //    case AssemblyInfo.Accelerator:
            //    case AssemblyInfo.ArchiSteamFarmPlus:
            //    case AssemblyInfo.Authenticator:
            //    case AssemblyInfo.GameAccount:
            //    case AssemblyInfo.GameList:
            //    case AssemblyInfo.GameTools:
            //        return true;
            //}
            //#endif
            if (mIsOfficial.HasValue)
                return mIsOfficial.Value;
            //try
            //{
            //    byte[] hashData;
            //    using (var fileStream = new FileStream(AssemblyLocation,
            //        FileMode.Open,
            //        FileAccess.Read,
            //        FileShare.ReadWrite | FileShare.Delete))
            //    {
            //        hashData = SHA256.HashData(fileStream);
            //    }
            //    if (PluginBase<TPlugin>.OfficialPluginHashDatas.Contains(hashData))
            //    {
            //        mIsOfficial = true;
            //        return true;
            //    }
            //}
            //catch
            //{

            //}
            var thisType = GetType();
            mIsOfficial = thisType.Name == "BD.WTTS.Plugins.Plugin" &&
                thisType.Assembly.GetName().Name?.TrimStart("BD.WTTS.Client.Plugins.") switch
                {
                    AssemblyInfo.Accelerator or
                    AssemblyInfo.ArchiSteamFarmPlus or
                    AssemblyInfo.Authenticator or
                    AssemblyInfo.GameAccount or
                    AssemblyInfo.GameList or
                    AssemblyInfo.GameTools => true,
                    _ => false,
                };
            return mIsOfficial.Value;
            //mIsOfficial = false;
            //return false;
        }
    }

    public virtual string? Icon => null;

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
}

partial class PluginBase<TPlugin>
{
    public static TPlugin Instance { get; private set; } = null!;

    public static IPlugin InterfaceInstance => Instance;

    readonly Lazy<string> mVersion = new(() =>
    {
        var assembly = typeof(TPlugin).Assembly;
        string? version = null;
        for (int i = 0; string.IsNullOrWhiteSpace(version); i++)
        {
            version = i switch
            {
                0 => assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? string.Empty,
                1 => assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? string.Empty,
                2 => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty,
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

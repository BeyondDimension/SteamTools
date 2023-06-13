namespace BD.WTTS.Plugins.Abstractions;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
partial class PluginBase<TPlugin>
{
    string DebuggerDisplay => Name;

    public static TPlugin Instance { get; private set; } = null!;

    public abstract string Name { get; }

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

    public virtual string Version => mVersion.Value;

    public virtual string StoreUrl =>
        $"https://steampp.net/store/plugins/names/{WebUtility.UrlEncode(Name.ToLowerInvariant())}";

    public virtual string HelpUrl =>
        $"https://steampp.net/store/plugins/helps/names/{WebUtility.UrlEncode(Name.ToLowerInvariant())}";

    public virtual Type? SettingsPageViewType { get; }

    public virtual string? Author { get; }

    public virtual string AuthorStoreUrl =>
        $"https://steampp.net/store/plugins/authors/{WebUtility.UrlEncode(Author)}";

    public virtual string? Description { get; }

    readonly Lazy<string> mAssemblyLocation = new(() =>
    {
        var assemblyLocation = typeof(TPlugin).Assembly.Location;
        return assemblyLocation.ThrowIsNull();
    });

    public virtual string AssemblyLocation => mAssemblyLocation.Value;

    public string AppDataDirectory => mAppDataDirectory.Value;

    public string CacheDirectory => mCacheDirectory.Value;

    static byte[][] OfficialPluginHashDatas => Array.Empty<byte[]>();

    bool? mIsOfficial;

    public bool IsOfficial
    {
        get
        {
#if DEBUG
            switch (Name)
            {
                case "Accelerator":
                case "ArchiSteamFarmPlus":
                case "Authenticator":
                case "GameAccount":
                case "GameList":
                case "GameTools":
                    return true;
            }
#endif
            if (mIsOfficial.HasValue)
                return mIsOfficial.Value;
            try
            {
                byte[] hashData;
                using (var fileStream = new FileStream(AssemblyLocation,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                {
                    hashData = SHA256.HashData(fileStream);
                }
                if (PluginBase<TPlugin>.OfficialPluginHashDatas.Contains(hashData))
                {
                    mIsOfficial = true;
                    return true;
                }
            }
            catch
            {

            }
            mIsOfficial = false;
            return false;
        }
    }
}

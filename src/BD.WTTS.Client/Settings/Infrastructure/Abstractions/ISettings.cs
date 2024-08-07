using SJsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Extensions.FileProviders;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public interface ISettings
{
    private static readonly MethodInfo TrySaveMethod;

    static ISettings()
    {
        var trySaveMethod = typeof(SettingsExtensions).GetMethod(nameof(SettingsExtensions.TrySave_____));
        TrySaveMethod = trySaveMethod.ThrowIsNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void TrySave([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] Type type, object optionsMonitor, bool notRead = false)
        => TrySaveMethod.MakeGenericMethod(type).Invoke(null, new object?[] {
            optionsMonitor,
            notRead ? bool.TrueString : null,
        });

    static abstract string Name { get; }

    static abstract JsonSerializerContext JsonSerializerContext { get; }

    static abstract JsonTypeInfo JsonTypeInfo { get; }

    protected const string DirName = "Settings";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool DirectoryExists(string? appDataDirectory = null)
    {
        var settingsDirPath = Path.Combine(appDataDirectory ?? IOPath.AppDataDirectory, DirName);
        if (!Directory.Exists(settingsDirPath))
        {
            Directory.CreateDirectory(settingsDirPath);
            return false;
        }
        return true;
    }

    private static readonly ConcurrentDictionary<Type, (string Name, string FilePath)> cacheFilePaths = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetFilePath(Type type, string name, string? appDataDirectory = null)
    {
        if (cacheFilePaths.TryGetValue(type, out var result))
        {
            return result.FilePath;
        }
        else
        {
            var settingsFilePath = Path.Combine(
                appDataDirectory ?? IOPath.AppDataDirectory,
                DirName,
                name + FileEx.JSON);

            cacheFilePaths.TryAdd(type, (name, settingsFilePath));
            return settingsFilePath;
        }
    }

    protected static readonly HashSet<Type> types = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SaveSettings([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        try
        {
            var optionsType = typeof(IOptionsMonitor<>).MakeGenericType(type);
            var options = Ioc.Get_Nullable(optionsType);
            if (options != null)
                TrySave(type, options);
        }
        catch (Exception ex)
        {
            Startup.GlobalExceptionHandler.Handler(ex, nameof(SaveSettings));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static async Task SaveAllSettingsAsync()
    {
        try
        {
            await Parallel.ForEachAsync(types, async ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] type, _) => await Task.Run(() =>
            {
                SaveSettings(type);
            }).ConfigureAwait(false)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Startup.GlobalExceptionHandler.Handler(ex, nameof(SaveAllSettingsAsync));
        }
    }

    static JsonSerializerOptions GetDefaultOptions()
    {
        var o = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = true,
            IncludeFields = false,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        o.Converters.Add(new JsonStringEnumConverter());
        return o;
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    sealed class JsonTypeInfoResolver : IJsonTypeInfoResolver
    {
        readonly DefaultJsonTypeInfoResolver resolver = new();

        static readonly Lazy<JsonTypeInfoResolver> instance = new(() => new());

        public static IJsonTypeInfoResolver Instance => instance.Value;

        JsonTypeInfoResolver() { }

        JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            if (types.Contains(type))
            {
                try
                {
#pragma warning disable IL2070 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.
                    if (typeof(JsonTypeInfoResolver)
                        .GetMethod(nameof(GetJsonTypeInfo), BindingFlags.NonPublic | BindingFlags.Static)
                        ?.MakeGenericMethod(type).Invoke(null, null) is JsonTypeInfo jsonTypeInfo)
                    {
                        if (jsonTypeInfo.Options == options)
                            return jsonTypeInfo;
                        if (Activator.CreateInstance(jsonTypeInfo.GetType(), options) is JsonTypeInfo jsonTypeInfo1)
                            return jsonTypeInfo1;
                    }
#pragma warning restore IL2070 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.
                }
                catch
                {

                }
            }
            return resolver.GetTypeInfo(type, options);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static JsonTypeInfo<TSettings> GetJsonTypeInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSettings>() where TSettings : class, ISettings<TSettings>, new()
            => TSettings.JsonTypeInfo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static TSettings? Deserialize<TSettings>(string settingsFilePath) where TSettings : ISettings
    {
        JsonObject? jobj;
        var options = ISettings.GetDefaultOptions();
        using var readStream = new FileStream(
            settingsFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        jobj = SJsonSerializer.Deserialize<JsonObject>(readStream, options);
        if (jobj != null)
        {
            var jnode = jobj[TSettings.Name];
            if (jnode != null)
            {
                options = ISettings.GetDefaultOptions();
                options.TypeInfoResolver = ISettings.JsonTypeInfoResolver.Instance;
                var settingsByRead = SJsonSerializer.Deserialize<TSettings>(jnode, options);
                return settingsByRead;
            }
        }
        return default;
    }
}

public interface ISettings<TSettings> : ISettings where TSettings : class, ISettings<TSettings>, new()
{
    static new abstract JsonTypeInfo<TSettings> JsonTypeInfo { get; }

    /// <summary>
    /// 从 UTF8 Json 流中反序列化实例
    /// </summary>
    /// <param name="utf8Json"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static TSettings Deserialize(Stream utf8Json)
        => SJsonSerializer.Deserialize(utf8Json, TSettings.JsonTypeInfo) ?? new();

    sealed class OptionsMonitor : IOptionsMonitor<TSettings>, IOptions<TSettings>
    {
        TSettings settings;
        readonly string settingsFileName;
        readonly string settingsFilePath;
        readonly PhysicalFileProvider fileProvider;

        public OptionsMonitor(string settingsFilePath, TSettings? settings = default)
        {
            this.settingsFilePath = settingsFilePath;
            var settingsDirPath = Path.GetDirectoryName(settingsFilePath);
            settingsDirPath.ThrowIsNull();
            this.settings = settings ?? ISettings.Deserialize<TSettings>(settingsFilePath) ?? new();
            fileProvider = new(settingsDirPath);
            settingsFileName = Path.GetFileName(settingsFilePath);
        }

        TSettings IOptionsMonitor<TSettings>.CurrentValue => settings;

        TSettings IOptions<TSettings>.Value => settings;

        TSettings IOptionsMonitor<TSettings>.Get(string? name) => settings;

        TSettings? AllowNullDeserialize()
        {
            try
            {
                var settings = ISettings.Deserialize<TSettings>(settingsFilePath);
                return settings;
            }
            catch
            {
                return default;
            }
        }

        IDisposable? IOptionsMonitor<TSettings>.OnChange(Action<TSettings, string?> listener)
            => ChangeToken.OnChange(() => fileProvider.Watch(settingsFileName), () =>
        {
            var settings_ = AllowNullDeserialize();
            if (settings_ != null)
            {
                // 监听到的设置模型实例，如果和 new 一个空的数据一样的，就是默认值则忽略
                var newSettingsData = Serializable.SMP2(settings);
                var emptySettingsData = Serializable.SMP2(new TSettings());
                if (newSettingsData.SequenceEqual(emptySettingsData))
                    return;

                settings = settings_;

                listener.Invoke(settings, TSettings.Name);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool Load(bool directoryExists,
       out Action<IServiceCollection>? @delegate,
       string? appDataDirectory = null)
        => Load(directoryExists,
            out @delegate,
            out var _,
            appDataDirectory);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool Load(bool directoryExists,
        out Action<IServiceCollection>? @delegate,
        out IOptionsMonitor<TSettings>? options,
        string? appDataDirectory = null)
    {
        options = default;
        @delegate = default;

        var type = typeof(TSettings);
        if (!types.Add(type))
            return default;

        TSettings? settings = default;

        var settingsFilePath = GetFilePath(type, TSettings.Name, appDataDirectory);

        bool writeFile = false;
        if (directoryExists)
        {
            if (!File.Exists(settingsFilePath))
            {
                writeFile = true;
            }
        }
        else
        {
            writeFile = true;
        }

        bool isInvalid = false;
        if (writeFile)
        {
            using var ms = new MemoryStream();
            settings = new();
            settings.Save_____(ms);
            ms.Position = 0;
            using var fs = File.Create(settingsFilePath);
            ms.CopyTo(fs);
            fs.Flush();
            fs.SetLength(fs.Position);
        }
        else
        {
            try
            {
                settings = ISettings.Deserialize<TSettings>(settingsFilePath) ?? new();
            }
            catch
            {
                settings = new();
                isInvalid = true;

                // 尝试将错误的配置保存为 .json.load.bak 防止启动软件当前配置被覆盖
                if (!SettingsExtensions.IsZeroFile(settingsFilePath))
                {
                    var settingsFilePath_load_bak = $"{settingsFilePath}.load.bak";
                    try
                    {
                        IOPath.FileIfExistsItDelete(settingsFilePath_load_bak);
                        File.Move(settingsFilePath,
                            settingsFilePath_load_bak, true);
                    }
                    catch
                    {

                    }
                }
            }
        }

        var monitor = new OptionsMonitor(settingsFilePath, settings);
        @delegate = s =>
        {
            s.AddSingleton<IOptions<TSettings>>(_ => monitor);
            s.AddSingleton<IOptionsMonitor<TSettings>>(_ => monitor);
        };
        options = monitor;
        return isInvalid;
    }
}

internal static class SettingsExtensions
{
    internal static bool IsZeroFile(string filePath, bool @catch = true)
    {
        try
        {
            return new FileInfo(filePath).Length == 0;
        }
        catch
        {
            return @catch;
        }
    }

    /// <summary>
    /// 将实例序列化为字符串
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    /// <param name="settings"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Serialize<TSettings>(this TSettings settings) where TSettings : ISettings
        => SJsonSerializer.Serialize(settings, typeof(TSettings), TSettings.JsonSerializerContext);

    /// <summary>
    /// 将实例序列化写入 UTF8 Json 流
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    /// <param name="utf8Json"></param>
    /// <param name="settings"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TSettings>(this TSettings settings, Stream utf8Json) where TSettings : ISettings
        => SJsonSerializer.Serialize(utf8Json, settings, typeof(TSettings), TSettings.JsonSerializerContext);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Save_____<TSettings>(this TSettings settings, Stream utf8Json) where TSettings : ISettings
    {
        utf8Json.Position = 0;
        utf8Json.Write("{\""u8);
        utf8Json.Write(Encoding.UTF8.GetBytes(TSettings.Name));
        utf8Json.Write("\":"u8);
        Serialize(settings, utf8Json);
        utf8Json.Write("}"u8);
        utf8Json.SetLength(utf8Json.Position);
        utf8Json.Flush();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void TrySave_____<TSettings>(IOptionsMonitor<TSettings> optionsMonitor, string? notRead = null) where TSettings : class, ISettings
    {
        var settings = optionsMonitor.CurrentValue;
        var settingsFilePath = ISettings.GetFilePath(typeof(TSettings), TSettings.Name);

        lock (TSettings.Name)
        {
            var settingsType = typeof(TSettings);
            SettingsPropertyBase.SetSaveStatus(settingsType);

            try
            {
                if (notRead != bool.TrueString) // 通过读取配置文件与内存中的配置进行比较
                {
                    try
                    {
                        if (File.Exists(settingsFilePath))
                        {
                            var settingsByRead = ISettings.Deserialize<TSettings>(settingsFilePath);
                            if (settingsByRead != default)
                            {
                                // 使用 MemoryPack 序列化两者比较相等
                                var right = MemoryPackSerializer.Serialize(settingsByRead);
                                var left = MemoryPackSerializer.Serialize(settings);
                                if (left.SequenceEqual(right))
                                    return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                static FileStream? OpenOrCreate(string path)
                {
                    try
                    {
                        return new FileStream(
                                        path,
                                        FileMode.OpenOrCreate,
                                        FileAccess.Write,
                                        FileShare.ReadWrite | FileShare.Delete);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine(ex);
#endif
                        return default;
                    }
                }

                try
                {
                    if (!SettingsExtensions.IsZeroFile(settingsFilePath))
                    {
                        var settingsFilePath2 = $"{settingsFilePath}.save.bak";
                        IOPath.FileTryDelete(settingsFilePath2);
                        File.Move(settingsFilePath, settingsFilePath2, true);
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine("bak settings file fail, ex: " + ex);
#endif
                }

                Policy.Handle<Exception>().Retry(3).Execute(() =>
                {
                    var dirPath = Path.GetDirectoryName(settingsFilePath);
                    if (dirPath != null) IOPath.DirCreateByNotExists(dirPath);
                    using var writeStream = OpenOrCreate(settingsFilePath);
                    if (writeStream == null)
                    {
                        using var memoryStream = new MemoryStream();
                        Save_____(settings, memoryStream);
                        var bytes = memoryStream.ToByteArray();
                        File.WriteAllBytes(settingsFilePath, bytes);
                    }
                    else
                    {
                        Save_____(settings, writeStream);
                    }
                });

            }
            finally
            {
            }
        }
    }
}
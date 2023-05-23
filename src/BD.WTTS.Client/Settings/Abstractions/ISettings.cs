// ReSharper disable once CheckNamespace
using Polly;

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
    internal static bool DirectoryExists()
    {
        var settingsDirPath = Path.Combine(IOPath.AppDataDirectory, DirName);
        if (!Directory.Exists(settingsDirPath))
        {
            Directory.CreateDirectory(settingsDirPath);
            return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetFilePath(string name)
    {
        var settingsFilePath = Path.Combine(
            IOPath.AppDataDirectory,
            DirName,
            name + FileEx.JSON);

        return settingsFilePath;
    }

    protected static readonly HashSet<Type> types = new();

    private static readonly Lazy<ConcurrentDictionary<Type, bool>> lazy_save_status = new(() =>
     {
         return new(types.Select(x => new KeyValuePair<Type, bool>(x, false)));
     });

    static ConcurrentDictionary<Type, bool> SaveStatus => lazy_save_status.Value;

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
        };
        o.Converters.Add(new JsonStringEnumConverter());
        return o;
    }

    /// <summary>
    /// 使用默认配置 Build Configuration
    /// </summary>
    /// <returns></returns>
    static IConfigurationRoot DefaultBuild()
    {
        ConfigurationBuilder builder = new();

        var saveMethod = typeof(SettingsExtensions).GetMethod(nameof(SettingsExtensions.Save_____));
        saveMethod.ThrowIsNull();

        foreach (var type in types)
        {
#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
            var settings = Activator.CreateInstance(type);
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
            var memoryStream = new MemoryStream();
            saveMethod.MakeGenericMethod(type)
                .Invoke(null, new object?[] { settings, memoryStream });
            memoryStream.Position = 0;
            builder.AddJsonStream(memoryStream);
        }

        return builder.Build();
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
                    if (typeof(JsonTypeInfoResolver)
                        .GetMethod(nameof(GetJsonTypeInfo), BindingFlags.NonPublic | BindingFlags.Static)
                        ?.MakeGenericMethod(type).Invoke(null, null) is JsonTypeInfo jsonTypeInfo)
                    {
                        if (jsonTypeInfo.Options == options)
                            return jsonTypeInfo;
                        if (Activator.CreateInstance(jsonTypeInfo.GetType(), options) is JsonTypeInfo jsonTypeInfo1)
                            return jsonTypeInfo1;
                    }
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
        => JsonSerializer.Deserialize(utf8Json, TSettings.JsonTypeInfo) ?? new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Action<IConfiguration, IServiceCollection>? Load(
        ConfigurationBuilder builder,
        bool directoryExists)
    {
        var type = typeof(TSettings);
        if (!types.Add(type)) return null;

        var settingsFilePath = GetFilePath(TSettings.Name);

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

        if (writeFile)
        {
            using var fs = File.Create(settingsFilePath);
            var settings = new TSettings();
            settings.Save_____(fs);
        }

        builder.AddJsonFile(settingsFilePath, true, true);

        return (configuration, services) =>
        {
            services.Configure<TSettings>(configuration.GetRequiredSection(TSettings.Name));
        };
    }
}

internal static class SettingsExtensions
{
    /// <summary>
    /// 将实例序列化为字符串
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    /// <param name="settings"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Serialize<TSettings>(this TSettings settings) where TSettings : ISettings
        => JsonSerializer.Serialize(settings, typeof(TSettings), TSettings.JsonSerializerContext);

    /// <summary>
    /// 将实例序列化写入 UTF8 Json 流
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    /// <param name="utf8Json"></param>
    /// <param name="settings"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TSettings>(this TSettings settings, Stream utf8Json) where TSettings : ISettings
        => JsonSerializer.Serialize(utf8Json, settings, typeof(TSettings), TSettings.JsonSerializerContext);

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
        var settingsFilePath = ISettings.GetFilePath(TSettings.Name);

        lock (TSettings.Name)
        {
            ISettings.SaveStatus[typeof(TSettings)] = true;

            try
            {
                if (notRead != bool.TrueString) // 通过读取配置文件与内存中的配置进行比较
                {
                    try
                    {
                        if (File.Exists(settingsFilePath))
                        {
                            JsonObject? jobj;
                            var options = ISettings.GetDefaultOptions();
                            using var readStream = new FileStream(
                                settingsFilePath,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.ReadWrite | FileShare.Delete);
                            jobj = JsonSerializer.Deserialize<JsonObject>(readStream, options);
                            if (jobj != null)
                            {
                                var jnode = jobj[TSettings.Name];
                                if (jnode != null)
                                {
                                    options = ISettings.GetDefaultOptions();
                                    options.TypeInfoResolver = ISettings.JsonTypeInfoResolver.Instance;
                                    var settingsByRead = JsonSerializer.Deserialize<TSettings>(jnode, options);

                                    // 使用 MemoryPack 序列化两者比较相等
                                    var right = MemoryPackSerializer.Serialize(settingsByRead);
                                    var left = MemoryPackSerializer.Serialize(settings);
                                    if (left.SequenceEqual(right))
                                        return;
                                }
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
                    var settingsFilePath2 = $"{settingsFilePath}.bak";
                    IOPath.FileTryDelete(settingsFilePath2);
                    File.Move(settingsFilePath, settingsFilePath2);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine("bak settings file fail, ex: " + ex);
#endif
                }

                Policy.Handle<Exception>().Retry(3).Execute(() =>
                {
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
                ISettings.SaveStatus[typeof(TSettings)] = false;
            }
        }
    }
}
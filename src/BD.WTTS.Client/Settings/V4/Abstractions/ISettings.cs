using Newtonsoft.Json.Converters;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public interface ISettings : IReactiveObject
{
    static readonly MethodInfo TrySaveMethod;

    static ISettings()
    {
        var trySaveMethod = typeof(SettingsExtensions).GetMethod(nameof(SettingsExtensions.TrySave_____));
        TrySaveMethod = trySaveMethod.ThrowIsNull();
    }

    static abstract string Name { get; }

    static abstract JsonSerializerContext JsonSerializerContext { get; }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SaveSettings(Type type)
    {
        try
        {
            var optionsType = typeof(IOptions<>).MakeGenericType(type);
            var options = Ioc.Get_Nullable(optionsType);
            if (options != null)
                TrySaveMethod.MakeGenericMethod(type).Invoke(null, new[] { options });
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
            await Parallel.ForEachAsync(types, async (type, _) => await Task.Run(() =>
            {
                SaveSettings(type);
            }).ConfigureAwait(false)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Startup.GlobalExceptionHandler.Handler(ex, nameof(SaveAllSettingsAsync));
        }
    }

    public static JsonSerializerOptions GetDefaultOptions()
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
}

public interface ISettings<TSettings> : ISettings where TSettings : class, ISettings<TSettings>, new()
{
    static abstract JsonTypeInfo<TSettings> JsonTypeInfo { get; }

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
            settings.Save(fs);
        }

        builder.AddJsonFile(settingsFilePath);

        return (configuration, services) =>
        {
            services.Configure<TSettings>(configuration.GetRequiredSection(TSettings.Name));
        };
    }
}

public static class SettingsExtensions
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Save<TSettings>(this TSettings settings, Stream utf8Json) where TSettings : ISettings
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
    public static void TrySave_____<TSettings>(IOptions<TSettings> options) where TSettings : class, ISettings
    {
        var settings = options.Value;
        var settingsFilePath = ISettings.GetFilePath(TSettings.Name);
        try
        {
            if (File.Exists(settingsFilePath))
            {
                var left = Serializable.SMP2(settings);
                var jobj = JsonSerializer.Deserialize<JsonObject>(settingsFilePath);
                var settings_read = jobj![TSettings.Name]!.GetValue<TSettings>();
                var right = Serializable.SMP2(settings_read);
                if (left.SequenceEqual(right))
                    return;
            }
        }
        catch
        {

        }

        using var fs = File.Open(settingsFilePath, FileMode.OpenOrCreate);
        Save(settings, fs);
    }
}
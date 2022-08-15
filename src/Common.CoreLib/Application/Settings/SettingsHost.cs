#if !DBREEZE
using System.Runtime.CompilerServices;

namespace System.Application.Settings;

public abstract class SettingsHostBase
{
    static readonly Dictionary<Type, SettingsHostBase> instances = new();

    public string CategoryName => GetCategoryName();

    protected internal virtual string GetCategoryName() => GetType().Name;

    protected SettingsHostBase()
    {
        instances[GetType()] = this;
    }

    const string ConfigName = "Config" + FileEx.MPO;

    protected static string GetLocalFilePath(string configName) => Path.Combine(IOPath.AppDataDirectory, configName);

    static readonly Lazy<string> _LocalFilePath = new(() => GetLocalFilePath(ConfigName));

    public static string LocalFilePath => _LocalFilePath.Value;

    public static ISerializationProvider Local { get; } = new FileSettingsProvider(LocalFilePath);

    protected static T GetInstance<T>() where T : SettingsHostBase, new()
    {
        return instances.TryGetValue(typeof(T), out var host) ? (T)host : new T();
    }

    protected internal static SerializableProperty<T> GetProperty<T>(Func<string, string> getKey, ISerializationProvider provider, T? defaultValue, bool autoSave, string propertyName) => new(getKey(propertyName), provider, defaultValue) { AutoSave = autoSave };
}

public static class SettingsHost
{
    public static void Save()
    {
        try
        {
            SettingsHostBase.Local.Save();
        }
        catch (Exception ex)
        {
            Log.Error(nameof(SettingsHost), ex, "Config Save");
            throw;
        }
    }

    /// <summary>
    /// 尝试备份配置
    /// </summary>
    /// <param name="bakLocalFilePath"></param>
    static void TryBackup(string bakLocalFilePath)
    {
        try
        {
            var localFilePath = SettingsHostBase.LocalFilePath;
            if (File.Exists(localFilePath))
            {
                File.Copy(localFilePath, bakLocalFilePath, true);
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(SettingsHost), ex, "Config TryBackUp");
        }
    }

    /// <summary>
    /// 尝试还原备份配置，返回是否有备份文件
    /// </summary>
    /// <param name="bakLocalFilePath"></param>
    /// <returns></returns>
    static bool TryRestoreBackup(string bakLocalFilePath)
    {
        try
        {
            var exists = File.Exists(bakLocalFilePath);
            if (exists)
            {
                var localFilePath = SettingsHostBase.LocalFilePath;
                File.Copy(bakLocalFilePath, localFilePath, true);
            }
            return exists;
        }
        catch (Exception ex)
        {
            Log.Error(nameof(SettingsHost), ex, "Config TryRestoreBackup");
        }
        return false;
    }

    /// <summary>
    /// 尝试删除配置
    /// </summary>
    static void TryDelete()
    {
        try
        {
            var localFilePath = SettingsHostBase.LocalFilePath;
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(SettingsHost), ex, "Config TryDelete");
        }
    }

    public static void Load()
    {
        var isLoad = false;
        var bakLocalFilePath = $"{SettingsHostBase.LocalFilePath}.bak";
        try
        {
            SettingsHostBase.Local.Load();
            isLoad = true;

            TryBackup(bakLocalFilePath);
        }
        catch (Exception ex)
        {
            if (!TryRestoreBackup(bakLocalFilePath))
            {
                // 还原备份文件失败，删除当前配置
                TryDelete();
            }

            Log.Error(nameof(SettingsHost), ex, $"Config Load, isLoad: {isLoad}");

            if (!isLoad) SettingsHostBase.Local.Load();
        }
    }
}

/// <summary>
/// 将全部设置项存储在一个文件中的静态实现版本
/// </summary>
/// <typeparam name="TSettings"></typeparam>
public abstract class SettingsHost2<TSettings> : SettingsHostBase where TSettings : SettingsHost2<TSettings>, new()
{
    protected SettingsHost2() : this(false)
    {
    }

    protected SettingsHost2(bool isSupported) : base()
    {
        if (!isSupported) throw new NotSupportedException();
    }

    static readonly Lazy<string> _CategoryName = new(() => typeof(TSettings).Name);

    public static new string CategoryName => _CategoryName.Value;

    static string GetKey(string propertyName) => $"{CategoryName}.{propertyName}";

    public static SerializableProperty<T> GetProperty<T>(T? defaultValue, bool autoSave = true, [CallerMemberName] string propertyName = "") => GetProperty(GetKey, Local, defaultValue, autoSave, propertyName);
}
#endif
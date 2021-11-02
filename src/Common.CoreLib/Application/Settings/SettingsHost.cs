using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace System.Application.Settings
{
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

        protected internal static SerializableProperty<T> GetProperty<T>(Func<string, string> getKey, ISerializationProvider provider, T? defaultValue, bool autoSave, string propertyName) where T : notnull => new(getKey(propertyName), provider, defaultValue) { AutoSave = autoSave };
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
                Log.Error(nameof(SettingsHost), "Config Save", ex);
                throw;
            }
        }

        public static void Load()
        {
            try
            {
                SettingsHostBase.Local.Load();
            }
            catch (Exception ex)
            {
                Log.Error(nameof(SettingsHost), "Config Load", ex);

                File.Delete(SettingsHostBase.LocalFilePath);
                SettingsHostBase.Local.Load();
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

        public static SerializableProperty<T> GetProperty<T>(T? defaultValue, bool autoSave, [CallerMemberName] string propertyName = "") where T : notnull => GetProperty(GetKey, Local, defaultValue, autoSave, propertyName);
    }

    ///// <summary>
    ///// 将当前类的设置项存储在当前类名命名的文件中的实例实现版本
    ///// </summary>
    ///// <typeparam name="TSettings"></typeparam>
    //public abstract class SettingsHost3<TSettings> : SettingsHost2<TSettings> where TSettings : SettingsHost3<TSettings>, new()
    //{
    //    protected SettingsHost3() : base(false)
    //    {
    //        Local = new FileSettingsProvider(LocalFilePath);
    //    }

    //    protected internal override string GetCategoryName() => CategoryName;

    //    static string ConfigName => $"{CategoryName.TrimEnd("Settings", StringComparison.OrdinalIgnoreCase)}Config";

    //    readonly Lazy<string> _LocalFilePath = new(() => GetLocalFilePath(ConfigName + FileEx.MPO));
    //    public new string LocalFilePath => _LocalFilePath.Value;

    //    public new ISerializationProvider Local { get; }

    //    public static TSettings Instance => GetInstance<TSettings>();

    //    string GetKey(string propertyName) => $"{((SettingsHostBase)this).CategoryName}.{propertyName}";

    //    public new SerializableProperty<T> GetProperty<T>(T? defaultValue, bool autoSave, [CallerMemberName] string propertyName = "") where T : notnull => GetProperty(GetKey, Local, defaultValue, autoSave, propertyName);
    //}

    // ----- 示例 -----

    ///// <summary>
    ///// 静态示例，设置项全部存储在一个文件中，路径见 <see cref="SettingsHostBase.LocalFilePath"/>
    ///// </summary>
    //public class DemoSettings : SettingsHost2<DemoSettings>
    //{
    //    /// <summary>
    //    /// 自动检查更新
    //    /// </summary>
    //    public static SerializableProperty<bool> IsAutoCheckUpdate { get; }
    //        = GetProperty(defaultValue: true, autoSave: true);

    //    /// <summary>
    //    /// 使用示例
    //    /// </summary>
    //    public static void UseCase()
    //    {
    //        var value = DemoSettings.IsAutoCheckUpdate.Value;

    //        DemoSettings.IsAutoCheckUpdate.ValueChanged += (_, e) =>
    //        {
    //            var newValue = e.NewValue;
    //        };
    //    }
    //}

    ///// <summary>
    ///// 实例示例，设置项全部存储在以当前类命名的文件中，路径见 <see cref="SettingsHost3{TSettings}.LocalFilePath"/>
    ///// </summary>
    //public class Demo2Settings : SettingsHost3<Demo2Settings>
    //{
    //    public Demo2Settings()
    //    {
    //        IsAutoCheckUpdate = GetProperty(defaultValue: true, autoSave: true);
    //    }

    //    public SerializableProperty<bool> IsAutoCheckUpdate { get; }

    //    /// <summary>
    //    /// 使用示例
    //    /// </summary>
    //    public static void UseCase()
    //    {
    //        var value = Demo2Settings.Instance.IsAutoCheckUpdate.Value;

    //        Demo2Settings.Instance.IsAutoCheckUpdate.ValueChanged += (_, e) =>
    //        {
    //            var newValue = e.NewValue;
    //        };
    //    }
    //}
}
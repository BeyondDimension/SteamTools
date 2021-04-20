using System;
using System.Application.Serialization;
using System.Application.Services;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Application.Models.Settings
{
    public abstract class SettingsHost
    {
        private static readonly Dictionary<Type, SettingsHost> instances = new();
        private readonly Dictionary<string, object> cachedProperties = new();

        protected virtual string CategoryName => GetType().Name;

        public const string ConfigName = "Config.mpo";

        protected SettingsHost()
        {
            instances[GetType()] = this;
        }

        /// <summary>
        /// 请参阅 <see cref="SerializableProperty{T}"/> 缓存在当前实例中获取
        ///  如果没有缓存，根据<see cref="create" />创建它
        /// </summary>
        /// <returns></returns>
        protected SerializableProperty<T> Cache<T>(Func<string, SerializableProperty<T>> create, [CallerMemberName] string propertyName = "")
        {
            var key = CategoryName + "." + propertyName;

            if (cachedProperties.TryGetValue(key, out object obj) && obj is SerializableProperty<T> property1)
                return property1;

            var property = create(key);
            cachedProperties[key] = property;

            return property;
        }

        #region Load / Save

        public static void Load()
        {
            try
            {
                Providers.Local.Load();
            }
            catch (Exception ex)
            {
                Log.Error(nameof(SettingsHost), "Config Load", ex);

                File.Delete(Providers.LocalFilePath);
                Providers.Local.Load();
            }

        }

        public static void Save()
        {
            try
            {
                Providers.Local.Save();
            }
            catch (Exception ex)
            {
                Log.Error(nameof(SettingsHost), "Config Save", ex);
                throw;
            }

        }

        #endregion

        /// <summary>
        /// <typeparamref name ="T" />获取类型的配置对象的唯一实例。
        /// </summary>
        public static T Instance<T>() where T : SettingsHost, new()
        {
            return instances.TryGetValue(typeof(T), out SettingsHost host) ? (T)host : new T();
        }
    }

    public class Providers
    {
        public static string LocalFilePath => Path.Combine(IOPath.AppDataDirectory, SettingsHost.ConfigName);

        public static ISerializationProvider Local { get; } = new FileSettingsProvider(LocalFilePath);
    }
}

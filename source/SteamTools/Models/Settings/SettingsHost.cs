using MetroTrilithon.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SteamTools.Models.Settings
{
    public abstract class SettingsHost
    {
        private static readonly Dictionary<Type, SettingsHost> instances = new Dictionary<Type, SettingsHost>();
        private readonly Dictionary<string, object> cachedProperties = new Dictionary<string, object>();

        protected virtual string CategoryName => this.GetType().Name;

        protected SettingsHost()
        {
            instances[this.GetType()] = this;
        }

        /// <summary>
        /// <请参阅cref ="SerializableProperty {T}" />缓存在当前实例中获取
        ///  如果没有缓存，请根据<see cref ="create"  />创建它
        /// </summary>
        /// <returns></returns>
        protected SerializableProperty<T> Cache<T>(Func<string, SerializableProperty<T>> create, [CallerMemberName] string propertyName = "")
        {
            var key = this.CategoryName + "." + propertyName;

            if (this.cachedProperties.TryGetValue(key, out object obj) && obj is SerializableProperty<T> property1)
                return property1;

            var property = create(key);
            this.cachedProperties[key] = property;

            return property;
        }

        #region Load / Save

        public static void Load()
        {
            try
            {
                Providers.Local.Load();
            }
            catch (Exception)
            {
                File.Delete(Providers.LocalFilePath);
                Providers.Local.Load();
            }

            try
            {
                Providers.Roaming.Load();
            }
            catch (Exception)
            {
                File.Delete(Providers.RoamingFilePath);
                Providers.Roaming.Load();
            }
        }

        public static void Save()
        {
            #region const message

            const string message = @"无法保存配置文件（{0}）。

错误详细信息：{1}";

            #endregion

            try
            {
                Providers.Local.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(message, Providers.LocalFilePath, ex.Message), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            try
            {
                Providers.Roaming.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(message, Providers.RoamingFilePath, ex.Message), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;

namespace System.Application.Serialization
{
	public class ApplicationSettingsProvider : ApplicationSettingsBase, ISerializationProvider
	{
		public static ISerializationProvider Default { get; } = new ApplicationSettingsProvider(typeof(ApplicationSettingsProvider).FullName + "." + nameof(Default));

		public bool IsLoaded { get; private set; }

		// UserScopedSettings 除非至少有一个，否则似乎不会读取LocalFileSettingsProvider
		// ReSharper disable once InconsistentNaming
		// ReSharper disable once UnassignedGetOnlyAutoProperty
		[UserScopedSetting, EditorBrowsable(EditorBrowsableState.Never)]
		public object __Infrastructure { get; }

		public ApplicationSettingsProvider() { }

		public ApplicationSettingsProvider(string settingsKey) : base(settingsKey) { }

		public void SetValue<T>(string key, T value)
		{
			this.AddProperty(key, typeof(T));
			this[key] = value;
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			this.AddProperty(key, typeof(T));

			try
			{
				value = (T)this[key];
				return true;
			}
			catch
			{
				value = default(T);
				return false;
			}
		}

		public bool RemoveValue(string key)
		{
			return this.RemoveProperry(key);
		}

		private void AddProperty(string key, Type type)
		{
			if (this.Properties.OfType<SettingsProperty>().All(x => x.Name != key))
			{
				var property = new SettingsProperty(key)
				{
					PropertyType = type,
					Provider = this.Providers.OfType<SettingsProvider>().FirstOrDefault(),
					SerializeAs = SettingsSerializeAs.Xml,
				};
				property.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());

				this.Properties.Add(property);
				this.Reload();
			}
		}

		private bool RemoveProperry(string key)
		{
			if (this.Properties.OfType<SettingsProperty>().All(x => x.Name != key))
			{
				return false;
			}

			this.Properties.Remove(key);
			this.Reload();

			return true;
		}

		void ISerializationProvider.Load()
		{
			this.Reload();
			this.IsLoaded = true;
		}

		event EventHandler ISerializationProvider.Reloaded
		{
			add { }
			remove { }
		}
	}
}

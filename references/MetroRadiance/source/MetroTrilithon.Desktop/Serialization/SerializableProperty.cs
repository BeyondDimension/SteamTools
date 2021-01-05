using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroTrilithon.Serialization
{
	public sealed class SerializableProperty<T> : SerializablePropertyBase<T>
	{
		public SerializableProperty(string key) : this(key, default(T)) { }

		public SerializableProperty(string key, T defaultValue) : base(key, ApplicationSettingsProvider.Default, defaultValue) { }

		public SerializableProperty(string key, ISerializationProvider provider) : base(key, provider) { }

		public SerializableProperty(string key, ISerializationProvider provider, T defaultValue) : base(key, provider, defaultValue) { }
	}
}

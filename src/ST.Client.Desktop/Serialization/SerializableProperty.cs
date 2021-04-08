using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Application.Serialization
{
	public sealed class SerializableProperty<T> : SerializablePropertyBase<T>
	{
        public SerializableProperty(string key, ISerializationProvider provider) : base(key, provider) { }

		public SerializableProperty(string key, ISerializationProvider provider, T defaultValue) : base(key, provider, defaultValue) { }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace MetroTrilithon.Serialization
{
	public interface ISerializationProvider
	{
		bool IsLoaded { get; }

		void Save();

		void Load();

		/// <summary>
		/// Occurs when the provider side has reloaded due.
		/// </summary>
		event EventHandler Reloaded;

		void SetValue<T>(string key, T value);

		bool TryGetValue<T>(string key, out T value);

		bool RemoveValue(string key);
	}
}

using System.Diagnostics.CodeAnalysis;

namespace System.Application.Serialization
{
    public interface ISerializationProvider
    {
        bool IsLoaded { get; }

        void Save();

        void Load();

        /// <summary>
        /// 在provider已重新加载到期时发生
        /// </summary>
        event EventHandler Reloaded;

        void SetValue<T>(string key, T value);

        bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value) where T : notnull;

        bool RemoveValue(string key);
    }
}
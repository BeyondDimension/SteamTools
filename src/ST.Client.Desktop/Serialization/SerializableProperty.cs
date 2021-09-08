namespace System.Application.Serialization
{
    public sealed class SerializableProperty<T> : SerializablePropertyBase<T> where T : notnull
    {
        public SerializableProperty(string key, ISerializationProvider provider) : base(key, provider)
        {
        }

        public SerializableProperty(string key, ISerializationProvider provider, T? defaultValue) : base(key, provider, defaultValue)
        {
        }
    }
}
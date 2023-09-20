using NJsonConvert = Newtonsoft.Json.JsonConvert;
using NJsonConverter = Newtonsoft.Json.JsonConverter;
using NJsonReader = Newtonsoft.Json.JsonReader;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;
using NJsonWriter = Newtonsoft.Json.JsonWriter;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void InitNJsonSerializer(HashSet<Type> types)
    {
        NJsonConverterImpl converter = new(types);
        NJsonConvert.DefaultSettings = () => new NJsonSerializerSettings()
        {
            Converters = { converter, },
        };
    }

    sealed class NJsonConverterImpl : NJsonConverter
    {
        readonly HashSet<Type> types;

        internal NJsonConverterImpl(HashSet<Type> types)
        {
            this.types = types;
        }

        public override bool CanConvert(Type objectType)
        {
            if (types.Contains(objectType))
            {
                return true;
            }
            if (objectType.IsClass)
            {
                if (objectType.Namespace?.Contains("Models",
                    StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    if (typeof(IMemoryPackable<>)
                        .MakeGenericType(objectType)
                        .IsAssignableFrom(objectType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override object? ReadJson(NJsonReader reader, Type objectType, object? existingValue, NJsonSerializer serializer)
        {
            try
            {
                var bytes = reader.ReadAsBytes();
                if (bytes == null)
                    return existingValue;
                return Serializable.DMP2(objectType, bytes);
            }
            catch
            {
                if (objectType?.IsValueType ?? false)
                {
                    return Activator.CreateInstance(objectType);
                }
                return null;
            }
        }

        public override void WriteJson(NJsonWriter writer, object? value, NJsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                var bytes = Serializable.SMP2(value.GetType(), value);
                writer.WriteValue(bytes);
            }
        }
    }
}
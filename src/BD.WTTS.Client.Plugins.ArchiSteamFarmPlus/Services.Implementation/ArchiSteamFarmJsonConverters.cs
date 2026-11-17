using System.Collections;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonException = System.Text.Json.JsonException;

namespace BD.WTTS.Services.Implementation;

// ASF accepts enum names in imported configuration, but its wire format writes numbers.
internal sealed class AsfEnumConverter<T> : System.Text.Json.Serialization.JsonConverter<T> where T : struct, Enum
{
    readonly System.Text.Json.Serialization.JsonConverter<T> numeric;
    readonly System.Text.Json.Serialization.JsonConverter<T> named;

    internal AsfEnumConverter(JsonSerializerOptions options)
    {
        numeric = (System.Text.Json.Serialization.JsonConverter<T>)new JsonNumberEnumConverter<T>().CreateConverter(typeof(T), options)!;
        named = (System.Text.Json.Serialization.JsonConverter<T>)new JsonStringEnumConverter<T>().CreateConverter(typeof(T), options)!;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => named.Read(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => numeric.Write(writer, value, options);
}

// The endpoint defines an ordered string-to-string map, not a polymorphic CLR object graph.
internal sealed class AsfOrderedDictionaryConverter : System.Text.Json.Serialization.JsonConverter<OrderedDictionary>
{
    public override OrderedDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected a key-to-name map.");

        var result = new OrderedDictionary();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return result;
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected a key.");
            var key = reader.GetString()!;
            if (!reader.Read() || reader.TokenType is not (JsonTokenType.String or JsonTokenType.Null))
                throw new JsonException("Expected a game name.");
            result[key] = reader.GetString();
        }
        throw new JsonException("Incomplete key-to-name map.");
    }

    public override void Write(Utf8JsonWriter writer, OrderedDictionary value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (DictionaryEntry entry in value)
        {
            if (entry.Key is not string key || entry.Value is not (string or null))
                throw new JsonException("Game keys and names must be strings.");
            writer.WriteString(key, (string?)entry.Value);
        }
        writer.WriteEndObject();
    }
}

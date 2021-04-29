using System;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Application.Utils
{
    public class LongToDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (Utf8Parser.TryParse(reader.ValueSpan, out long value, out _))
                return DateTime.UnixEpoch.AddMilliseconds(value);

            throw new FormatException();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(
                JsonEncodedText.Encode(((long) (value - DateTime.UnixEpoch).TotalMilliseconds).ToString()));
        }
    }
}
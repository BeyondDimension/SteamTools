#if WTTS_NATIVE_AOT
using System.Text.Json;
using System.Text.Json.Serialization;
using dotnetCampus.Ipc.Serialization;

namespace BD.WTTS.Serialization;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true,
    Converters = new[] { typeof(IpcCancellationTokenJsonConverter) })]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(IEnumerable<string>))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(bool?))]
[JsonSerializable(typeof(CancellationToken))]
[JsonSerializable(typeof(ToastLength))]
[JsonSerializable(typeof(IPCToastService.ToastIcon))]
[JsonSerializable(typeof(IPCToastService.ToastText))]
#if !LIB_CLIENT_IPC
[JsonSerializable(typeof(OperationResult))]
[JsonSerializable(typeof(OperationResult<List<(string ip, string domain)>>))]
[JsonSerializable(typeof(IEnumerable<(string ip, string domain)>))]
[JsonSerializable(typeof(IReadOnlyDictionary<string, string>))]
#endif
internal sealed partial class PlatformIpcJsonSerializerContext : JsonSerializerContext
{
    [ModuleInitializer]
    internal static void Register()
    {
        GeneratedIpcJsonSerializer.RegisterContext(Default);
        MemoryPackFormatterProvider.Register<SubProcessArgumentIndex2Model>();
        MemoryPackFormatterProvider.Register(System.Runtime.Serialization.Formatters.ProcessStartInfoFormatterAttribute.Formatter.Default);
    }
}

// A token transfers its cancellation state, not an OS wait handle owned by another process.
internal sealed class IpcCancellationTokenJsonConverter : System.Text.Json.Serialization.JsonConverter<CancellationToken>
{
    public override CancellationToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return new CancellationToken(document.RootElement.TryGetProperty(nameof(CancellationToken.IsCancellationRequested), out var canceled)
            && canceled.GetBoolean());
    }

    public override void Write(Utf8JsonWriter writer, CancellationToken value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean(nameof(CancellationToken.IsCancellationRequested), value.IsCancellationRequested);
        writer.WriteBoolean(nameof(CancellationToken.CanBeCanceled), value.CanBeCanceled);
        writer.WriteEndObject();
    }
}
#endif

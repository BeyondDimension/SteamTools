#if WTTS_NATIVE_AOT
using System.Text.Json.Serialization;
using dotnetCampus.Ipc.Serialization;

namespace BD.WTTS.Serialization;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(StartProxyResult))]
[JsonSerializable(typeof(StartProxyResultCode))]
[JsonSerializable(typeof(ProxyLogUpdate))]
[JsonSerializable(typeof(IReadOnlyCollection<ScriptIPCDTO>))]
internal sealed partial class AcceleratorIpcJsonSerializerContext : JsonSerializerContext
{
    [ModuleInitializer]
    internal static void Register()
    {
        GeneratedIpcJsonSerializer.RegisterContext(Default);
        MemoryPackFormatterProvider.Register<ReverseProxySettings>();
        MemoryPackFormatterProvider.Register<FlowStatistics>();
        MemoryPackFormatterProvider.Register<ScriptIPCDTO>();
        MemoryPackFormatterProvider.Register<AccelerateProjectDTO>();
        MemoryPackFormatterProvider.Register(X509CertificatePackableFormatterAttribute.Formatter.Default);
    }
}
#endif

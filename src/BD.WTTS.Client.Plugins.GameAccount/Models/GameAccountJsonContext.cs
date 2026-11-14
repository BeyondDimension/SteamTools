using System.Text.Json.Serialization;

namespace BD.WTTS.Models;

// Keep the bundled platform schema's numeric enums and constructor-based initialization.
// Commands and other read-only runtime state are not part of these JSON contracts.
[JsonSourceGenerationOptions(IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(PlatformAccount[]))]
[JsonSerializable(typeof(Dictionary<string, string>), TypeInfoPropertyName = "AccountDictionary")]
internal sealed partial class GameAccountJsonContext : JsonSerializerContext
{
}

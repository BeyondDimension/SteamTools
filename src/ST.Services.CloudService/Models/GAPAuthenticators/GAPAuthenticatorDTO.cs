using MessagePack;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models;

/// <inheritdoc cref="IGAPAuthenticatorDTO"/>
[MessagePackObject(keyAsPropertyName: true)]
public sealed partial class GAPAuthenticatorDTO : IGAPAuthenticatorDTO, IExplicitHasValue
{
    [MPIgnore, N_JsonIgnore, S_JsonIgnore]
    public ushort Id { get; set; }

    public int Index { get; set; }

    public string Name { get; set; } = string.Empty;

    [MPIgnore, N_JsonIgnore, S_JsonIgnore]
    public GamePlatform Platform => Value == null ? default : Value.Platform;

    public Guid? ServerId { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset LastUpdate { get; set; }

    public IGAPAuthenticatorValueDTO? Value { get; set; }

    bool IExplicitHasValue.ExplicitHasValue()
    {
        return !string.IsNullOrEmpty(Name) && Value != null;
    }
}
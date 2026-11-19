using BD.WTTS.Models.Abstractions;
using MessagePack.Resolvers;
using WinAuth;

namespace BD.WTTS.Serialization;

[GeneratedMessagePackResolver]
internal partial class AuthenticatorVaultResolver;

internal static class AuthenticatorPayloadSerializer
{
    // Retain the existing MessagePack union IDs, map keys and LZ4 envelope.
    // Explicit resolvers keep vault I/O independent of runtime-generated formatters.
    static readonly MessagePackSerializerOptions options = Serializable.lz4Options
        .WithSecurity(MessagePackSecurity.UntrustedData)
        .WithResolver(CompositeResolver.Create(
            [new MessagePack.Formatters.ArrayFormatter<AccountPlatformAuthenticator>(), new MessagePack.Formatters.ListFormatter<AccountPlatformAuthenticator>()],
            [WinAuthMessagePackResolver.Instance, AuthenticatorVaultResolver.Instance, BuiltinResolver.Instance]));

    public static IAuthenticatorValueDTO? ReadValue(byte[] data) =>
        MessagePackSerializer.Deserialize<IAuthenticatorValueDTO>(data, options);

    public static byte[] WriteValue(IAuthenticatorValueDTO value) => MessagePackSerializer.Serialize(value, options);

    public static AccountPlatformAuthenticator[]? ReadBackup(byte[] data) =>
        MessagePackSerializer.Deserialize<AccountPlatformAuthenticator[]>(data, options);

    public static byte[] WriteBackup(List<AccountPlatformAuthenticator> rows) => MessagePackSerializer.Serialize(rows, options);

    public static Task WriteBackupAsync(Stream stream, List<AccountPlatformAuthenticator> rows) =>
        MessagePackSerializer.SerializeAsync(stream, rows, options);
}

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

[Obsolete]
[Mobius(Obsolete = true)]
public sealed class EmptyLocalDataProtectionProvider : ILocalDataProtectionProvider
{
    public ValueTask<byte[]?> DB(byte[]? value)
    {
        return new(value);
    }

    public ValueTask<byte[]?> EB(byte[]? value)
    {
        return new(value);
    }
}
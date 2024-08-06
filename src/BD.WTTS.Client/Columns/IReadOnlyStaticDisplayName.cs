// ReSharper disable once CheckNamespace
namespace BD.WTTS.Columns;

[Mobius(Obsolete = true)]
[Obsolete]
public interface IReadOnlyStaticDisplayName
{
    static abstract string? DisplayName { get; }
}

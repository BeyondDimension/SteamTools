// ReSharper disable once CheckNamespace
namespace BD.WTTS.Columns;

[Mobius(Obsolete = true)]
public interface IReadOnlyStaticDisplayName
{
    static abstract string? DisplayName { get; }
}

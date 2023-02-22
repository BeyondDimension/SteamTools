namespace BD.WTTS.Markup.Abstractions;

public interface IMarkupExtension<T>
{
    T ProvideValue(IServiceProvider serviceProvider);
}
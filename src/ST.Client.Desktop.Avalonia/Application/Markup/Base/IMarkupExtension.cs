namespace System.Application.Markup
{
    public interface IMarkupExtension<T>
    {
        T ProvideValue(IServiceProvider serviceProvider);
    }
}

namespace System.Application.Markup
{
    public abstract class MarkupExtension<T> : IMarkupExtension<T>
    {
        protected string member;

        public MarkupExtension(string member) => this.member = member;

        public abstract T ProvideValue(IServiceProvider serviceProvider);
    }
}

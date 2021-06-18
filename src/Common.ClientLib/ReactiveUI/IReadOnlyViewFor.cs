// ReSharper disable once CheckNamespace
namespace ReactiveUI
{
    public interface IReadOnlyViewFor<out T> where T : class
    {
        T? ViewModel { get; }
    }
}
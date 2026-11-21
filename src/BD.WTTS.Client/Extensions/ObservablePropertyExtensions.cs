using System.ComponentModel;

namespace BD.WTTS;

public static class ObservablePropertyExtensions
{
    // The name filters notifications; only the typed delegate reads the property.
    public static IObservable<TValue> ObserveValue<TSource, TValue>(this TSource source, string propertyName, Func<TSource, TValue> getter)
        where TSource : INotifyPropertyChanged
        => Observable.Create<TValue>(observer =>
        {
            void OnChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName)
                    observer.OnNext(getter(source));
            }

            source.PropertyChanged += OnChanged;
            try
            {
                observer.OnNext(getter(source));
            }
            catch
            {
                source.PropertyChanged -= OnChanged;
                throw;
            }
            return Disposable.Create(() => source.PropertyChanged -= OnChanged);
        }).DistinctUntilChanged();
}

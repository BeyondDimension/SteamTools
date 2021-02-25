using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Application.Mvvm
{
    internal static class EventArgsFactory
    {
        [NotNull]
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs>
            PropertyChangedEventArgsDictionary = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        public static PropertyChangedEventArgs GetPropertyChangedEventArgs([DisallowNull] string propertyName)
        {
            return PropertyChangedEventArgsDictionary.GetOrAdd(propertyName ?? string.Empty,
                name => new PropertyChangedEventArgs(name));
        }
    }
}

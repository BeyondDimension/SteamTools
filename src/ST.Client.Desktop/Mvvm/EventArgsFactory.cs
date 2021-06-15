using System.Collections.Concurrent;
using System.ComponentModel;

namespace System.Application.Mvvm
{
    internal static class EventArgsFactory
    {
        static readonly ConcurrentDictionary<string, PropertyChangedEventArgs>
            PropertyChangedEventArgsDictionary = new();

        public static PropertyChangedEventArgs GetPropertyChangedEventArgs(string? propertyName)
        {
            return PropertyChangedEventArgsDictionary.GetOrAdd(propertyName ?? string.Empty,
                name => new PropertyChangedEventArgs(name));
        }
    }
}
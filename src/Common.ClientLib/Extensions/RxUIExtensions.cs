using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace ReactiveUI
{
    public static class RxUIExtensions
    {
        public static bool RaiseAndSetIfChangedReturnIsNotChange<TObj, TRet>(
            this TObj reactiveObject,
            ref TRet backingField,
            TRet newValue,
            [CallerMemberName] string? propertyName = null) where TObj : IReactiveObject
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            var r = EqualityComparer<TRet>.Default.Equals(backingField, newValue);

            if (r)
            {
                return true; // value Equals
            }

            reactiveObject.RaisingPropertyChanging(propertyName);
            backingField = newValue;
            reactiveObject.RaisingPropertyChanged(propertyName);
            return false; // value Changed
        }

        public static IDisposable SubscribeInMainThread<T>(this IObservable<T> source, Action<T> onNext) => source.ObserveOn(RxApp.MainThreadScheduler).Subscribe(onNext);
    }
}
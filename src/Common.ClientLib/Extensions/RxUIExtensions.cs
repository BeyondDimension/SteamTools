using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DynamicData.Binding;

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

        //public static IDisposable WhenAnyValue2SubscribeInMainThread<TSender, TRet>(this TSender sender, Expression<Func<TSender, TRet>> property1, Action<TRet> onNext) where TSender : INotifyPropertyChanged
        //{
        //    onNext(property1.Compile()(sender));
        //    return sender.WhenValueChanged(property1, notifyOnInitialValue: false)!
        //         .SubscribeInMainThread(onNext);
        //}
    }
}
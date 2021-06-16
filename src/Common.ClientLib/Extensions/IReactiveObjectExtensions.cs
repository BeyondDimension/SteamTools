using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUI;

// ReSharper disable once CheckNamespace
namespace ReactiveUI
{
    public static class IReactiveObjectExtensions
    {
        public static bool RaiseAndSetIfChanged2<TObj, TRet>(
            this TObj reactiveObject,
            ref TRet backingField,
            TRet newValue,
            [CallerMemberName] string propertyName = null) where TObj : IReactiveObject
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            var r = EqualityComparer<TRet>.Default.Equals(backingField, newValue);

            if (r)
            {
                return true;
            }

            reactiveObject.RaisingPropertyChanging(propertyName);
            backingField = newValue;
            reactiveObject.RaisingPropertyChanged(propertyName);
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using Object = Java.Lang.Object;

// ReSharper disable once CheckNamespace
namespace Android.Runtime
{
    /// <summary>
    /// Java 泛型 助手类
    /// </summary>
    public static class JavaGenericHelper
    {
        /// <summary>
        /// <see cref="Java.Util.Arrays.AsList(Object[])"/> 代替方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <returns></returns>
        public static IList<T> AsList<T>(params T[] a) => new JavaList<T>(a);

        /// <summary>
        /// 适用于实现接口 <see cref="IParcelableCreator.NewArray(int)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static Object[] NewArray<T>(T[] ts) where T : IJavaObject
            => Array.ConvertAll(ts, x => Extensions.JavaCast<Object>(x));
    }
}
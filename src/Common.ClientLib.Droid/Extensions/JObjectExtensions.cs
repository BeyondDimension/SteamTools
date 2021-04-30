using Android.Runtime;
using System.Collections.Generic;
using System.Linq;
using ArrayList = Java.Util.ArrayList;
using ICollection = System.Collections.ICollection;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class JObjectExtensions
    {
        /// <summary>
        /// 将 <see cref="IJavaObject"/> 转换为泛型类型，常用于修复绑定库中泛型丢失导致的问题
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Cast<T>(this IJavaObject obj)
        {
            if (obj is T t)
            {
                return t;
            }
            else
            {
                var msg = $"Java generic binding conversion failed, obj_type: {obj?.GetType()}, t_type: {typeof(T)}.";
                throw new InvalidCastException(msg);
            }
        }

        public static ArrayList ToJavaList<T>(this IEnumerable<T> e)
        {
            ICollection c;
            if (e is ICollection c1) c = c1;
            else c = e.ToArray();
            return new ArrayList(c);
        }
    }
}
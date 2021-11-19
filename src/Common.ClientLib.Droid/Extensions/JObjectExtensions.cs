using Android.Runtime;
using System.Collections;

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

        public static ICollection ToJavaCollection(this IEnumerable e)
        {
            if (e is ICollection collection)
            {
                return collection;
            }
            else
            {
                return new JavaList(e);
            }
        }
    }
}
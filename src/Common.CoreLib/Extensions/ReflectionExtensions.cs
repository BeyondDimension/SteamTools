using System.Reflection;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

public static class ReflectionExtensions
{
    const string TAG = "ReflectionEx";

    /// <summary>
    /// 检索应用于指定程序集的指定类型的自定义特性。
    /// <para>如果发生 <see cref="FileNotFoundException"/> 将返回 <see langword="null"/></para>
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="attrType"></param>
    /// <returns></returns>
    public static Attribute[]? GetCustomAttributesSafe(this Assembly assembly, Type attrType)
    {
        try
        {
            return assembly.GetCustomAttributes(attrType).ToArray();
        }
        catch (FileNotFoundException)
        {
            // Sometimes the previewer doesn't actually have everything required for these loads to work
            Log.Warn(TAG, "Could not load assembly: {0} for Attribute {1} | Some renderers may not be loaded", assembly.FullName, attrType.FullName);
        }

        return null;
    }

    public static T GetRequiredCustomAttribute<T>(this Assembly assembly) where T : Attribute
    {
        var requiredCustomAttribute = assembly.GetCustomAttribute<T>();
        if (requiredCustomAttribute == null)
            throw new NullReferenceException(nameof(requiredCustomAttribute));
        return requiredCustomAttribute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsNullableType(this Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// 类型是否为 <see cref="Nullable{T}"/> 可空类型
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullable(this Type t)
    {
        if (t.IsValueType) // 值类型(struct)进行可空判断
        {
            return IsNullableType(t);
        }
        return true; // 引用类型(class)都可空
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsStatic(this Type t) => t.IsAbstract && t.IsSealed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeCode GetTypeCode(this Type type)
    {
        return Type.GetTypeCode(type);
    }
}
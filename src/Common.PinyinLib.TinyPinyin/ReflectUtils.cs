#if MONOANDROID || ANDROID
using System;

namespace TinyPinyin
{
    internal static class ReflectUtils
    {
        public static Func<T?> GetStaticField<T>(string fieldName) where T : notnull
        {
            var field = Java.Lang.Class.FromType(typeof(PinyinHelper)).GetDeclaredField(fieldName);
            if (field != null)
            {
                return () =>
                {
                    if (field.Get(null) is T value)
                    {
                        return value;
                    }
                    return default;
                };
            }
            return () => default;
        }
    }
}
#endif